using System;

namespace Model
{
    internal class Thing
    {
        public Thing()
        {
            Name = $"Name-{Guid.NewGuid().ToString()}";
            Title = $"Title-{Guid.NewGuid().ToString()}";
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }
}