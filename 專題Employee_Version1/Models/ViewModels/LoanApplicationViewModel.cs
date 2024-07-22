using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace 專題Employee_Version1.Models.ViewModels
{
    public class LoanApplicationViewModel
    {
        public int LoanApplicationID { get; set; }
        public string ProductName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

        public DateTime ApplicationDate { get; set; }
        public decimal LoanAmount { get; set; }
        public string LoanStatus { get; set; }
        public string DisbursementAccount { get; set; }
        public List<RepaymentSchedule> RepaymentSchedules { get; set; }
        public string RepaymentAccount { get; set; }
        public string EconomicProof { get; set; }
        public Nullable<int> RepaymentMonths { get; set; }
        public Nullable<decimal> TotalRepaymentAmount { get; set; }

        public Nullable<decimal> InterestRate { get; set; }

    }
}