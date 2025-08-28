using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using System.Diagnostics;
using System.Text;

namespace EZGO.Maui.Core.Services.Navigation
{
    /// <summary>
    /// Navigation service.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private const string LockKey = nameof(NavigationService);

        /// <summary>
        /// Gets or sets the main page.
        /// </summary>
        /// <value>
        /// The main page.
        /// </value>
        private Page MainPage
        {
            get => Application.Current.MainPage;
            set => Application.Current.MainPage = value;
        }

        private INavigation Navigation => MainPage?.Navigation;

        public int GetNavigationStackCount() => MainPage?.Navigation.NavigationStack.Count ?? -1;

        /// <summary>
        /// Gets the current page.
        /// </summary>
        /// <returns>
        /// The current page.
        /// </returns>
        public Page GetCurrentPage()
        {
            Page page = null;

            if (Application.Current.MainPage?.Navigation.NavigationStack != null)
                page = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

            return page;
        }

        /// <summary>
        /// Pushes the page to the stack
        /// </summary>
        /// <param name="page">Page to be pushed to the stack</param>
        /// <param name="noHistory">if set to <c>true</c> [no history].</param>
        /// <param name="animated">if set to <c>true</c> [animated].</param>        
        /// <returns>An awaitable Task</returns>
        public Task PushAsync(Page page, bool noHistory = false, bool animated = false)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));


            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, async () =>
            {
                if (MainPage == null)
                    return;

                if (noHistory)
                {
                    // Insert a page before root
                    MainPage.Navigation.InsertPageBefore(page, ((NavigationPage)MainPage).RootPage);

                    // And do pop to root
                    await MainPage.Navigation.PopToRootAsync(animated);
                }
                else
                    await MainPage.Navigation.PushAsync(page);
            });
        }

        /// <summary>
        /// Pops the asynchronous.
        /// </summary>
        /// <returns>
        /// Task
        /// </returns>
        public Task CloseAsync()
        {
            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, async () =>
            {
                Page page = null;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    page = await MainPage.Navigation.PopAsync();
                });

                if (page != null)
                {
                    (page as IDisposable)?.Dispose();
                    (page.BindingContext as IDisposable)?.Dispose();
                    page.BindingContext = null;
                    page.Parent = null;
                }
            });
        }

        /// <summary>
        /// Pops to root asynchronous.
        /// </summary>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <returns>
        /// Task
        /// </returns>
        public Task PopToRootAsync(bool animated = false)
        {
            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, async () =>
            {
                await MainPage.Navigation.PopToRootAsync(animated);
            });
        }

        /// <summary>
        /// Inserts the page before.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="before">The before.</param>
        public Task InsertPageBeforeAsync(Page page, Page before)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (before == null)
                throw new ArgumentNullException(nameof(before));

            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, () =>
            {
                if (!MainPage.Navigation.NavigationStack.Contains(page))
                    MainPage.Navigation.InsertPageBefore(page, before);
            });
        }

        /// <summary>
        /// Removes the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <exception cref="ArgumentNullException">page</exception>
        public Task RemovePageAsync(Page page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, () =>
            {
                MainPage.Navigation.RemovePage(page);
            });
        }

        /// <summary>
        /// Pushes the modal page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>
        /// Task
        /// </returns>
        /// <exception cref="ArgumentNullException">page</exception>
        public Task PushModalAsync(Page page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, async () =>
            {
                await MainPage.Navigation.PushModalAsync(page);
            });
        }

        /// <summary>
        /// Pop the first modal page from the stack
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public Task<Page> PopModalAsync()
        {
            return AsyncAwaiter.AwaitResultAsync(LockKey, async () =>
            {
                return await MainPage.Navigation.PopModalAsync();
            });
        }

#if DEBUG
        private static List<(string, WeakReference, string, WeakReference)> VMs = new List<(string, WeakReference, string, WeakReference)>();
#endif

        /// <summary>
        /// Navigates to the specified View that is mapped to the ViewModelType
        /// </summary>
        /// <typeparam name="TViewModelType">The type of the view model type.</typeparam>
        /// <param name="noHistory">if set to <c>true</c> [no history].</param>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <param name="viewModel">The viewmodel.</param>
        /// <returns>A task</returns>
        public Task NavigateAsync<TViewModelType>(bool noHistory = false, bool animated = true, TViewModelType viewModel = null)
            where TViewModelType : BaseViewModel
        {
            return AsyncAwaiter.ExecuteIfPossibleAsync(LockKey, async () =>
            {
#if DEBUG
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var lastElapsed = 0L;
                var timing = "[Navigation]: ";
#endif

                // Get the page
                Page page = ViewFactory.CreateView(viewModel);
#if DEBUG
                timing += $"Create view: {watch.ElapsedMilliseconds - lastElapsed}ms";
                lastElapsed = watch.ElapsedMilliseconds;
#endif
                // If the new page should be the only page visible (root)
                if (noHistory)
                {

                    // Insert a page before root
                    MainPage.Navigation.InsertPageBefore(page, ((NavigationPage)MainPage).RootPage);

                    // And do pop to root
                    await MainPage.Navigation.PopToRootAsync(animated);

#if DEBUG
                    timing += $", No history navigation: {watch.ElapsedMilliseconds - lastElapsed}ms";
                    lastElapsed = watch.ElapsedMilliseconds;
#endif
                }
                else
                {
                    // If history should be retained just push the page
                    await MainPage.Navigation.PushAsync(page, false);
#if DEBUG
                    timing += $", Push page: {watch.ElapsedMilliseconds - lastElapsed}ms";
                    lastElapsed = watch.ElapsedMilliseconds;
                    watch.Stop();
#endif
                }
#if DEBUG
                Debug.WriteLine(timing, "NavTiming");
                //GC.Collect();                     //
                //GC.WaitForPendingFinalizers();    // For testing memory leaks
                //GC.Collect();                     //

                // For testing output
                VMs.Add((page.BindingContext.GetType().Name, new WeakReference(page.BindingContext), page.GetType().Name, new WeakReference(page)));
                var aliveCount = VMs.Count(x => x.Item2.IsAlive);
                var alivePages = VMs.Count(x => x.Item4.IsAlive);

                StringBuilder line = new StringBuilder("\r\n");
                foreach (var vm in VMs)
                {
                    line.Append($"NavLife: {(vm.Item4.IsAlive ? "[Alive]" : " [Dead]")}: {vm.Item3}\r\nNavLife: \t{(vm.Item2.IsAlive ? "[Alive]" : " [Dead]")}: {vm.Item1}\r\n");
                    //if (vm.Item2.IsAlive) line.Append(LiveCycleHelper.GetAliveObjects(vm.Item2.Target));
                }
                Debug.Write(line);
                Debug.WriteLine($"---------- VMs: {VMs.Count}, Alive: {aliveCount}, Dead: {VMs.Count - aliveCount} ----------", "NavLife");
                Debug.WriteLine($"---------- Pages: {VMs.Count}, Alive: {alivePages}, Dead: {VMs.Count - alivePages} ----------", "NavLife");
#endif
            });
        }

        /// <summary>
        /// Removes the last pages asynchronous.
        /// </summary>
        /// <param name="pageAmountToRemove">The amount of pages to remove.</param>
        public Task RemoveLastPagesAsync(int pageAmountToRemove)
        {
            return AsyncAwaiter.AwaitAsync(LockKey, async () =>
            {
                // TODO
                try
                {
                    if (pageAmountToRemove <= 0)
                        throw new ArgumentOutOfRangeException(nameof(pageAmountToRemove));

                    int pageCount = GetNavigationStackCount();

                    if (pageAmountToRemove > pageCount - 1)
                        throw new ArgumentException("Cannot remove all the pages from the view");

                    // A - B - C - D
                    // When asked to remove 3 pages, first remover B and C and then pop D

                    // Call RemovePage one less time than necessary because that last page will be poped
                    pageAmountToRemove--;

                    // Start from the second to last page
                    var page_i = pageCount - 2;

                    // Remove all the necessary pages
                    while (pageAmountToRemove-- > 0)
                    {
                        Page page = Navigation.NavigationStack[page_i];
                        Navigation.RemovePage(page);

                        page_i--;
                    }
                }
                catch (Exception exception)
                {
                    Debugger.Break();
                    //Crashes.TrackError(exception);
                }

                // Pop the last page
                await MainPage.Navigation.PopAsync();
            });

        }

        public async Task PopOrNavigateToPage<T>(Type type, T viewModel = null) where T : BaseViewModel
        {
            var pageT = ViewFactory.GetView(type);
            var page = MainPage.Navigation.NavigationStack.Where(x =>
            {
                var pageType = x.GetType();
                return pageType == pageT;
            }).FirstOrDefault();
            var root = MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (page != null && page == root)
            {
                await MainPage.Navigation.PopToRootAsync();
            }
            else
            {
                await NavigateAsync<T>(viewModel: viewModel, animated: false, noHistory: true);
            }
        }

        public bool IsInNavigationStack(Type pageType)
        {
            var page = Navigation?.NavigationStack?.FirstOrDefault(stackMember =>
            {
                if (stackMember == null) return false;
                var type = stackMember.GetType();
                return type == pageType;
            });
            return page != null;
        }
    }
}