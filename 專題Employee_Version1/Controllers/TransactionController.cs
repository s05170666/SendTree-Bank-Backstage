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

            // 本週的最後一天（星期六）
            DateTime endOfWeek = firstDayOfWeek.AddDays(6);

            // 當前年份
            int currentYear = DateTime.Now.Year;

            var currentTransaction = _dbCustomer.Transactions.Include(x => x.Account)
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

            var historyTransactions = _dbCustomer.Transactions
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

            var transactions = _dbCustomer.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
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

            var transactionTotalCounts = transactions.Count();
            var totalAmount = transactions.Sum(t => t.Amount);

            // Prepare data for chart
            var transactionDates = transactions.Select(t => t.TransactionDate.Date).ToList();
            // Distinct() 會回傳一個沒有重複的集合
            var uniqueDates = transactionDates.Distinct().ToList();
            // Count() 會回傳集合中符合條件的元素數量
            var transactionCounts = uniqueDates.Select(date => transactionDates.Count(d => d == date)).ToList();

            var chartData = new
            {
                Labels = uniqueDates.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
                Data = transactionCounts
            };


            // Prepare data for transaction category pie chart
            var transactionCategories = transactions.Select(t => t.TransactionType).ToList();
            var uniqueCategories = transactionCategories.Distinct().ToList();
            var categoryCounts = uniqueCategories.Select(category => transactionCategories.Count(c => c == category)).ToList();

            var pieChartData = new
            {
                Labels = uniqueCategories,
                Data = categoryCounts
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