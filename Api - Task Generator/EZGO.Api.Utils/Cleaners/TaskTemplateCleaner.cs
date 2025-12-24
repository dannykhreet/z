using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Cleaners
{
    /// <summary>
    /// TaskTemplateCleaner; TaskTemplate cleaner, cleans incorrect data from task templates.
    /// </summary>
    public static class TaskTemplateCleaner
    {
        public static TaskTemplate CleanTaskTemplate(TaskTemplate taskTemplate)
        {
            if(!string.IsNullOrEmpty(taskTemplate.Picture))
            {
                taskTemplate.Picture = Cleaners.MediaCleaner.CleanPicture(taskTemplate.Picture);
            }

            if (!string.IsNullOrEmpty(taskTemplate.Video))
            {
                taskTemplate.Video = Cleaners.MediaCleaner.CleanVideo(taskTemplate.Video);
            }

            if (!string.IsNullOrEmpty(taskTemplate.VideoThumbnail))
            {
                taskTemplate.VideoThumbnail = Cleaners.MediaCleaner.CleanPicture(taskTemplate.VideoThumbnail);
            }

            if(taskTemplate.Steps != null && taskTemplate.Steps.Count > 0)
            {
                for(var i = 0; i < taskTemplate.Steps.Count; i++)
                {
                    taskTemplate.Steps[i] = CleanStep(taskTemplate.Steps[i]);
                }
            }

            return taskTemplate;
        }

        public static Step CleanStep(Step step)
        {
            if (!string.IsNullOrEmpty(step.Picture))
            {
                step.Picture = Cleaners.MediaCleaner.CleanPicture(step.Picture);
            }

            if (!string.IsNullOrEmpty(step.Video))
            {
                step.Video = Cleaners.MediaCleaner.CleanVideo(step.Video);
            }


            if (!string.IsNullOrEmpty(step.VideoThumbnail))
            {
                step.VideoThumbnail = Cleaners.MediaCleaner.CleanPicture(step.VideoThumbnail);
            }


            return step;
        }
    }
}
