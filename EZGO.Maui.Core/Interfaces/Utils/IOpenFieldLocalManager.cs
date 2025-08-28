using System;
using System.Threading.Tasks;
using EZGO.Maui.Core.Models.Local;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IOpenFieldLocalManager
    {
        Task AddOrUpdateLocalTemplateAsync(LocalTemplateModel model);
        Task<bool> CheckIfLocalTemplateExistsAsync(int checklistTemplateId);
    }
}
