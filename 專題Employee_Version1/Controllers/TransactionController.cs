using Newtonsoft.Json;
using System;
using System.Web.Mvc;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.services;

namespace 專題Employee_Version1.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;

        public TransactionController()
        {
            _transactionService = new TransactionService(new Version3_CustomerEntities1());
        }

        // GET: Transaction
        public ActionResult Index()
        {
            var overview = _transactionService.GetCurrentWeekOverview(DateTime.Now);
            ViewBag.totalAmount = overview.TotalAmount;
            ViewBag.totalTransaction = overview.TotalTransaction;
            ViewBag.StartDate = overview.StartDateLabel;
            ViewBag.currentDate = overview.EndDateLabel;
            return View(overview.Transactions);
        }

        public ActionResult History(DateTime? startDate, DateTime? endDate)
        {
            var overview = _transactionService.GetHistoryOverview(startDate, endDate);
            ViewBag.totalAmount = overview.TotalAmount;
            ViewBag.totalTransaction = overview.TotalTransaction;
            return View("Index", overview.Transactions);
        }

        public ActionResult TransactionTimeDistribution(DateTime? startDate, DateTime? endDate)
        {
            var result = _transactionService.GetTimeDistribution(startDate, endDate);
            ViewBag.ChartData = JsonConvert.SerializeObject(new
            {
                Labels = result.ChartLabels,
                Data = result.ChartData
            });
            ViewBag.PieChartData = JsonConvert.SerializeObject(new
            {
                Labels = result.PieLabels,
                Data = result.PieData
            });
            ViewBag.StartDate = result.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = result.EndDate.ToString("yyyy-MM-dd");
            ViewBag.TransactionTotalCounts = result.TransactionTotalCounts;
            ViewBag.TotalAmount = result.TotalAmount;

            return View(result.Transactions);
        }
    }
}
