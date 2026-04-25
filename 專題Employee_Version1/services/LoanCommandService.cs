using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ServiceModels;
using 專題Employee_Version1.Models.ViewModels;

namespace 專題Employee_Version1.services
{
    public class LoanCommandService
    {
        private readonly Version3_LoanEntities5 _dbLoan;
        private readonly Version3_CustomerEntities1 _dbCustomer;
        private readonly string _connectionString;

        public LoanCommandService(
            Version3_LoanEntities5 dbLoan,
            Version3_CustomerEntities1 dbCustomer,
            string connectionString)
        {
            _dbLoan = dbLoan;
            _dbCustomer = dbCustomer;
            _connectionString = connectionString;
        }

        public void UpdateLoanProduct(LoanProductViewModels loan, string imageFileName)
        {
            var finalImageFileName = imageFileName;
            if (string.IsNullOrEmpty(finalImageFileName))
            {
                finalImageFileName = _dbLoan.LoanProducts
                    .Where(x => x.LoanProductID == loan.LoanProductID)
                    .Select(x => x.ImageFileName)
                    .FirstOrDefault();
            }

            var loanProduct = new LoanProduct
            {
                LoanProductID = loan.LoanProductID,
                ProductName = loan.ProductName,
                InterestRate = loan.InterestRate,
                LoanTerm = loan.LoanTerm,
                MaxLoanAmount = loan.MaxLoanAmount,
                MinLoanAmount = loan.MinLoanAmount,
                ProductDescription = loan.ProductDescription,
                ImageFileName = finalImageFileName
            };

            _dbLoan.Entry(loanProduct).State = System.Data.Entity.EntityState.Modified;
            _dbLoan.SaveChanges();
        }

        public async Task<LoanCommandResult> CreateProductAsync(
            LoanProductViewModels product,
            Stream imageStream,
            string imageFileName)
        {
            if (imageStream == null || string.IsNullOrEmpty(imageFileName))
            {
                return new LoanCommandResult
                {
                    Success = false,
                    Message = "請上傳圖片檔案。"
                };
            }

            var azureBlobService = new AzureBlobService(_connectionString, "sharefolder");
            var uploadResult = await azureBlobService.UploadFileAsync(imageFileName, imageStream);

            if (uploadResult != "Upload successful")
            {
                return new LoanCommandResult
                {
                    Success = false,
                    Message = uploadResult
                };
            }

            var loanProduct = new LoanProduct
            {
                ProductName = product.ProductName,
                InterestRate = product.InterestRate,
                LoanTerm = product.LoanTerm,
                MaxLoanAmount = product.MaxLoanAmount,
                MinLoanAmount = product.MinLoanAmount,
                ProductDescription = product.ProductDescription,
                ImageFileName = imageFileName
            };

            _dbLoan.LoanProducts.Add(loanProduct);
            await _dbLoan.SaveChangesAsync();

            return new LoanCommandResult { Success = true };
        }

        public bool DeleteLoanProduct(int id)
        {
            var loan = _dbLoan.LoanProducts.Find(id);
            if (loan == null)
            {
                return false;
            }

            _dbLoan.LoanProducts.Remove(loan);
            _dbLoan.SaveChanges();
            return true;
        }

        public LoanCommandResult ConfirmLoanApplication(int id)
        {
            var loanApplication = _dbLoan.LoanApplications.Find(id);
            if (loanApplication == null)
            {
                return new LoanCommandResult { Success = false, Message = "LoanApplicationNotFound" };
            }

            if (loanApplication.LoanStatus != "Pending")
            {
                return new LoanCommandResult { Success = false, Message = "LoanApplicationNotPending" };
            }

            loanApplication.LoanStatus = "Confirmed";
            _dbLoan.Entry(loanApplication).State = System.Data.Entity.EntityState.Modified;

            var repaymentAccount = CreateRepaymentAccount(loanApplication.LoanAmount);
            _dbLoan.RepaymentAccounts.Add(repaymentAccount);
            _dbLoan.SaveChanges();

            loanApplication.RepaymentAccountID = repaymentAccount.RepaymentAccountID;
            _dbLoan.Entry(loanApplication).State = System.Data.Entity.EntityState.Modified;

            GenerateRepaymentSchedules(id);
            _dbLoan.SaveChanges();

            ImportAccountTransaction(loanApplication.DisbursementAccount, loanApplication.LoanAmount);

            return new LoanCommandResult { Success = true };
        }

        public LoanCommandResult RejectLoanApplication(int id)
        {
            var loanApplication = _dbLoan.LoanApplications.Find(id);
            if (loanApplication == null)
            {
                return new LoanCommandResult { Success = false, Message = "LoanApplicationNotFound" };
            }

            if (loanApplication.LoanStatus != "Pending")
            {
                return new LoanCommandResult { Success = false, Message = "LoanApplicationNotPending" };
            }

            loanApplication.LoanStatus = "Rejected";
            _dbLoan.Entry(loanApplication).State = System.Data.Entity.EntityState.Modified;
            _dbLoan.SaveChanges();
            return new LoanCommandResult { Success = true };
        }

        public bool ToggleRepaymentScheduleStatus(int id)
        {
            var repaymentSchedule = _dbLoan.RepaymentSchedules.Find(id);
            if (repaymentSchedule == null)
            {
                return false;
            }

            repaymentSchedule.RepaymentStatus = repaymentSchedule.RepaymentStatus == "Pending" ? "Paid" : "Pending";
            _dbLoan.Entry(repaymentSchedule).State = System.Data.Entity.EntityState.Modified;
            _dbLoan.SaveChanges();
            return true;
        }

        public byte[] ExportLoanApplicationsCsv()
        {
            var loanApplications = _dbLoan.LoanApplications.ToList();
            var csv = new StringBuilder();
            csv.AppendLine("LoanApplicationID,CustomerID,LoanAmount,LoanStatus");

            foreach (var loan in loanApplications)
            {
                csv.AppendLine(loan.LoanApplicationID + "," + loan.CustomerID + "," + loan.LoanAmount + "," + loan.LoanStatus);
            }

            return Encoding.ASCII.GetBytes(csv.ToString());
        }

        public async Task<byte[]> DownloadEconomicProofAsync(string fileName)
        {
            var azureBlobService = new AzureBlobService(_connectionString, "ecommer");
            var stream = await azureBlobService.DownloadFileAsync(fileName);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private RepaymentAccount CreateRepaymentAccount(decimal totalRepaymentAmount)
        {
            return new RepaymentAccount
            {
                AccountNumber = Guid.NewGuid().ToString(),
                TotalRepaymentAmount = totalRepaymentAmount,
                AmountPaid = 0,
                CreatedDate = DateTime.Now
            };
        }

        private void ImportAccountTransaction(string accountNumber, decimal amount)
        {
            var customerAccount = _dbCustomer.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (customerAccount == null)
            {
                throw new Exception("Customer account with number '" + accountNumber + "' not found.");
            }

            customerAccount.Balance += amount;
            var transaction = new Transaction
            {
                TransactionDate = DateTime.Now,
                TransactionType = "Loan Credit",
                Amount = amount,
                Description = "Loan Disbursement",
                AccountID = customerAccount.AccountID,
                BalanceAfterTransaction = customerAccount.Balance
            };
            _dbCustomer.Transactions.Add(transaction);
            _dbCustomer.SaveChanges();
        }

        private void GenerateRepaymentSchedules(int loanApplicationID)
        {
            using (var context = new Version3_LoanEntities2())
            {
                var command = context.Database.Connection.CreateCommand();
                command.CommandText = "GenerateRepaymentSchedules";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@LoanApplicationID", loanApplicationID));

                context.Database.Connection.Open();
                command.ExecuteNonQuery();
                context.Database.Connection.Close();
            }
        }
    }
}
