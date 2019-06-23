using System;

namespace Model
{
    public class Thing
    {
        public Thing()
        {
            Name = $"City-{Guid.NewGuid().ToString()}";
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}