﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace 專題Employee_Version1.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Version3_LoanEntities4 : DbContext
    {
        public Version3_LoanEntities4()
            : base("name=Version3_LoanEntities4")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CustomersInLoan> CustomersInLoans { get; set; }
        public virtual DbSet<IncomeRangeRate> IncomeRangeRates { get; set; }
        public virtual DbSet<LoanApplication> LoanApplications { get; set; }
        public virtual DbSet<LoanProduct> LoanProducts { get; set; }
        public virtual DbSet<OccupationGroupRate> OccupationGroupRates { get; set; }
        public virtual DbSet<RepaymentAccount> RepaymentAccounts { get; set; }
        public virtual DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }
        public virtual DbSet<TransactionLog> TransactionLogs { get; set; }
    
        public virtual int GenerateRepaymentSchedules(Nullable<int> loanApplicationID)
        {
            var loanApplicationIDParameter = loanApplicationID.HasValue ?
                new ObjectParameter("LoanApplicationID", loanApplicationID) :
                new ObjectParameter("LoanApplicationID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("GenerateRepaymentSchedules", loanApplicationIDParameter);
        }
    }
}
