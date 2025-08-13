using System;
using System.Collections.Generic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models;
using NodaTime;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface ICompletedTemplate
    {
        string Name { get; set; }
        SignatureModel FirstSignature { get; }
        SignatureModel SecondSignature { get; }
        string SignatureString { get; }
        LocalDateTime LocalSignedAt { get; }
        bool IsPieChartVisible { get; set; }
        public List<StatusModel<TaskStatusEnum>> TaskStatuses { get; }
        public List<int> EditedByUsersId { get; }
    }
}
