using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 專題Employee_Version1.Models.ViewModels
{
    public class LoanViewModel
    {
        public IEnumerable<LoanProduct> LoanProducts { get; set; }
        public LoanProductViewModels NewLoanProductViewModel { get; set; }
    }
}