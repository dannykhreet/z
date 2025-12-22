using System;
using System.Collections.Generic;
using WebApp.Models.Company;

namespace WebApp.Models
{
    public class CompanyModel
    {

        public int Id { get; set; }

        public int ManagerId { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Picture { get; set; }

        public int? AreaId { get; set; }

        public List<CompanyShiftModel> Shifts { get; set; }

    }
}
