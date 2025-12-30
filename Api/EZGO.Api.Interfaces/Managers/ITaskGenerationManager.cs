using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.TaskGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ITaskGenerationManager
    {
        Task<bool> GenerateOneTimeOnlyCompany(int companyId, int? templateId = null);
        Task<bool> GenerateWeeklyCompany(int companyId, int? templateId = null);
        Task<bool> GenerateMonthlyCompany(int companyId, int? templateId = null);
        Task<bool> GenerateShiftsCompany(int companyId, int? templateId = null);
        Task<bool> GeneratePeriodDayCompany(int companyId, int? templateId = null);
        Task<bool> GeneratePeriodHourCompany(int companyId, int? templateId = null);
        Task<bool> GeneratePeriodMinuteCompany(int companyId, int? templateId = null);
        Task<bool> GenerateDynamicDayCompany(int companyId, int? templateId = null);
        Task<bool> GenerateDynamicHourCompany(int companyId, int? templateId = null);
        Task<bool> GenerateDynamicMinuteCompany(int companyId, int? templateId = null);
        Task<bool> GenerateSpecificTemplate(int companyId, int templateId, RecurrencyTypeEnum recurrencyType);
        Task<bool> GenerateSpecificTemplateBasedOnTask(int companyId, int taskId);
        Task<bool> GenerateAllCompany(int companyId, CancellationToken stoppingToken);
        Task<bool> GenerateAll(CancellationToken stoppingToken);
        Task<string> GetTemplateRecurrencyTypeBasedOnTask(int companyId, int taskId);
        Task<bool> CheckGenerationCompany(int companyId);
        Task<List<int>> GetRunnableHours();
        Task<List<int>> GetRunnableMinutes();
        Task<string> GetRunnableType();
        Task<bool> AddGenerationLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "");

    }
}
