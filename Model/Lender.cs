using System;

namespace Model
{
    public class Lender
    {
        public Lender() {}

        public Lender(bool childrenToo = false)
        {
            Name = $"Lender-{Guid.NewGuid().ToString()}";
            if ( childrenToo)
            {
                Address = new Address();
            }
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
    }
}