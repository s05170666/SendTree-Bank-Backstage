using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using System.Configuration;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ViewModels;
using 專題Employee_Version1.services;

namespace 專題Employee_Version1.Controllers
{
    public class LoanController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private readonly Version3_CustomerEntities1 _dbCustomer = new Version3_CustomerEntities1();
        private readonly Version3_LoanEntities5 _dbLoan3 = new Version3_LoanEntities5();
        private readonly LoanQueryService _loanQueryService;
        private readonly LoanCommandService _loanCommandService;

        public LoanController()
        {
            _loanQueryService = new LoanQueryService(_dbLoan3);
            _loanCommandService = new LoanCommandService(_dbLoan3, _dbCustomer, _connectionString);
        }

        public ActionResult Index()
        {
            var loanApplications = _dbLoan3.LoanApplications.AsNoTracking().ToList();
            ViewBag.AllCount = loanApplications.Count;
            ViewBag.PendingCount = loanApplications.Count(x => x.LoanStatus == "Pending");
            ViewBag.ConfirmedCount = loanApplications.Count(x => x.LoanStatus == "Confirmed" || x.LoanStatus == "Rejected");
            return View(loanApplications);
        }

        // 貸款商品列表
        public ActionResult LoanList()
        {
            var viewModel = new LoanViewModel
            {
                LoanProducts = _dbLoan3.LoanProducts.AsNoTracking().ToList(),
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

        // 貸款商品修改
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
                ProductDescription = loan.ProductDescription
            };
            return View(loanViewModel);
        }

        [HttpPost]
        public ActionResult Edit(LoanProductViewModels loan)
        {
            if (!ModelState.IsValid)
            {
                return View("LoanList");
            }

            var file = loan.ImageFile;
            var imageFileName = file != null ? Path.GetFileName(file.FileName) : null;
            if (file != null)
            {
                var path = Path.Combine(Server.MapPath("~/ImageFiles/"), imageFileName);
                file.SaveAs(path);
            }

            _loanCommandService.UpdateLoanProduct(loan, imageFileName);
            return RedirectToAction("LoanList");
        }

        // 貸款商品新增
        [HttpPost]
        public async Task<ActionResult> CreateProduct(LoanViewModel loan)
        {
            if (!ModelState.IsValid)
            {
                return View("LoanList");
            }

            var file = loan.NewLoanProductViewModel.ImageFile;
            if (file == null || file.ContentLength <= 0)
            {
                ModelState.AddModelError("", "請上傳圖片檔案。");
                return View("LoanList");
            }

            var imageFileName = Path.GetFileName(file.FileName);
            var result = await _loanCommandService.CreateProductAsync(
                loan.NewLoanProductViewModel,
                file.InputStream,
                imageFileName);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View("LoanList");
            }

            return RedirectToAction("LoanList");
        }

        // 貸款商品刪除
        public ActionResult Delete(int id)
        {
            var deleted = _loanCommandService.DeleteLoanProduct(id);
            if (!deleted)
            {
                return HttpNotFound();
            }
            return RedirectToAction("LoanList");
        }

        // 所有貸款申請
        public ActionResult AllLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.AsNoTracking().ToList();
            return PartialView("_AllLoanApplications", loanApplications);
        }

        // 待處理貸款申請
        public ActionResult PendingLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.AsNoTracking().Where(x => x.LoanStatus == "Pending").ToList();
            return PartialView("_PendingLoanApplications", loanApplications);
        }

        // 已確認貸款申請
        public ActionResult ConfirmedLoanApplications()
        {
            var loanApplications = _dbLoan3.LoanApplications.AsNoTracking().Where(x => x.LoanStatus == "Confirmed" || x.LoanStatus == "Rejected").ToList();
            return PartialView("_ConfirmedLoanApplications", loanApplications);
        }

        // 貸款申請詳細資料
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
                    .Count(rs => rs.LoanApplicationID == id && rs.RepaymentStatus == "Paid");

                ViewBag.TotalPaidAmount = totalPaidAmount.ToString("N0");
                ViewBag.CountPaidAmount = countPaidAmount.ToString("N0");
            }

            return View(loanApplicationViewMdoel);
        }

        // 貸款申請通過
        public ActionResult LoanConfirm(int id)
        {
            var result = _loanCommandService.ConfirmLoanApplication(id);
            if (!result.Success && result.Message == "LoanApplicationNotFound")
            {
                return HttpNotFound();
            }
            if (!result.Success)
            {
                return View();
            }

            return RedirectToAction("LoanApplicationDetail", new { id });
        }

        // 拒絕貸款申請
        public ActionResult LoanReject(int id)
        {
            var result = _loanCommandService.RejectLoanApplication(id);
            if (!result.Success && result.Message == "LoanApplicationNotFound")
            {
                return HttpNotFound();
            }
            if (!result.Success)
            {
                return View();
            }

            return RedirectToAction("LoanApplicationDetail", new { id });
        }

        // 還款計劃
        public ActionResult MonthlyRepaymentPlan2()
        {
            return View();
        }

        // 依日期篩選還款計劃
        [HttpGet]
        public ActionResult GetFilteredRepaymentSchedules(DateTime? startDate, DateTime? endDate)
        {
            var result = _loanQueryService.GetFilteredRepaymentSchedules(startDate, endDate);
            var response = new
            {
                RepaymentSchedules = result.RepaymentSchedules.Select(rs => new
                {
                    RepaymentSchedule = new
                    {
                        RepaymentDate = rs.RepaymentSchedule.RepaymentDate.ToString("yyyy/MM/dd"),
                        rs.RepaymentSchedule.RepaymentAmount,
                        rs.RepaymentSchedule.RepaymentStatus
                    },
                    LoanApplication = new
                    {
                        rs.LoanApplication.LoanApplicationID
                    },
                    Customer = new
                    {
                        rs.Customer.CustomerID,
                        rs.Customer.FirstName
                    }
                }).ToList(),
                result.AllCount,
                result.PaidCount,
                result.UnpaidCount
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // 還款計劃狀態修改
        [HttpPost]
        public ActionResult RepaymentScheduleStatusEdit(int id)
        {
            var success = _loanCommandService.ToggleRepaymentScheduleStatus(id);
            if (!success)
            {
                return HttpNotFound();
            }
            return Json(new { success = true });
        }

        // 當日還款紀錄
        public ActionResult TransactionLogList()
        {
            var transactionLogs = _loanQueryService.GetTodayTransactionLogs(DateTime.Today);
            return View(transactionLogs);
        }

        public ActionResult TransactionLogByAccountNumber(string accountNumber)
        {
            var result = _loanQueryService.GetTransactionLogsByAccountNumber(accountNumber);
            ViewBag.AmountPaid = result.AmountPaid.ToString("N0");
            ViewBag.RepaymentAccountNumber = result.RepaymentAccountNumber;
            return View(result.Logs);
        }

        public ActionResult LoanStatic()
        {
            return View();
        }

        // 貸款申請統計
        public ActionResult GetLoanInterestRates()
        {
            var data = _loanQueryService.GetLoanInterestRates();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLoanCountByMonth()
        {
            var result = _loanQueryService.GetLoanCountByMonth();
            return Json(new
            {
                年份 = result.Years,
                months = result.Months,
                result.HomeCounts,
                CatCounts = result.CarCounts,
                result.StudentCounts,
                result.IntCounts,
                result.Dates
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLoanAmount()
        {
            var data = _loanQueryService.GetLoanAmounts();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RepaymentSchedule()
        {
            return View();
        }

        // 從信箱查詢還款計劃
        public JsonResult GetRepaymentSchedule(string email)
        {
            var customer = _dbCustomer.Customers.FirstOrDefault(c => c.Email == email);
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

            return Json(new
            {
                customer.LastName,
                customer.FirstName,
                customer.Email,
                RepaymentSchedules = repaymentSchedules
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportLoanApplications()
        {
            var bytes = _loanCommandService.ExportLoanApplicationsCsv();
            return File(new MemoryStream(bytes), "text/csv", "LoanApplications.csv");
        }

        // 下載經濟證明
        public async Task<ActionResult> DownloadEconomicProof(string fileName)
        {
            var fileBytes = await _loanCommandService.DownloadEconomicProofAsync(fileName);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
