using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Interfaces.Instructions
{
    public interface IInstructionsService : IDisposable
    {
        Task<List<InstructionsModel>> GetInstructions(bool refresh = false, bool isFromSyncService = false);
        Task<InstructionsModel> GetInstruction(int id, bool refresh = false);
        Task<InstructionsModel> GetInstructionFromApi(int id, bool refresh = false);
        Task SetWorkInstructionRelations(List<BasicTaskModel> tasks, List<InstructionsModel> allInstructions = null);
        Task SetWorkInstructionRelations(List<TaskTemplateModel> taskTemplates, List<InstructionsModel> allInstructions = null);
        Task<List<InstructionsModel>> GetInstructionsForCurrentArea(bool refresh = false, bool isFromSyncService = false);
        Task<InstructionsModel> GetInstructionForCurrentArea(int id);
        Task<List<WorkInstructionTemplateChangeNotification>> GetWorkInstructionChanges(int id);
        Task<bool> ConfirmWorkInstructionChanges(int id);
    }
}
