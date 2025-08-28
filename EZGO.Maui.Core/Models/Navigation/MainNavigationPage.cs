using EZGO.Maui.Core.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Syncfusion.Maui.DataSource.Extensions;
using System.Diagnostics;

namespace EZGO.Maui.Core.Models.Navigation
{
    public class MainNavigationPage : NavigationPage
    {
        public MainNavigationPage() : base()
        {

        }

        public MainNavigationPage(Page page) : base(page)
        {
            Popped += MainNavigationPage_Popped;
            PoppedToRoot += MainNavigationPage_PoppedToRoot;
            //Causing errors because there is no RemovePageRequested event in MAUI
            //RemovePageRequested += MainNavigationPage_RemovePageRequested;
        }

        private void MainNavigationPage_RemovePageRequested(object sender, NavigationRequestedEventArgs e)
        {
            CleanupPage(e.Page);
        }

        private void MainNavigationPage_PoppedToRoot(object sender, NavigationEventArgs e)
        {
            if (e is PoppedToRootEventArgs args)
            {
                // Clean up all popped pages
                if (args.PoppedPages != null)
                    args.PoppedPages.ForEach(x => CleanupPage(x));
            }
        }

        private void MainNavigationPage_Popped(object sender, NavigationEventArgs e)
        {
            // Perform cleanup
            CleanupPage(e.Page);
        }

        private void CleanupPage(Page page)
        {
            if (page is TabbedPage tabbedPage)
            {
                tabbedPage.Children.ForEach(p =>
                {
                    var bc = p.BindingContext as BaseViewModel;
                    p.Appearing -= bc.AppearingHandler;
                    p.Disappearing -= bc.DisappearingHandler;
                });
            }
            if (page.BindingContext is BaseViewModel viewmodel)
            {
                page.Appearing -= viewmodel.AppearingHandler;
                page.Disappearing -= viewmodel.DisappearingHandler;
                page.BindingContext = null;
                if (page is IDisposable disposablePage) disposablePage.Dispose();
                MainThread.BeginInvokeOnMainThread(viewmodel.Dispose);
            }
            page.Parent = null;

#if DEBUG
            Debug.WriteLine($"Cleaned page: '{page.GetType().Name}'");
#endif
            page = null;
        }
    }
}