using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class UserRelationArea
    {
        public int AreaId { get; set; }
        public int UserId { get; set; }
        public string AreaName { get; set; }
        public string AreaNamePath { get; set; }
        public UserRelationArea()
        {
        }
    }
}
