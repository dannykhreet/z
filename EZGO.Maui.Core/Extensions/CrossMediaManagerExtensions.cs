using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks;
using MediaManager;

namespace EZGO.Maui.Core.Extensions
{
    public static class CrossMediaManagerExtensions
    {
        public static async Task PlayMediaItem(this IMediaManager manager, MediaItem mediaItem)
        {
            if (mediaItem == null)
                return;

            if (mediaItem.IsLocalFile)
            {
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await manager.Play(mediaItem.MediaFile.Path);
                    });
                }
                catch { }
            }
            else
            {
                try
                {
                    var url = await AppUrlsResolver.Video(mediaItem.VideoUrl);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await manager.Play(url);
                    });
                }
                catch { }
            }
        }

        public static async Task PlayFromItem(this IMediaManager manager, IDetailItem item)
        {
            if (item != null && item.HasVideo)
            {
                var url = item.Video;

                if (!item.IsLocalMedia)
                    url = await AppUrlsResolver.Video(item.Video);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await manager.Play(url);
                });
            }
        }


        public static async Task PlayFromTaskTemplate(this IMediaManager manager, BasicTaskTemplateModel taskTemplate)
        {
            if (taskTemplate != null && taskTemplate.HasVideo)
            {
                var url = await AppUrlsResolver.Video(taskTemplate.Video);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await manager.Play(url);
                });
            }
        }

        public static async Task PlayFromTaskStep(this IMediaManager manager, StepModel stepModel)
        {
            if (stepModel != null && stepModel.HasVideo)
            {
                var url = await AppUrlsResolver.Video(stepModel.Video);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await manager.Play(url);
                });
            }
        }

        public static async Task PlayFromTask(this IMediaManager manager, BasicTaskModel taskModel)
        {
            if (taskModel != null && taskModel.HasVideo)
            {
                var url = await AppUrlsResolver.Video(taskModel.Video);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await manager.Play(url);
                });
            }
        }

        public static async Task PlayFromInstruction(this IMediaManager manager, InstructionItem instructionItem)
        {
            if (instructionItem != null && instructionItem.HasVideo)
            {
                var url = await AppUrlsResolver.Video(instructionItem.Video);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await manager.Play(url);
                });
            }
        }
    }
}
