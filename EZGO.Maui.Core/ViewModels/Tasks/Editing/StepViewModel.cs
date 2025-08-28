using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.ViewModels.Tasks.Editing
{
    public class StepViewModel : NotifyPropertyChanged
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Description { get; set; }
        public int TaskTemplateId { get; set; }
        public MediaItem MediaItem { get; set; }

        public StepViewModel()
        { 
        }

        public StepViewModel(StepModel step)
        {
            Id = step.Id;
            Index = step.Index;
            Description = step.Description;
            TaskTemplateId = step.TaskTemplateId;

            if (step.IsVideo)
                MediaItem = MediaItem.Video(step.Video, step.VideoThumbnail, step.MediaIsLocal);
            else if (step.IsPicture)
                MediaItem = MediaItem.Picture(step.Picture, step.MediaIsLocal);
        }

        public bool IsEmpty() => string.IsNullOrEmpty(Description) && MediaItem == null;
    }
}
