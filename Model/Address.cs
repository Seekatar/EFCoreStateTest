using System;

namespace Model
{
    public class Address
    {
        public Address()
        {
            City = $"City-{Guid.NewGuid().ToString()}";
        }

        public Guid Id { get; set; }
        public string City { get; set; }
    }
}