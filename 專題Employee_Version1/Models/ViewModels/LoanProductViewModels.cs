using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace 專題Employee_Version1.Models.ViewModels
{
    public class LoanProductViewModels
    {
        public int LoanProductID { get; set; }
        [Required(ErrorMessage = "necessary")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "necessary")]
        public decimal InterestRate { get; set; }
        [Required(ErrorMessage = "necessary")]
        public int LoanTerm { get; set; }
        [Required(ErrorMessage = "necessary")]
        public decimal MaxLoanAmount { get; set; }
        [Required(ErrorMessage = "necessary")]
        public decimal MinLoanAmount { get; set; }
        [Required(ErrorMessage = "necessary")]
        public string ProductDescription { get; set; }
  
        public string ImageFileName { get; set; }
        [Required(ErrorMessage = "necessary")]
        public HttpPostedFileBase ImageFile { get; set; }
        public string ImageUrl { get; set; } // 新增圖片 URL 屬性

    }
}