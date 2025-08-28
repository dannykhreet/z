using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Models.Actions
{
    public class BasicActionsModel : NotifyPropertyChanged, IItemFilter<ActionStatusEnum>
    {
        public int Id { get; set; }
        public List<int> AssignedUserIds { get; set; }

        public List<MediaItem> LocalMediaItems { get; set; }

        public int? LocalId { get; set; }

        private List<string> images;
        public List<string> Images
        {
            get => images;
            set
            {
                images = value;

                OnPropertyChanged();
            }
        }

        private string image1;
        public string Image1
        {
            get => image1;
            set
            {
                image1 = value;

                OnPropertyChanged();
            }
        }

        private string image2;
        public string Image2
        {
            get => image2;
            set
            {
                image2 = value;

                OnPropertyChanged();
            }
        }
        private string image3;
        public string Image3
        {
            get => image3;
            set
            {
                image3 = value;

                OnPropertyChanged();
            }
        }
        private string image4;
        public string Image4
        {
            get => image4;
            set
            {
                image4 = value;

                OnPropertyChanged();
            }
        }
        private string image5;
        public string Image5
        {
            get => image5;
            set
            {
                image5 = value;

                OnPropertyChanged();
            }
        }
        private string image6;
        public string Image6
        {
            get => image6;
            set
            {
                image6 = value;

                OnPropertyChanged();
            }
        }

        private int mediaCount;

        public int MediaCount
        {
            get => mediaCount;
            set
            {
                mediaCount = value;

                OnPropertyChanged();
            }
        }

        private ActionStatusEnum status;
        public ActionStatusEnum FilterStatus
        {
            get => status;
            set
            {
                status = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowChatInput));
            }
        }

        private int unviewedCommentNr;
        public int UnviewedCommentNr
        {
            get => unviewedCommentNr;
            set
            {
                unviewedCommentNr = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasUnseenComments));
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool SendToUltimo { get; set; }
        public string UltimoStatus { get; set; }
        public DateTime? UltimoStatusDateTime { get; set; }
        public LocalDateTime DueDate { get; set; }
        public LocalDateTime CreatedAt { get; set; }
        public LocalDateTime ModifiedAt { get; set; }
        public LocalDateTime LastCommentDate { get; set; }
        public string CreatedBy { get; set; }
        public int CreatedById { get; set; }
        public bool IsResolved { get; set; }
        private bool isParticipant;
        public bool IsParticipant
        {
            get => isParticipant;
            set
            {
                isParticipant = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowChatInput));
            }
        }

        public bool IsMine { get; set; }
        public bool HasImages { get; set; }

        // To be filled on detail
        public List<AreaBasic> AssignedAreas { get; set; }

        private List<UserBasic> assignedUsers;
        public List<UserBasic> AssignedUsers
        {
            get => assignedUsers;
            set
            {
                assignedUsers = value;

                OnPropertyChanged();
            }
        }

        public LocalDateTime ResolvedAt { get; set; }
        private List<ActionCommentModel> comments;
        public List<ActionCommentModel> Comments
        {
            get => comments;
            set
            {
                comments = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasComments));
            }
        }
        public int TaskTemplateId { get; set; }
        public int TaskId { get; set; }
        public int CompanyId { get; set; }

        private ActionParentBasic parent;
        public ActionParentBasic Parent
        {
            get => parent;
            set
            {
                parent = value;

                OnPropertyChanged();
            }
        }

        private List<string> videos;

        public List<string> Videos
        {
            get => videos;
            set
            {
                videos = value;

                OnPropertyChanged();
            }
        }

        private List<string> videoThumbNails;

        public List<string> VideoThumbNails
        {
            get => videoThumbNails;
            set
            {
                videoThumbNails = value;

                OnPropertyChanged();
            }
        }

        public bool ShowChatInput => FilterStatus != ActionStatusEnum.Solved;

        public bool HasUnseenComments => UnviewedCommentNr > 0;

        public bool HasComments => Comments != null && Comments.Any();

        public bool RetrieveImagesOffline { get; set; } = false;

        public List<Tag> Tags { get; set; }

        public bool ContainsTags => Tags?.Count > 0;

        public ActionsModel ToModel()
        {
            ActionsModel result = new ActionsModel
            {
                AssignedAreas = AssignedAreas,
                AssignedUsers = AssignedUsers,
                CreatedAt = CreatedAt.ToDateTimeUnspecified(),
                ModifiedAt = ModifiedAt.ToDateTimeUnspecified(),
                ResolvedAt = ResolvedAt.ToDateTimeUnspecified(),
                Comment = Name,
                Comments = Comments,
                CompanyId = CompanyId,
                CreatedBy = CreatedBy,
                CreatedById = CreatedById,
                Description = Description,
                DueDate = DueDate.ToDateTimeUnspecified(),
                Id = Id,
                Images = Images,
                IsResolved = IsResolved,
                Parent = Parent,
                TaskId = TaskId,
                TaskTemplateId = TaskTemplateId,
                UnviewedCommentNr = UnviewedCommentNr,
                Videos = Videos,
                VideoThumbNails = VideoThumbNails,
                LocalMediaItems = LocalMediaItems,
                LocalId = LocalId,
                Tags = Tags,
                SendToUltimo = SendToUltimo,
                UltimoStatus = UltimoStatus,
                UltimoStatusDateTime = UltimoStatusDateTime
            };

            return result;
        }
    }

    public class BasicActionsComparer : IEqualityComparer<BasicActionsModel>
    {
        public bool Equals(BasicActionsModel x, BasicActionsModel y)
        {
            if (x.Id == y.Id)
            {
                return true;
            }
            else
                return false;
        }

        public int GetHashCode(BasicActionsModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }

}
