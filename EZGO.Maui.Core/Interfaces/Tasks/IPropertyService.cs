using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Tasks.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Tasks
{
    public interface IPropertyService : IDisposable
    {
        Task<bool> RegisterTaskPropertyValueAync(PropertyUserValue value);

        Task<List<Property>> GetAllPropertiesAsync(bool refresh = false, bool isFromSyncService = false);

        Task LoadTaskPropertiesAsync(List<BasicTaskModel> tasks, bool includeProperties, bool refresh);

        Task LoadTaskTemplatesPropertiesAsync(List<BasicTaskTemplateModel> taskTemplates, bool refresh);

        Task<int> UploadLocalPropertyValues();
    }
}
