using System;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class StatusFactory
    {
        public static IStatus<T> CreateStatus<T>()
        {
            if (typeof(T) == typeof(ActionStatusEnum))
            {
                return (IStatus<T>)new ActionStatuses();
            }
            else if (typeof(T) == typeof(TaskStatusEnum))
            {
                return (IStatus<T>)new TaskStatuses();
            }
            else if (typeof(T) == typeof(InstructionTypeEnum))
            {
                return (IStatus<T>)new InstructionStatuses();
            }
            else if (typeof(T) == typeof(SkillTypeEnum))
            {
                return (IStatus<T>)new SkillStatuses();
            }

            throw new InvalidOperationException();
        }

        private StatusFactory()
        {
        }
    }
}
