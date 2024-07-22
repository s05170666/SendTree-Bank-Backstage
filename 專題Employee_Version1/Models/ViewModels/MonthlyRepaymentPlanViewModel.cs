using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 專題Employee_Version1.Models.ViewModels
{
    public class MonthlyRepaymentPlanViewModel
    {
        public RepaymentSchedule RepaymentSchedule { get; set; }
        public LoanApplication LoanApplication { get; set; }
        public CustomersInLoan Customer { get; set; }
    }
}