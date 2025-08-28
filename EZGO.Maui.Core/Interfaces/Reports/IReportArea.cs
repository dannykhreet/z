using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Models.Areas;

namespace EZGO.Maui.Core.Interfaces.Reports
{
    public interface IReportArea
    {
        List<BasicWorkAreaModel> WorkAreas { get; set; }

        List<BasicWorkAreaModel> FlattenedWorkAreas { get; set; }
    }
}
