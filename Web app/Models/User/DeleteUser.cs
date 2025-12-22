using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.User
{
    public class DeleteUser
    {
        public int UserId { get; set; }
        public int? SuccessorId { get; set; }
        public string ValidationKey { get; set; }
    }
}
