using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.FlattenDataManagers
{
    public interface IFlattenAutomatedManager
    {
        Task<bool> FlattenCurrentTemplatesAll();
        Task<bool> FlattenCurrentTemplatesAll(int companyId);
        Task<bool> FlattenCurrentTemplatesType(int companyId, TemplateTypeEnum templateType);
        Task<bool> SaveFlattendData(int companyId, int userId, int templateId, string version, DateTime creationDate, TemplateTypeEnum templateType, string flattenedData);
        Task<bool> AddFlattenerLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "");
        Task<SortedList<DateTime, string>> RetrieveVersionsList(int companyId, int templateId, TemplateTypeEnum templateType);
        Task<string> RetrieveVersionJson(int companyId, int templateId, string version, TemplateTypeEnum templateType);
        List<Exception> GetPossibleExceptions();

    }
}
