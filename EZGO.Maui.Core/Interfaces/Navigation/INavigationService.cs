using EZGO.Maui.Core.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Interfaces.Navigation
{
    /// <summary>
    /// Navigation service.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Pushes the page to the stack asynchronous.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="noHistory">if set to <c>true</c> [no history].</param>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <returns>Task</returns>
        Task PushAsync(Page page, bool noHistory = false, bool animated = false);

        /// <summary>
        /// Pops the asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        Task CloseAsync();

        /// <summary>
        /// Determinise whenever passed page type exist within navigation stack
        /// </summary>
        /// <returns></returns>
        bool IsInNavigationStack(Type type);

        /// <summary>
        /// Pushes the modal asynchronous.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>Task</returns>
        Task PushModalAsync(Page page);

        /// <summary>
        /// Pops the modal asynchronous.
        /// </summary>
        /// <returns>Page</returns>
        Task<Page> PopModalAsync();

        /// <summary>
        /// Pops to root asynchronous.
        /// </summary>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <returns>Task</returns>
        Task PopToRootAsync(bool animated = false);

        /// <summary>
        /// Inserts the page before.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="before">The before.</param>
        Task InsertPageBeforeAsync(Page page, Page before);

        /// <summary>
        /// Removes the page.
        /// </summary>
        /// <param name="page">The page.</param>
        Task RemovePageAsync(Page page);

        /// <summary>
        /// Navigates to the specified View that is mapped to the ViewModelType
        /// </summary>
        /// <typeparam name="TViewModelType">The type of the view model type.</typeparam>
        /// <param name="noHistory">if set to <c>true</c> [no history].</param>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <param name="viewModel">The viewmodel.</param>
        /// <returns>A task</returns>
        Task NavigateAsync<TViewModelType>(bool noHistory = false, bool animated = true, TViewModelType viewModel = null) where TViewModelType : BaseViewModel;

        /// <summary>
        /// Gets the current page.
        /// </summary>
        /// <returns>The current page.</returns>
        Page GetCurrentPage();

        /// <summary>
        /// Removes the last pages asynchronous.
        /// </summary>
        /// <param name="pageAmountToRemove">The amount of pages to remove.</param>
        /// <returns>Task.</returns>
        Task RemoveLastPagesAsync(int pageAmountToRemove);

        /// <summary>
        /// Gets current page caount on the navigation stack.
        /// </summary>
        /// <returns>Number of pages on the navigation stack.</returns>
        int GetNavigationStackCount();

        Task PopOrNavigateToPage<T>(Type type, T viewModel = null) where T : BaseViewModel;
    }
}
