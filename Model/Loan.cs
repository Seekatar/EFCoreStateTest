using System;

namespace Model
{
    internal class LoanBase
    {
        protected LoanBase()
        {
            Name = $"Loan-{Guid.NewGuid()}";
        }
        protected LoanBase(bool childrenToo = false)
        {
            Name = $"Loan-{Guid.NewGuid()}";
            if (childrenToo)
            {
                Lender = new Lender();
            }
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Lender Lender { get; set; }
        public LenderContact LenderContact { get; set; }
    }

    internal class Loan : LoanBase
    {
        public Loan()
        {
        }
        public Loan(bool childrenToo = false) : base(childrenToo)
        {
            if (childrenToo)
            {
                LenderContact = new LenderContact { Lender = Lender };
            }
        }
    }

    internal class LoanEx : LoanBase
    {
        public LoanEx()
        {
        }
        public LoanEx(bool childrenToo = false) : base(childrenToo)
        {
            if (childrenToo)
            {
                LenderContact = new LenderContact { Lender = Lender };
            }
        }
        public Guid? LenderId { get; set; }
        public Guid? LenderContactId { get; set; }
    }
}