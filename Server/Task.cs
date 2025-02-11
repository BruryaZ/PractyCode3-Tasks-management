using System;
using System.Collections.Generic;

namespace TodoApi
{
    public partial class Task
    {
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public bool? IsComplete { get; set; }
    }
}
