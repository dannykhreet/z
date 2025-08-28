using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Services.Tasks
{
    public class PropertyService : IPropertyService
    {
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IFileService _fileService;

        private const string localTaskPropertyModelsFilename = "localtaskpropertymodels.json";

        public PropertyService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
            _fileService = DependencyService.Get<IFileService>();
        }

        public async Task<List<Property>> GetAllPropertiesAsync(bool refresh = false, bool isFromSyncService = false)
        {
            var action = "properties?include=propertyvalues";

            var result = await _apiRequestHandler.HandleListRequest<Property>(action, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<bool> RegisterTaskPropertyValueAync(PropertyUserValue value)
        {
            if (value == null)
                return false;
            if (await InternetHelper.HasInternetConnection())
            {
                return await PostPropertyValues(value);
            }
            else
            {
                var localPropertyValues = await GetLocalPropertyValuesAsync();
                var property = localPropertyValues.FirstOrDefault(x => x.PropertyId == value.PropertyId);
                if (property != null)
                    localPropertyValues.Remove(property);

                localPropertyValues.Add(value);
                await SaveLocalPropertyValuesAsync(localPropertyValues);
                return false;
            }
        }

        private async Task<bool> PostPropertyValues(PropertyUserValue value)
        {
            string action;

            // If it's a new property
            if (value.Id == 0)
            {
                action = "propertyuservalue/tasks/add";
            }
            // If the property already exists
            else
            {
                action = $"propertyuservalue/tasks/change/{value.Id}";
            }

            var response = await _apiRequestHandler?.HandlePostRequest(action, value);

            if (response.IsSuccessStatusCode)
            {
                // If it was a new object
                if (value.Id == 0)
                {
                    var responsestr = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());
                    var newId = int.Parse(responsestr);
                    value.Id = newId;
                    value.CreatedAt = DateTime.UtcNow;
                }

                value.ModifiedAt = DateTime.UtcNow;
            }

            return response.IsSuccessStatusCode;
        }

        public async Task LoadTaskPropertiesAsync(List<BasicTaskModel> tasks, bool includeProperties, bool refresh)
        {
            if (tasks == null || !tasks.Any())
                return;

            if (includeProperties)
            {
                var props = await GetAllPropertiesAsync(refresh: refresh);
                tasks.SelectMany(task => task.Properties ?? new List<PropertyTaskTemplateModel>()).Join(props, inner => inner.PropertyId, outer => outer.Id, (taskProp, property) =>
                {
                    taskProp.Property = property;
                    return taskProp;
                }).ToList(); // Call ToList to make enumarable run
            }

            // Needed to create a property for at least planned time
            tasks.ForEach(task => task.CreatePropertyList());
        }

        public async Task LoadTaskTemplatesPropertiesAsync(List<BasicTaskTemplateModel> taskTemplates, bool refresh)
        {
            if (taskTemplates == null || !taskTemplates.Any())
                return;

            var props = await GetAllPropertiesAsync(refresh: refresh).ConfigureAwait(false);
            if (props != null)
            {
                taskTemplates.SelectMany(task => task.Properties ?? new List<PropertyTaskTemplateModel>()).Join(props, inner => inner.PropertyId, outer => outer.Id, (taskProp, property) =>
                {
                    taskProp.Property = property;
                    return taskProp;
                }).ToList(); // Call ToList to make enumarable run

                // Needed to create a property for at least planned time
                taskTemplates.ForEach(task => task.CreatePropertyList());
            }
        }

        private async Task<List<PropertyUserValue>> GetLocalPropertyValuesAsync()
        {
            var result = new List<PropertyUserValue>();

            string localPropertyValues = await _fileService.ReadFromInternalStorageAsync(localTaskPropertyModelsFilename, Constants.PersistentDataDirectory);

            if (!string.IsNullOrWhiteSpace(localPropertyValues))
                result = JsonSerializer.Deserialize<List<PropertyUserValue>>(localPropertyValues) ?? new List<PropertyUserValue>();

            return result;
        }

        public async Task<int> UploadLocalPropertyValues()
        {
            int count = 0;

            var localPropertyValues = await GetLocalPropertyValuesAsync();

            var Iterations = localPropertyValues.Count - 1;

            for (int i = Iterations; i >= 0; i--)
            {
                var value = localPropertyValues[i];

                if (await InternetHelper.HasInternetConnection())
                {
                    bool result = await PostPropertyValues(value);

                    if (result)
                    {
                        localPropertyValues.Remove(value);

                        await SaveLocalPropertyValuesAsync(localPropertyValues);

                        count++;
                    }
                }
                else
                    break;
            }

            return count;
        }

        private async Task SaveLocalPropertyValuesAsync(List<PropertyUserValue> propertyValues)
        {
            string propertyValuesJson = JsonSerializer.Serialize(propertyValues);

            await AsyncAwaiter.AwaitAsync(nameof(PropertyService) + localTaskPropertyModelsFilename, async () =>
            {
                await _fileService.SaveFileToInternalStorageAsync(propertyValuesJson, localTaskPropertyModelsFilename, Constants.PersistentDataDirectory);
            });
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }
    }
}
