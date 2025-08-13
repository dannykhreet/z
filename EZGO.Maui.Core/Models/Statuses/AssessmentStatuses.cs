using System.Collections.Generic;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class AssessmentStatuses : IStatus<AssessmentsTypeEnum>
    {
        public AssessmentStatuses()
        {
            StatusModels = new List<StatusModel<AssessmentsTypeEnum>>
            {
                new StatusModel<AssessmentsTypeEnum>(AssessmentsTypeEnum.Default, "GreenColor"),
            };
        }

        public AssessmentsTypeEnum CurrentStatus { get; set; }
        public List<StatusModel<AssessmentsTypeEnum>> StatusModels { get; set; }
    }
}
