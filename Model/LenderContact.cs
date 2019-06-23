using System;

namespace Model
{
    internal class LenderContact
    {
        public LenderContact()
        {
            Name = $"LenderContact-{Guid.NewGuid()}";
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Lender Lender { get; set; }
    }
}