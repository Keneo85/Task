using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        private Category() { }
    }
}
