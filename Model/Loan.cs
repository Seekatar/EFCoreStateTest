using System;

namespace Model
{
    public class Loan
    {
        public Loan() {}
        public Loan(bool childrenToo = false)
        {
            Name = $"Loan-{Guid.NewGuid()}";
            if (childrenToo)
            {
                Lender = new Lender();
                LenderContact = new LenderContact {Lender = Lender};
            }
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Lender Lender { get; set; }
        public LenderContact LenderContact { get; set; }
    }

}