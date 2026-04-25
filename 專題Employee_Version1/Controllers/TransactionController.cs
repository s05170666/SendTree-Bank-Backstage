using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ViewModels;

namespace 專題Employee_Version1.Controllers
{
    public class TransactionController : Controller
    {
        private Version3_CustomerEntities1 _dbCustomer = new Version3_CustomerEntities1();
        // GET: Transaction
        public ActionResult Index()
        {
            // 當前日期
            DateTime currentDate = DateTime.Now;

            // 當前週的第一天（星期日）
            DateTime firstDayOfWeek = currentDate.Date.AddDays(-(int)currentDate.DayOfWeek);

            // 當前年份
            int currentYear = DateTime.Now.Year;

            var currentTransaction = _dbCustomer.Transactions.AsNoTracking().Include(x => x.Account)
                .Where(x => x.TransactionDate >= firstDayOfWeek && x.TransactionDate <= currentDate && x.TransactionDate.Year == currentYear)
                .ToList();

            ViewBag.totalAmount = currentTransaction.Sum(x => x.Amount);
            ViewBag.totalTransaction = currentTransaction.Count();
            ViewBag.StartDate = firstDayOfWeek.ToString("MM/dd");
            ViewBag.currentDate = currentDate.ToString("MM/dd");

            return View(currentTransaction);
        }



        public ActionResult History(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.Now.AddDays(-30);
            endDate = endDate ?? DateTime.Now;

            var historyTransactions = _dbCustomer.Transactions.AsNoTracking()
                                    .Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate)
                                    .ToList();

            ViewBag.totalAmount = historyTransactions.Sum(x => x.Amount);
            ViewBag.totalTransaction = historyTransactions.Count();

            return View("Index", historyTransactions);
        }

        public ActionResult TransactionTimeDistribution(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.Now.AddDays(-30);
            endDate = endDate ?? DateTime.Now;

            var baseQuery = _dbCustomer.Transactions.AsNoTracking()
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

            var transactions = baseQuery.Select(t => new TransactionViewModel
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

            var transactionTotalCounts = transactions.Count();
            var totalAmount = transactions.Sum(t => t.Amount);

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

            var chartData = new
            {
                Labels = dailyCounts.Select(x => x.Date.Value.ToString("yyyy-MM-dd")).ToList(),
                Data = dailyCounts.Select(x => x.Count).ToList()
            };


            var categoryCounts = baseQuery
                .GroupBy(t => t.TransactionType)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var pieChartData = new
            {
                Labels = categoryCounts.Select(x => x.Category).ToList(),
                Data = categoryCounts.Select(x => x.Count).ToList()
            };


            ViewBag.ChartData = JsonConvert.SerializeObject(chartData);
            ViewBag.PieChartData = JsonConvert.SerializeObject(pieChartData);
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TransactionTotalCounts = transactionTotalCounts;
            ViewBag.TotalAmount = totalAmount;

            return View(transactions);

        }
    }
}
