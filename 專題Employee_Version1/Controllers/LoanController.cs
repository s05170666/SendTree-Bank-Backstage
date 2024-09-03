using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ViewModels;
using System.Data.Entity;
using System.Security.Cryptography.Xml;
using System.Web.UI.WebControls;
using System.Text;
using 專題Employee_Version1.services;
using System.Threading.Tasks;
using System.Configuration;


namespace 專題Employee_Version1.Controllers
{

    public class LoanController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private readonly AzureBlobService _azureBlobService;
        private Version3_CustomerEntities1 _dbCustomer = new Version3_CustomerEntities1();
        private Version3_LoanEntities5 _dbLoan3 = new Version3_LoanEntities5();

        public ActionResult Index()
        {
            var loanApplications = _dbLoan3.LoanApplications.ToList();
            ViewBag.AllCount = loanApplications.Count;
            ViewBag.PendingCount = loanApplications.Count(x => x.LoanStatus == "Pending");
            ViewBag.ConfirmedCount = loanApplications.Count(x => x.LoanStatus == "Confirmed" || x.LoanStatus == "Rejected");

            return View(loanApplications);
        }

        //貸款商品列表
        public ActionResult LoanList()
        {
            var viewModel = new LoanViewModel
            {
                LoanProducts = _dbLoan3.LoanProducts.ToList(),
                NewLoanProductViewModel = new LoanProductViewModels
                {

                    ProductName = "這是測試用產品",
                    InterestRate = 8,
                    LoanTerm = 36,
                    MaxLoanAmount = 500000,
                    MinLoanAmount = 100000,
                    ProductDescription = "這是測試用的敘述"
                }

            };

            return View(viewModel);
        }
        //貸款商品修改
        public ActionResult Edit(int id)
        {
            var loan = _dbLoan3.LoanProducts.Find(id);

            var loanViewModel = new LoanProductViewModels
            {
                LoanProductID = loan.LoanProductID,
                ProductName = loan.ProductName,
                InterestRate = loan.InterestRate,
                LoanTerm = loan.LoanTerm,
                MaxLoanAmount = loan.MaxLoanAmount,
                MinLoanAmount = loan.MinLoanAmount,
                ProductDescription = loan.ProductDescription,

            };
            return View(loanViewModel);
        }

        [HttpPost]
        public ActionResult Edit(LoanProductViewModels loan)
        {
            if (ModelState.IsValid)
            {
                var file = loan.ImageFile;
                var imagefileNames = loan.ImageFile.FileName;

                if (file != null)
                {
                    var path = Path.Combine(Server.MapPath("~/ImageFiles/"), imagefileNames);
                    file.SaveAs(path);
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
                    ImageFileName = imagefileNames
                };

                _dbLoan3.Entry(loanProduct).State = System.Data.Entity.EntityState.Modified;
                return RedirectToAction("LoanList");
            }
            return View("LoanList");
        }

        //貸款商品新增
        [HttpPost]
        public async Task<ActionResult> CreateProduct(LoanViewModel loan)
        {
            if (ModelState.IsValid)
            {
                var file = loan.NewLoanProductViewModel.ImageFile;

                if (file != null && file.ContentLength > 0)
                {
                    string imageFileName = Path.GetFileName(file.FileName);

                    using (var stream = file.InputStream)
                    {
                        // 初始化 AzureBlobService，傳入 Azure Blob Storage 的連接字串和容器名稱
                        string containerName = "sharefolder";
                        AzureBlobService azureBlobService = new AzureBlobService(connectionString, containerName);

                        // 上傳圖片到 Azure Blob Storage 並取得結果訊息
                        string uploadResult = await azureBlobService.UploadFileAsync(imageFileName, stream);

                        if (uploadResult == "Upload successful")
                        {
                            // 保存 LoanProduct 到資料庫
                            var loanProduct = new LoanProduct
                            {
                                ProductName = loan.NewLoanProductViewModel.ProductName,
                                InterestRate = loan.NewLoanProductViewModel.InterestRate,
                                LoanTerm = loan.NewLoanProductViewModel.LoanTerm,
                                MaxLoanAmount = loan.NewLoanProductViewModel.MaxLoanAmount,
                                MinLoanAmount = loan.NewLoanProductViewModel.MinLoanAmount,
                                ProductDescription = loan.NewLoanProductViewModel.ProductDescription,
                                ImageFileName = imageFileName // 保存圖片檔案名稱到資料庫
                            };

                            _dbLoan3.LoanProducts.Add(loanProduct);
                            await _dbLoan3.SaveChangesAsync();

                            return RedirectToAction("LoanList");
                        }
                        else
                        {
                            // 處理上傳失敗的情況
                            ModelState.AddModelError("", uploadResult);
                        }
                    }
                }
            }

            // 如果 ModelState 不合法，返回到表單頁面或其他處理邏輯
            return View("LoanList");
        }



        //貸款商品刪除
        public ActionResult Delete(int id)
        {
            var loan = _dbLoan3.LoanProducts.Find(id);
            if (loan == null)
            {
                return HttpNotFound();
            }
            _dbLoan3.LoanProducts.Remove(loan);
            _dbLoan3.SaveChanges();
            return RedirectToAction("LoanList");
        }

        //所有貸款申請
        public ActionResult AllLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.ToList();
            return PartialView("_AllLoanApplications", loanApplications);
        }

        //待處理貸款申請
        public ActionResult PendingLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.Where(x => x.LoanStatus == "Pending").ToList();
            return PartialView("_PendingLoanApplications", loanApplications);
        }

        //已確認貸款申請
        public ActionResult ConfirmedLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.Where(x => x.LoanStatus == "Confirmed" || x.LoanStatus == "Rejected").ToList();
            return PartialView("_ConfirmedLoanApplications", loanApplications);
        }

        //貸款申請詳細資料
        public ActionResult LoanApplicationDetail(int id)
        {
            var loanApplication = _dbLoan3.LoanApplications.Find(id);
            var customer = _dbLoan3.CustomersInLoans.Find(loanApplication.CustomerID);
            var loanApplicationViewMdoel = new LoanApplicationViewModel
            {
                LoanApplicationID = loanApplication.LoanApplicationID,
                ProductName = loanApplication.LoanProduct.ProductName,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                ApplicationDate = loanApplication.ApplicationDate,
                LoanAmount = loanApplication.LoanAmount,
                LoanStatus = loanApplication.LoanStatus,
                DisbursementAccount = loanApplication.DisbursementAccount,
                RepaymentMonths = loanApplication.RepaymentMonths,
                TotalRepaymentAmount = loanApplication.TotalRepaymentAmount,
                InterestRate = loanApplication.InterestRate,
                EconomicProof = loanApplication.EconomicProof


            };

            if (loanApplication.LoanStatus == "Confirmed")
            {
                var repaymentAccount = _dbLoan3.RepaymentAccounts.Find(loanApplication.RepaymentAccountID);
                var repaymentSchedules = _dbLoan3.RepaymentSchedules
                               .Where(rs => rs.LoanApplicationID == id)
                               .OrderBy(rs => rs.RepaymentDate)
                               .ToList();
                loanApplicationViewMdoel.RepaymentSchedules = repaymentSchedules;
                loanApplicationViewMdoel.RepaymentAccount = repaymentAccount.AccountNumber;

                var totalPaidAmount = _dbLoan3.RepaymentSchedules
                    .Where(rs => rs.LoanApplicationID == id && rs.RepaymentStatus == "Paid")
                    .Select(rs => rs.RepaymentAmount)
                    .DefaultIfEmpty()
                    .Sum();
                var countPaidAmount = _dbLoan3.RepaymentSchedules
                    .Where(rs => rs.LoanApplicationID == id && rs.RepaymentStatus == "Paid")
                    .Count();

                ViewBag.TotalPaidAmount = totalPaidAmount.ToString("N0");
                ViewBag.CountPaidAmount = countPaidAmount.ToString("N0");

            }


            return View(loanApplicationViewMdoel);
        }

        // 貸款申請通過
        public ActionResult LoanConfirm(int id)
        {
            var loanApplication = _dbLoan3.LoanApplications.Find(id);

            if (loanApplication == null)
            {
                return HttpNotFound();
            }

            if (loanApplication.LoanStatus == "Pending")
            {
                // 確認貸款申請
                loanApplication.LoanStatus = "Confirmed";
                _dbLoan3.Entry(loanApplication).State = System.Data.Entity.EntityState.Modified;

                decimal loanAmount = loanApplication.LoanAmount;

                // 生成還款帳號
                var repaymentAccount = CreateRepaymentAccount(loanAmount);
                _dbLoan3.RepaymentAccounts.Add(repaymentAccount);
                _dbLoan3.SaveChanges(); // 先保存更改，獲取生成的 RepaymentAccountID

                // 更新 LoanApplication 的 RepaymentAccountID
                loanApplication.RepaymentAccountID = repaymentAccount.RepaymentAccountID;
                _dbLoan3.Entry(loanApplication).State = System.Data.Entity.EntityState.Modified;

                // 生成還款計劃
                GenerateRepaymentSchedules(id);



                // 保存更改到數據庫
                _dbLoan3.SaveChanges();


                // 匯入帳號
                ImportAccountTransaction(loanApplication.DisbursementAccount, loanApplication.LoanAmount);

                var repaymentaccountid = _dbLoan3.RepaymentAccounts
                    .Where(ra => ra.AccountNumber == loanApplication.DisbursementAccount)
                    .Select(ra => ra.RepaymentAccountID)
                    .FirstOrDefault();

                return RedirectToAction("LoanApplicationDetail", new { id = id });
            }

            return View();
        }

        // 拒絕貸款申請
        public ActionResult LoanReject(int id)
        {
            var loanApplication = _dbLoan3.LoanApplications.Find(id);

            if (loanApplication == null)
            {
                return HttpNotFound();
            }

            if (loanApplication.LoanStatus == "Pending")
            {
                loanApplication.LoanStatus = "Rejected";
                _dbLoan3.Entry(loanApplication).State = System.Data.Entity.EntityState.Modified;
                _dbLoan3.SaveChanges();
                return RedirectToAction("LoanApplicationDetail", new { id = id });
            }

            return View();
        }


        //還款計劃
        public ActionResult MonthlyRepaymentPlan2()
        {
            return View();
        }

        //依日期篩選還款計劃
        [HttpGet]
        public ActionResult GetFilteredRepaymentSchedules(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            endDate = endDate ?? startDate.Value.AddMonths(1).AddDays(-1);

            var repaymentSchedules = _dbLoan3.RepaymentSchedules
                .Where(rs => (!startDate.HasValue || rs.RepaymentDate >= startDate.Value) &&
                             (!endDate.HasValue || rs.RepaymentDate <= endDate.Value))
                .OrderBy(rs => rs.RepaymentDate)
                .ToList();

            var viewModelWithOutStatus = repaymentSchedules.Select(rs => new
            {
                RepaymentSchedule = new
                {
                    RepaymentDate = rs.RepaymentDate.ToString("yyyy/MM/dd"), // 將日期格式化為 MM/dd 格式
                    rs.RepaymentAmount,
                    rs.RepaymentStatus
                },
                LoanApplication = new
                {
                    rs.LoanApplicationID,
                },
                Customer = new
                {
                    CustomerID = _dbLoan3.LoanApplications.FirstOrDefault(la => la.LoanApplicationID == rs.LoanApplicationID)?.CustomerID,
                    FirstName = _dbLoan3.CustomersInLoans.FirstOrDefault(c => c.CustomerID == _dbLoan3.LoanApplications.FirstOrDefault(la => la.LoanApplicationID == rs.LoanApplicationID).CustomerID)?.FirstName
                }
            }).ToList();

            var resultWithOutStatus = new
            {
                RepaymentSchedules = viewModelWithOutStatus,
                AllCount = viewModelWithOutStatus.Count,
                PaidCount = viewModelWithOutStatus.Count(v => v.RepaymentSchedule.RepaymentStatus == "Paid"),
                UnpaidCount = viewModelWithOutStatus.Count(v => v.RepaymentSchedule.RepaymentStatus != "Paid")
            };

            return Json(resultWithOutStatus, JsonRequestBehavior.AllowGet);
        }

        //還款計劃狀態修改
        [HttpPost]
        public ActionResult RepaymentScheduleStatusEdit(int id)
        {
            var repaymentSchedule = _dbLoan3.RepaymentSchedules.Find(id);

            if (repaymentSchedule == null)
            {
                return HttpNotFound(); // 如果找不到相應的還款計畫，返回 404 Not Found
            }

            if (repaymentSchedule.RepaymentStatus == "Pending")
            {
                repaymentSchedule.RepaymentStatus = "Paid";
            }
            else if (repaymentSchedule.RepaymentStatus == "Paid")
            {
                repaymentSchedule.RepaymentStatus = "Pending";
            }

            _dbLoan3.Entry(repaymentSchedule).State = EntityState.Modified;
            _dbLoan3.SaveChanges(); // 保存修改

            return Json(new { success = true });
        }

        //當日還款紀錄
        public ActionResult TransactionLogList()
        {
            //今天的還款紀錄
            var today = DateTime.Today;

            var transactionLogs = _dbLoan3.TransactionLogs
                .Where(tl => DbFunctions.TruncateTime(tl.TransactionDate) == today)
                .Select(tl => new TransactionLogViewModel
                {
                    RepaymentAccountNumber = tl.RepaymentAccount.AccountNumber,
                    TransactionDate = tl.TransactionDate,
                    Amount = tl.Amount
                })
                .ToList();

            return View(transactionLogs);
        }

        public ActionResult TransactionLogByAccountNumber(string accountNumber)
        {
            var transactionLogs = _dbLoan3.TransactionLogs
                .Where(tl => tl.RepaymentAccount.AccountNumber == accountNumber)
                .Select(tl => new TransactionLogViewModel
                {
                    RepaymentAccountNumber = tl.RepaymentAccount.AccountNumber,
                    TransactionDate = tl.TransactionDate,
                    Amount = tl.Amount,

                })
                .ToList();
            ViewBag.AmountPaid = transactionLogs.Sum(tl => tl.Amount).ToString("N0");
            ViewBag.RepaymentAccountNumber = accountNumber;

            return View(transactionLogs);
        }


        public ActionResult LoanStatic()
        {

            return View();
        }


        //貸款申請統計
        public ActionResult GetLoanInterestRates()
        {
            var data = _dbLoan3.LoanApplications
                .GroupBy(lp => new { lp.LoanProductID, lp.LoanProduct.ProductName })
                .Select(g => new
                {
                    LoanProductID = g.Key.LoanProductID,
                    LoanProductName = g.Key.ProductName,
                    InterestRates = g.Select(lp => lp.InterestRate).ToList(),
                    TotalLoanCount = g.Count()
                })
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLoanCountByMonth()
        {
            var data = _dbLoan3.LoanApplications
                .GroupBy(lp => new { lp.LoanProductID, lp.LoanProduct.ProductName, lp.ApplicationDate.Year, lp.ApplicationDate.Month })
                .Select(g => new
                {
                    LoanProductID = g.Key.LoanProductID,
                    LoanProductName = g.Key.ProductName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalLoanCount = g.Count()
                })
                .OrderBy(g => g.LoanProductID)
                .ThenBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();
            int[] years = new int[] { 2023, 2024 };
            int[] months = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            List<int> homeLoanCounts = new List<int>();
            List<int> carLoanCounts = new List<int>();
            List<int> studentCounts = new List<int>();
            List<int> ints = new List<int>();
            List<string> dates = new List<string>();
            foreach (var year in years)
            {
                foreach (var month in months)
                {
                    var homeLoanCount = data.FirstOrDefault(d => d.LoanProductID == 1 && d.Year == year && d.Month == month)?.TotalLoanCount ?? 0;
                    var carLoanCount = data.FirstOrDefault(d => d.LoanProductID == 2 && d.Year == year && d.Month == month)?.TotalLoanCount ?? 0;
                    var studentCount = data.FirstOrDefault(d => d.LoanProductID == 3 && d.Year == year && d.Month == month)?.TotalLoanCount ?? 0;
                    var intCount = data.FirstOrDefault(d => d.LoanProductID == 5 && d.Year == year && d.Month == month)?.TotalLoanCount ?? 0;


                    homeLoanCounts.Add(homeLoanCount);
                    carLoanCounts.Add(carLoanCount);
                    studentCounts.Add(studentCount);
                    ints.Add(intCount);
                    dates.Add($"{year}/{month}");
                }
            }

            var result = new
            {
                年份 = years,
                months = months,
                HomeCounts = homeLoanCounts,
                CatCounts = carLoanCounts,
                StudentCounts = studentCounts,
                IntCounts = ints,
                Dates = dates
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        

        public ActionResult GetLoanAmount()
        {
            var data = _dbLoan3.LoanApplications
                .GroupBy(lp => new { lp.LoanProductID, lp.LoanProduct.ProductName })
                .Select(g => new
                {
                    LoanProductID = g.Key.LoanProductID,
                    LoanProductName = g.Key.ProductName,
                    LoanAmounts = g.Select(lp => lp.LoanAmount).ToList(),
                    TotalLoanCount = g.Count()
                })
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        public ActionResult RepaymentSchedule()
        {
            return View();
        }

        //從信箱查詢還款計劃
        public JsonResult GetRepaymentSchedule(string email)
        {
            var customer = _dbCustomer.Customers
                .Where(c => c.Email == email)
                .FirstOrDefault();
            if (customer == null)
            {
                return Json(new { Error = "Customer not found" }, JsonRequestBehavior.AllowGet);
            }
            var repaymentSchedules = _dbLoan3.RepaymentSchedules
                .Where(rs => rs.LoanApplication.CustomersInLoan.CustomerID == customer.CustomerID)
                .OrderBy(rs => rs.RepaymentDate)
                .Select(rs => new
                {
                    rs.LoanApplicationID,
                    rs.RepaymentDate,
                    rs.RepaymentStatus,
                    rs.RepaymentAmount
                })
                .ToList();

            var result = new
            {
                LastName = customer.LastName,
                FirstName = customer.FirstName,
                Email = customer.Email,
                RepaymentSchedules = repaymentSchedules
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // 生成還款帳號
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

        //匯入帳戶交易
        private void ImportAccountTransaction(string accountNumber, decimal amount)
        {
            var customerAccount = _dbCustomer.Accounts
                .Where(a => a.AccountNumber == accountNumber)
                .FirstOrDefault();

            if (customerAccount != null)
            {
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
            else
            {
                throw new Exception($"Customer account with number '{accountNumber}' not found.");
            }
        }



        public ActionResult ExportLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.ToList();

            var csv = new StringBuilder();
            csv.AppendLine("LoanApplicationID,CustomerID,LoanAmount,LoanStatus");

            foreach (var loan in loanApplications)
            {
                csv.AppendLine($"{loan.LoanApplicationID},{loan.CustomerID},{loan.LoanAmount},{loan.LoanStatus}");
            }

            var byteArray = Encoding.ASCII.GetBytes(csv.ToString());
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/csv", "LoanApplications.csv");
        }

        //下載經濟證明
        public async Task<ActionResult> DownloadEconomicProof(string fileName)
        {
            // 初始化 AzureBlobService，傳入 Azure Blob Storage 的連接字串和容器名稱
            string containerName = "ecommer";
            AzureBlobService azureBlobService = new AzureBlobService(connectionString, containerName);

            //下載文件
            Stream stream = await azureBlobService.DownloadFileAsync(fileName);


            //將文件流轉換為 byte 陣列
            byte[] fileBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            // 返回文件
            return File(fileBytes, "application/octet-stream", fileName);
        }


        //使用預存程序生成還款計劃
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