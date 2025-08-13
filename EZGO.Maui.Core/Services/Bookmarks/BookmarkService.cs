using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using EZGO.Api.Models;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Bookmarks;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using EZGO.Maui.Core.ViewModels.Audits;
using EZGO.Maui.Core.ViewModels.Checklists;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using EZGO.Maui.Core.Models.Checklists;

namespace EZGO.Maui.Core.Services.Bookmarks
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IChecklistService _checklistService;
        private readonly INavigationService _navigationService;
        private readonly IAuditsService _auditsService;
        private readonly IInstructionsService _instructionsService;

        private static readonly SemaphoreSlim ParsingQRLock = new SemaphoreSlim(1, 1);


        public BookmarkService(
           IChecklistService checklistService,
           IAuditsService auditsService,
           INavigationService navigationService,
           IInstructionsService instructionsService)
        {
            _checklistService = checklistService;
            _navigationService = navigationService;
            _auditsService = auditsService;
            _instructionsService = instructionsService;
        }

        public async Task ParseQRCode(Bookmark bookmark)
        {
            if (bookmark == null || bookmark.Guid == Guid.Empty)
                return;

            await ParsingQRLock.WaitAsync();

            try
            {
                HttpClient httpClient = Statics.AppHttpClient;
                httpClient.BaseAddress = new Uri(Statics.ApiUrl);

                var uri = $"bookmark/{bookmark.Guid}";

                HttpResponseMessage response = await httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var apiBookmark = await response.Content.ReadAsJsonAsync<Bookmark>();
                    await ParseBookmark(apiBookmark);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        throw new UnauthorizedAccessException();
                }
            }
            finally
            {
                ParsingQRLock.Release();
            }
        }

        private async Task ParseBookmark(Bookmark bookmark)
        {
            if (bookmark == null)
                return;

            switch (bookmark.ObjectType)
            {
                case EZGO.Api.Models.Enumerations.ObjectTypeEnum.ChecklistTemplate:
                    await NavigateToChecklistTemplate(bookmark);
                    break;
                case EZGO.Api.Models.Enumerations.ObjectTypeEnum.AuditTemplate:
                    await NavigateToAuditTemplate(bookmark);
                    break;
                case EZGO.Api.Models.Enumerations.ObjectTypeEnum.WorkInstructionTemplate:
                    await NavigateToWorkInstructionTemplate(bookmark);
                    break;
                default:
                    return;
            }
        }


        private async Task RemoveBookmarkPageFromNavigationStack()
        {
            var bookmarkPage = Application.Current.MainPage.Navigation.NavigationStack.Where(p => p is IBookmarkPage).FirstOrDefault();
            if (bookmarkPage != null)
                await _navigationService.RemovePageAsync(bookmarkPage);
        }

        private async Task NavigateToWorkInstructionTemplate(Bookmark bookmark)
        {
            var workInstructionTemplate = await _instructionsService.GetInstructionFromApi(bookmark.ObjectId, refresh: true);
            using var scope = App.Container.CreateScope();

            var vm = scope.ServiceProvider.GetService<InstructionsItemsViewModel>();
            vm.Instructions = new System.Collections.Generic.List<InstructionsModel>() { workInstructionTemplate };
            vm.SelectedInstruction = workInstructionTemplate ?? new InstructionsModel();
            vm.IsFromDeeplink = true;
            vm.IsMenuVisible = false;
            vm.WorkInstructionTemplateId = bookmark.ObjectId;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _navigationService.NavigateAsync(viewModel: vm);
                await RemoveBookmarkPageFromNavigationStack();
            });
        }

        private async Task NavigateToAuditTemplate(Bookmark bookmark)
        {
            var auditTemplate = await _auditsService.GetAuditTemplateWithIncludesAsync(bookmark.ObjectId, refresh: true);
            using var scope = App.Container.CreateScope();
            var taskTemplatesViewModel = scope.ServiceProvider.GetService<AuditTaskTemplatesViewModel>();
            taskTemplatesViewModel.AuditTemplateId = auditTemplate.Id;
            taskTemplatesViewModel.auditTemplate = auditTemplate;
            taskTemplatesViewModel.IsFromBookmark = true;
            taskTemplatesViewModel.PagesFromDeepLink = 1;
            taskTemplatesViewModel.IsMenuVisible = false;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _navigationService.NavigateAsync(viewModel: taskTemplatesViewModel);
                await RemoveBookmarkPageFromNavigationStack();
            });
        }

        private async Task NavigateToChecklistTemplate(Bookmark bookmark)
        {
            var checklistTemplate = await _checklistService.GetChecklistTemplateWithTaskTemplatesAsync(bookmark.ObjectId, refresh: true);

            using var scope = App.Container.CreateScope();

            if (checklistTemplate.HasIncompleteChecklists ?? false)
            {
                var incompleteChecklistsViewModel = scope.ServiceProvider.GetService<IncompleteChecklistsViewModel>();
                incompleteChecklistsViewModel.ChecklistTemplateId = checklistTemplate.Id;
                incompleteChecklistsViewModel.Picture = checklistTemplate.Picture;
                incompleteChecklistsViewModel.PagesFromDeepLink = 1;
                incompleteChecklistsViewModel.IsFromBookmark = true;
                incompleteChecklistsViewModel.IsMenuVisible = false;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await _navigationService.NavigateAsync(viewModel: incompleteChecklistsViewModel);
                    await RemoveBookmarkPageFromNavigationStack();
                });
                return;
            }
            var taskTemplatesViewModel = scope.ServiceProvider.GetService<TaskTemplatesViewModel>();
            taskTemplatesViewModel.ChecklistTemplateId = checklistTemplate.Id;
            taskTemplatesViewModel.selectedChecklist = checklistTemplate;
            taskTemplatesViewModel.PagesFromDeepLink = 1;
            taskTemplatesViewModel.IsFromBookmark = true;
            taskTemplatesViewModel.IsMenuVisible = false;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _navigationService.NavigateAsync(viewModel: taskTemplatesViewModel);
                await RemoveBookmarkPageFromNavigationStack();
            });
        }
    }
}
