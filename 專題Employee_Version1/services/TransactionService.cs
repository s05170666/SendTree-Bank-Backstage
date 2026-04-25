using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ViewModels;

namespace 專題Employee_Version1.services
{
    public class TransactionService
    {
        private readonly Version3_CustomerEntities1 _dbCustomer;

        public TransactionService(Version3_CustomerEntities1 dbCustomer)
        {
            _dbCustomer = dbCustomer;
        }

        public TransactionOverviewResult GetCurrentWeekOverview(DateTime currentDate)
        {
            var firstDayOfWeek = currentDate.Date.AddDays(-(int)currentDate.DayOfWeek);
            var currentYear = currentDate.Year;

            var transactions = _dbCustomer.Transactions.AsNoTracking().Include(x => x.Account)
                .Where(x => x.TransactionDate >= firstDayOfWeek &&
                            x.TransactionDate <= currentDate &&
                            x.TransactionDate.Year == currentYear)
                .ToList();

            return new TransactionOverviewResult
            {
                Transactions = transactions,
                TotalAmount = transactions.Sum(x => x.Amount),
                TotalTransaction = transactions.Count,
                StartDateLabel = firstDayOfWeek.ToString("MM/dd"),
                EndDateLabel = currentDate.ToString("MM/dd")
            };
        }

        public TransactionOverviewResult GetHistoryOverview(DateTime? startDate, DateTime? endDate)
        {
            var effectiveStart = startDate ?? DateTime.Now.AddDays(-30);
            var effectiveEnd = endDate ?? DateTime.Now;

            var transactions = _dbCustomer.Transactions.AsNoTracking()
                .Where(x => x.TransactionDate >= effectiveStart && x.TransactionDate <= effectiveEnd)
                .ToList();

            return new TransactionOverviewResult
            {
                Transactions = transactions,
                TotalAmount = transactions.Sum(x => x.Amount),
                TotalTransaction = transactions.Count,
                StartDateLabel = effectiveStart.ToString("MM/dd"),
                EndDateLabel = effectiveEnd.ToString("MM/dd")
            };
        }

        public TransactionTimeDistributionResult GetTimeDistribution(DateTime? startDate, DateTime? endDate)
        {
            var effectiveStart = startDate ?? DateTime.Now.AddDays(-30);
            var effectiveEnd = endDate ?? DateTime.Now;

            var baseQuery = _dbCustomer.Transactions.AsNoTracking()
                .Where(t => t.TransactionDate >= effectiveStart && t.TransactionDate <= effectiveEnd);

            var transactions = baseQuery
                .Select(t => new TransactionViewModel
                {
                    TransactionID = t.TransactionID,
                    AccountID = t.AccountID,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    Description = t.Description,
                    InteractiveAccountNumber = t.InteractiveAccountNumber
                })
                .ToList();

            var dailyCounts = baseQuery
                .GroupBy(t => DbFunctions.TruncateTime(t.TransactionDate))
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList()
                .Where(x => x.Date.HasValue)
                .ToList();

            var categoryCounts = baseQuery
                .GroupBy(t => t.TransactionType)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new TransactionTimeDistributionResult
            {
                Transactions = transactions,
                TransactionTotalCounts = transactions.Count,
                TotalAmount = transactions.Sum(t => t.Amount),
                ChartLabels = dailyCounts.Select(x => x.Date.Value.ToString("yyyy-MM-dd")).ToList(),
                ChartData = dailyCounts.Select(x => x.Count).ToList(),
                PieLabels = categoryCounts.Select(x => x.Category).ToList(),
                PieData = categoryCounts.Select(x => x.Count).ToList(),
                StartDate = effectiveStart,
                EndDate = effectiveEnd
            };
        }
    }

    public class TransactionOverviewResult
    {
        public List<Transaction> Transactions { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransaction { get; set; }
        public string StartDateLabel { get; set; }
        public string EndDateLabel { get; set; }
    }

    public class TransactionTimeDistributionResult
    {
        public List<TransactionViewModel> Transactions { get; set; }
        public int TransactionTotalCounts { get; set; }
        public decimal TotalAmount { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<int> ChartData { get; set; }
        public List<string> PieLabels { get; set; }
        public List<int> PieData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
