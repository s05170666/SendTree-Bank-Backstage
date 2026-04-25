using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ViewModels;

namespace 專題Employee_Version1.services
{
    public class LoanQueryService
    {
        private readonly Version3_LoanEntities5 _dbLoan;

        public LoanQueryService(Version3_LoanEntities5 dbLoan)
        {
            _dbLoan = dbLoan;
        }

        public RepaymentScheduleFilterResult GetFilteredRepaymentSchedules(DateTime? startDate, DateTime? endDate)
        {
            var effectiveStartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var effectiveEndDate = endDate ?? effectiveStartDate.AddMonths(1).AddDays(-1);

            var repaymentSchedules = _dbLoan.RepaymentSchedules.AsNoTracking()
                .Where(rs => rs.RepaymentDate >= effectiveStartDate && rs.RepaymentDate <= effectiveEndDate)
                .OrderBy(rs => rs.RepaymentDate)
                .Select(rs => new RepaymentScheduleItem
                {
                    RepaymentSchedule = new RepaymentScheduleDto
                    {
                        RepaymentDate = rs.RepaymentDate,
                        RepaymentAmount = rs.RepaymentAmount,
                        RepaymentStatus = rs.RepaymentStatus
                    },
                    LoanApplication = new LoanApplicationDto
                    {
                        LoanApplicationID = rs.LoanApplicationID
                    },
                    Customer = new CustomerDto
                    {
                        CustomerID = rs.LoanApplication.CustomerID,
                        FirstName = rs.LoanApplication.CustomersInLoan.FirstName
                    }
                })
                .ToList();

            return new RepaymentScheduleFilterResult
            {
                RepaymentSchedules = repaymentSchedules,
                AllCount = repaymentSchedules.Count,
                PaidCount = repaymentSchedules.Count(x => x.RepaymentSchedule.RepaymentStatus == "Paid"),
                UnpaidCount = repaymentSchedules.Count(x => x.RepaymentSchedule.RepaymentStatus != "Paid")
            };
        }

        public List<TransactionLogViewModel> GetTodayTransactionLogs(DateTime today)
        {
            return _dbLoan.TransactionLogs.AsNoTracking()
                .Where(tl => DbFunctions.TruncateTime(tl.TransactionDate) == today)
                .Select(tl => new TransactionLogViewModel
                {
                    RepaymentAccountNumber = tl.RepaymentAccount.AccountNumber,
                    TransactionDate = tl.TransactionDate,
                    Amount = tl.Amount
                })
                .ToList();
        }

        public TransactionLogQueryResult GetTransactionLogsByAccountNumber(string accountNumber)
        {
            var transactionLogs = _dbLoan.TransactionLogs.AsNoTracking()
                .Where(tl => tl.RepaymentAccount.AccountNumber == accountNumber)
                .Select(tl => new TransactionLogViewModel
                {
                    RepaymentAccountNumber = tl.RepaymentAccount.AccountNumber,
                    TransactionDate = tl.TransactionDate,
                    Amount = tl.Amount
                })
                .ToList();

            return new TransactionLogQueryResult
            {
                Logs = transactionLogs,
                AmountPaid = transactionLogs.Sum(tl => tl.Amount),
                RepaymentAccountNumber = accountNumber
            };
        }
    }

    public class RepaymentScheduleFilterResult
    {
        public List<RepaymentScheduleItem> RepaymentSchedules { get; set; }
        public int AllCount { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
    }

    public class RepaymentScheduleItem
    {
        public RepaymentScheduleDto RepaymentSchedule { get; set; }
        public LoanApplicationDto LoanApplication { get; set; }
        public CustomerDto Customer { get; set; }
    }

    public class RepaymentScheduleDto
    {
        public DateTime RepaymentDate { get; set; }
        public decimal RepaymentAmount { get; set; }
        public string RepaymentStatus { get; set; }
    }

    public class LoanApplicationDto
    {
        public int LoanApplicationID { get; set; }
    }

    public class CustomerDto
    {
        public int? CustomerID { get; set; }
        public string FirstName { get; set; }
    }

    public class TransactionLogQueryResult
    {
        public List<TransactionLogViewModel> Logs { get; set; }
        public decimal AmountPaid { get; set; }
        public string RepaymentAccountNumber { get; set; }
    }
}
