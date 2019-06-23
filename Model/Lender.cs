using System;

namespace Model
{
    internal class Lender
    {
        public Lender() {}

        public Lender(bool childrenToo = false)
        {
            Name = $"Lender-{Guid.NewGuid().ToString()}";
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}