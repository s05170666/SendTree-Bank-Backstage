using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 專題Employee_Version1.Models.ViewModels
{
    public class TransactionLogViewModel
    {
        public string RepaymentAccountNumber { get; set; }

        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? AmountPaid { get; set; }

    }
}