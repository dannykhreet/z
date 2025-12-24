using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Users
{
    public class UserExtendedDetails
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EmployeeId { get; set;  }
        public string EmployeeFunction { get; set; }
        public string Bio { get; set; }
        public string Description { get; set; }
    }
}
