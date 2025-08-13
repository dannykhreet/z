using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.Tags;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Actions
{
    public class ActionsModel : ActionsAction, IBase<BasicActionsModel>
    {
        public new List<UserBasic> AssignedUsers { get; set; } = new List<UserBasic>();
        public new List<AreaBasic> AssignedAreas { get; set; } = new List<AreaBasic>();
        public string AssignedResources => SetAssignedResources();
        public string ApiUri { get; set; }
        public List<MediaItem> LocalMediaItems { get; set; }
        public int? LocalId { get; set; }
        public new List<ActionCommentModel> Comments { get; set; }
        public new int UnviewedCommentNr { get; set; }
        public new int CompanyId { get; set; }

        public LocalDateTime LocalCreatedAt
        {
            get
            {
                if (CreatedAt.HasValue)
                    return Settings.ConvertDateTimeToLocal(CreatedAt.Value.ToLocalTime());
                else return new LocalDateTime();
            }
        }

        public LocalDateTime LocalDueDate
        {
            get
            {
                if (DueDate.HasValue)
                    return Settings.ConvertDateTimeToLocal(DueDate.Value.ToLocalTime());
                else return new LocalDateTime();
            }
        }

        public BasicActionsModel ToBasic()
        {
            Images ??= new List<string>();

            BasicActionsModel result = new BasicActionsModel
            {
                AssignedUserIds = AssignedUsers?.Select(user => user.Id).ToList() ?? new List<int>(),
                AssignedUsers = AssignedUsers ?? new List<UserBasic>(),
                AssignedAreas = AssignedAreas ?? new List<AreaBasic>(),
                Images = Images,
                Image1 = Images.ElementAtOrDefault(0),
                Image2 = Images.ElementAtOrDefault(1),
                Image3 = Images.ElementAtOrDefault(2),
                Image4 = Images.ElementAtOrDefault(3),
                Image5 = Images.ElementAtOrDefault(4),
                Image6 = Images.ElementAtOrDefault(5),
                Name = Comment,
                Comments = Comments,
                Parent = Parent,
                LastCommentDate = Settings.ConvertDateTimeToLocal(LastCommentDate ?? DateTime.MinValue),
                UnviewedCommentNr = UnviewedCommentNr,
                CreatedBy = CreatedBy,
                CreatedById = CreatedById,
                Description = Description,
                CreatedAt = Settings.ConvertDateTimeToLocal(CreatedAt ?? DateTime.MinValue),
                ModifiedAt = Settings.ConvertDateTimeToLocal(ModifiedAt ?? DateTime.MinValue),
                DueDate = Settings.ConvertDateTimeToLocal(DueDate ?? DateTime.Now),
                Id = Id,
                TaskId = TaskId ?? 0,
                TaskTemplateId = TaskTemplateId ?? 0,
                IsResolved = IsResolved ?? false,
                IsParticipant = (CreatedById == UserSettings.Id || (AssignedUsers?.Any(user => user.Id == UserSettings.Id) ?? false)),
                IsMine = CreatedById == UserSettings.Id || UserSettings.RoleType == RoleTypeEnum.Manager,
                FilterStatus = IsResolved ?? false ? ActionStatusEnum.Solved : (DateTime.Today > DueDate) ? ActionStatusEnum.PastDue : ActionStatusEnum.Unsolved,
                Videos = Videos,
                VideoThumbNails = VideoThumbNails,
                LocalMediaItems = LocalMediaItems,
                LocalId = LocalId,
                Tags = Tags,
                SendToUltimo = SendToUltimo,
                UltimoStatus = UltimoStatus,
                UltimoStatusDateTime = UltimoStatusDateTime
            };

            List<string> images = new List<string> { result.Image1, result.Image2, result.Image3, result.Image4, result.Image5, result.Image6 };

            int videoIndex = 0;
            for (int imageIndex = 0; imageIndex < images.Count; imageIndex++)
            {
                if (images[imageIndex] == null)
                {
                    images[imageIndex] = result.VideoThumbNails?.ElementAtOrDefault(videoIndex);
                    videoIndex++;
                }
            }

            result.Image1 = images[0];
            result.Image2 = images[1];
            result.Image3 = images[2];
            result.Image4 = images[3];
            result.Image5 = images[4];
            result.Image6 = images[5];

            result.MediaCount = images.Count(item => !string.IsNullOrEmpty(item));
            result.HasImages = images.Any(item => !string.IsNullOrEmpty(item));

            return result;
        }

        private string SetAssignedResources()
        {
            StringBuilder assignedResourceNames = new StringBuilder();

            if (AssignedUsers != null && AssignedUsers.Any())
            {
                foreach (var item in AssignedUsers)
                {
                    assignedResourceNames.Append(item.Name + ", ");
                }
            }

            if (AssignedAreas != null && AssignedAreas.Any())
            {
                using var scope = App.Container.CreateScope();
                var _workAreaService = scope.ServiceProvider.GetService<IWorkAreaService>();
                var workAreas = _workAreaService?.GetBasicWorkAreasAsync().Result;
                workAreas = _workAreaService?.GetFlattenedBasicWorkAreas(workAreas);
                foreach (var area in AssignedAreas)
                {
                    var item = workAreas.FirstOrDefault(w => w.Id == area.Id);
                    if (item != null)
                    {
                        var splittedFullName = item.FullDisplayName.Split(" -> ");
                        var txt = "";
                        if (splittedFullName.Count() > 2)
                        {
                            txt += "../";
                            txt += string.Join("/", splittedFullName.TakeLast(2));
                        }
                        else
                        {
                            txt = string.Join("/", splittedFullName);
                        }

                        assignedResourceNames.Append(txt + ", ");
                    }
                }
            }

            int lastIndex = assignedResourceNames.ToString().LastIndexOf(',');

            if (lastIndex != -1)
            {
                assignedResourceNames.Remove(lastIndex, 1);
            }

            return assignedResourceNames.ToString();
        }
    }

    public class ActionsComparer : IEqualityComparer<ActionsModel>
    {
        private readonly bool _withLocalActions;
        public ActionsComparer(bool withLocalActions = false)
        {
            _withLocalActions = withLocalActions;
        }
        public bool Equals(ActionsModel x, ActionsModel y)
        {
            if (x.Id == y.Id)
            {
                if (_withLocalActions && x.Id == 0)
                    return false;
                return true;
            }
            else
                return false;
        }

        public int GetHashCode(ActionsModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
