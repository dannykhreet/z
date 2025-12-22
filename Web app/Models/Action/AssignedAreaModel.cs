using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Action
{
    public class AssignedAreaModel
    {
        public int ActionId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string NamePath { get; set; }
        public int? ParentId { get; set; }
    }
}
