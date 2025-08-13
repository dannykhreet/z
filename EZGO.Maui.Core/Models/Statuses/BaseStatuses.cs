using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class BaseStatuses<T> : IStatus<T>
    {
        private List<StatusModel<T>> statusModels;

        public BaseStatuses(List<StatusModel<T>> statuses)
        {
            StatusModels = statuses;
        }

        public T CurrentStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<StatusModel<T>> StatusModels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
