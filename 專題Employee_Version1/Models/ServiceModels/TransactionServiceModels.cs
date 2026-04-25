using System;
using System.Collections.Generic;
using 專題Employee_Version1.Models;
using 專題Employee_Version1.Models.ViewModels;

namespace 專題Employee_Version1.Models.ServiceModels
{
    // 交易總覽結果（供列表頁與歷史查詢共用）
    public class TransactionOverviewResult
    {
        // 交易清單
        public List<Transaction> Transactions { get; set; }

        // 交易總額
        public decimal TotalAmount { get; set; }

        // 交易筆數
        public int TotalTransaction { get; set; }

        // 起始日期（顯示用）
        public string StartDateLabel { get; set; }

        // 結束日期（顯示用）
        public string EndDateLabel { get; set; }
    }

    // 交易圖表分析結果
    public class TransactionTimeDistributionResult
    {
        // 交易明細（表格使用）
        public List<TransactionViewModel> Transactions { get; set; }

        // 交易總筆數
        public int TransactionTotalCounts { get; set; }

        // 交易總額
        public decimal TotalAmount { get; set; }

        // 折線圖 x 軸標籤
        public List<string> ChartLabels { get; set; }

        // 折線圖資料
        public List<int> ChartData { get; set; }

        // 圓餅圖標籤
        public List<string> PieLabels { get; set; }

        // 圓餅圖資料
        public List<int> PieData { get; set; }

        // 查詢起始日
        public DateTime StartDate { get; set; }

        // 查詢結束日
        public DateTime EndDate { get; set; }
    }
}
