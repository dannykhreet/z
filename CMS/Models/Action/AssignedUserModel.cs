using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Action
{
    public class AssignedUserModel
    {
        public int ActionId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }

        //"ActionId": 10373,
        //"Id": 1815,
        //"Name": "Frank Rijkaard",
        //"Picture": "users/FrankRijkaard.png"
    }
}
