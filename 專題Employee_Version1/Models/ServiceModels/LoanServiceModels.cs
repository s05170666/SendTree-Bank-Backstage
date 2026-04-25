using System;
using System.Collections.Generic;
using 專題Employee_Version1.Models.ViewModels;

namespace 專題Employee_Version1.Models.ServiceModels
{
    // 還款計畫篩選結果（提供前端清單與統計數）
    public class RepaymentScheduleFilterResult
    {
        // 還款計畫資料清單
        public List<RepaymentScheduleItem> RepaymentSchedules { get; set; }

        // 全部案件數
        public int AllCount { get; set; }

        // 已繳款案件數
        public int PaidCount { get; set; }

        // 未繳款案件數
        public int UnpaidCount { get; set; }
    }

    // 單筆還款計畫資料（組合還款、案件、客戶資訊）
    public class RepaymentScheduleItem
    {
        // 還款資料
        public RepaymentScheduleDto RepaymentSchedule { get; set; }

        // 貸款申請資料
        public LoanApplicationDto LoanApplication { get; set; }

        // 客戶資料
        public CustomerDto Customer { get; set; }
    }

    // 還款欄位
    public class RepaymentScheduleDto
    {
        // 到期日
        public DateTime RepaymentDate { get; set; }

        // 應還金額
        public decimal RepaymentAmount { get; set; }

        // 還款狀態（Paid / Pending）
        public string RepaymentStatus { get; set; }
    }

    // 貸款申請欄位
    public class LoanApplicationDto
    {
        // 案件編號
        public int LoanApplicationID { get; set; }
    }

    // 客戶欄位
    public class CustomerDto
    {
        // 客戶編號
        public int? CustomerID { get; set; }

        // 客戶名稱（名字）
        public string FirstName { get; set; }
    }

    // 還款帳號查詢結果
    public class TransactionLogQueryResult
    {
        // 還款交易明細
        public List<TransactionLogViewModel> Logs { get; set; }

        // 累計繳款總額
        public decimal AmountPaid { get; set; }

        // 還款帳號
        public string RepaymentAccountNumber { get; set; }
    }

    // 利率統計資料（依貸款商品）
    public class LoanInterestRateItem
    {
        // 商品編號
        public int LoanProductID { get; set; }

        // 商品名稱
        public string LoanProductName { get; set; }

        // 各案件利率清單
        public List<decimal> InterestRates { get; set; }

        // 總貸款筆數
        public int TotalLoanCount { get; set; }
    }

    // 貸款金額統計資料（依貸款商品）
    public class LoanAmountItem
    {
        // 商品編號
        public int LoanProductID { get; set; }

        // 商品名稱
        public string LoanProductName { get; set; }

        // 各案件金額清單
        public List<decimal> LoanAmounts { get; set; }

        // 總貸款筆數
        public int TotalLoanCount { get; set; }
    }

    // 每月貸款件數統計（固定輸出格式，供前端圖表直接使用）
    public class LoanMonthlyStatsResult
    {
        // 年份清單
        public int[] Years { get; set; }

        // 月份清單
        public int[] Months { get; set; }

        // 房貸件數
        public List<int> HomeCounts { get; set; }

        // 車貸件數
        public List<int> CarCounts { get; set; }

        // 就學貸件數
        public List<int> StudentCounts { get; set; }

        // 其他商品件數（原本 ProductID = 5）
        public List<int> IntCounts { get; set; }

        // x 軸日期文字
        public List<string> Dates { get; set; }
    }
}
