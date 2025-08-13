using EZGO.Maui.Core.Classes;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Actions
{
    public class BasicActionCommentModel : NotifyPropertyChanged
    {
        public LocalDateTime ModifiedAt { get; set; }

        public string CreatedBy { get; set; }

        public int UserId { get; set; }

        public string Comment { get; set; }

        public bool CommentIsVisible => !string.IsNullOrWhiteSpace(Comment);

        public List<MediaItem> LocalMediaItems { get; set; }

        public MediaItem VideoMediaItem { get; set; }

        public List<MediaItem> ImageMediaItems { get; set; }

        private bool unPosted;
        public bool UnPosted
        {
            get => unPosted;
            set
            {
                unPosted = value;

                OnPropertyChanged();
            }
        }

        public int Id { get; set; }
        public string LocalId { get; set; }
        public int? LocalActionId { get; set; }
    }
}
