using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Models;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IStatus<T>
    {
        T CurrentStatus { get; set; }

        List<StatusModel<T>> StatusModels { get; set; }

        List<StatusModel<T>> GetStatuses()
        {
            return StatusModels;
        }

        virtual void SetItemsWithStatus(int itemsCount, T status)
        {
            var model = StatusModels.FirstOrDefault(x => x.Status.ToString() == status.ToString());
            if (model != null)
            {
                model.ItemNumber = itemsCount;
            }
        }
    }
}
