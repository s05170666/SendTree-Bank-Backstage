using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ServiceModels;
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

        public List<LoanInterestRateItem> GetLoanInterestRates()
        {
            return _dbLoan.LoanApplications.AsNoTracking()
                .GroupBy(lp => new { lp.LoanProductID, lp.LoanProduct.ProductName })
                .Select(g => new LoanInterestRateItem
                {
                    LoanProductID = g.Key.LoanProductID,
                    LoanProductName = g.Key.ProductName,
                    InterestRates = g.Select(lp => lp.InterestRate).ToList(),
                    TotalLoanCount = g.Count()
                })
                .ToList();
        }

        public LoanMonthlyStatsResult GetLoanCountByMonth()
        {
            var data = _dbLoan.LoanApplications.AsNoTracking()
                .GroupBy(lp => new { lp.LoanProductID, lp.ApplicationDate.Year, lp.ApplicationDate.Month })
                .Select(g => new
                {
                    g.Key.LoanProductID,
                    g.Key.Year,
                    g.Key.Month,
                    TotalLoanCount = g.Count()
                })
                .ToList();

            var years = new[] { 2023, 2024 };
            var months = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var homeLoanCounts = new List<int>();
            var carLoanCounts = new List<int>();
            var studentCounts = new List<int>();
            var intCounts = new List<int>();
            var dates = new List<string>();

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
                    intCounts.Add(intCount);
                    dates.Add(year + "/" + month);
                }
            }

            return new LoanMonthlyStatsResult
            {
                Years = years,
                Months = months,
                HomeCounts = homeLoanCounts,
                CarCounts = carLoanCounts,
                StudentCounts = studentCounts,
                IntCounts = intCounts,
                Dates = dates
            };
        }

        public List<LoanAmountItem> GetLoanAmounts()
        {
            return _dbLoan.LoanApplications.AsNoTracking()
                .GroupBy(lp => new { lp.LoanProductID, lp.LoanProduct.ProductName })
                .Select(g => new LoanAmountItem
                {
                    LoanProductID = g.Key.LoanProductID,
                    LoanProductName = g.Key.ProductName,
                    LoanAmounts = g.Select(lp => lp.LoanAmount).ToList(),
                    TotalLoanCount = g.Count()
                })
                .ToList();
        }
    }
}
