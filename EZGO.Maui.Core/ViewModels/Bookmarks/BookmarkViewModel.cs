using System;
using System.Threading.Tasks;
using System.Windows.Input;
using EZGO.Api.Models;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Bookmarks;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using BarcodeScanning;

namespace EZGO.Maui.Core.ViewModels.Bookmarks
{
    public class BookmarkViewModel : BaseViewModel
    {
        private readonly IBookmarkService _bookmarkService;

        public ICommand ScanQR => new Command<BarcodeResult[]>((BarcodeResult[] result) =>
        {
            ExecuteLoadingAction(async () => { await QRCodeScanned(result); });
        }, CanExecuteCommands);


        private async Task QRCodeScanned(BarcodeResult[] result)
        {
            if (result.Length > 0)
            {
                try
                {
                    var bookmark = JsonSerializer.Deserialize<Bookmark>(result.FirstOrDefault().DisplayValue);
                    await _bookmarkService.ParseQRCode(bookmark);
                }
                //user has no access to the item
                catch (UnauthorizedAccessException)
                {
                    await HandleUnauthorizedException();
                }
            }
        }

        private async Task HandleUnauthorizedException()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                Page page = NavigationService.GetCurrentPage();
                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.qrScannerNoAccess);
                await page.DisplayActionSheet(result, null, cancel);
            });
        }

        public BookmarkViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService, IBookmarkService bookmarkService) : base(navigationService, userService, messageService, actionsService)
        {
            _bookmarkService = bookmarkService;
        }
    }
}

