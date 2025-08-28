using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.ViewModels;
using System.Diagnostics;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// View factory.
    /// </summary>
    public class ViewFactory
    {
        /// <summary>
        /// Returns the ViewModel-View Dictionary with the ViewModel-type as key
        /// <para>Please note that this is explicitly set to a private static readonly variable.</para>
        /// </summary>
        private static readonly IDictionary<Type, Type> viewModelViewDictionary = new Dictionary<Type, Type>();

        /// <summary>
        /// Pages that have been created through the lifetime of the application.
        /// </summary>
        private static readonly IDictionary<Type, Page> PageCache = new Dictionary<Type, Page>();

        /// <summary>
        /// Register a View with ViewModel
        /// </summary>
        /// <typeparam name="TView">Type of View to register</typeparam>
        /// <typeparam name="TViewModel">Type of ViewModel to register</typeparam>
        public static void RegisterView<TView, TViewModel>()
            where TView : Page
            where TViewModel : BaseViewModel
        {
            if (viewModelViewDictionary.ContainsKey(typeof(TViewModel)))
                throw new InvalidOperationException("The ViewModel has already been registered.");

            if (viewModelViewDictionary.Values.Contains(typeof(TView)))
                throw new InvalidOperationException("The View has already been registered.");

            // Add the ViewModel-View to the dictionary
            viewModelViewDictionary[typeof(TViewModel)] = typeof(TView);
        }

        /// <summary>
        /// Unregisters a ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">Type of ViewModel to deregister</typeparam>
        public static void UnregisterView<TViewModel>()
            where TViewModel : BaseViewModel
        {
            if (viewModelViewDictionary.ContainsKey(typeof(TViewModel)))
                viewModelViewDictionary.Remove(typeof(TViewModel));
        }

        /// <summary>
        /// Unregisters all ViewModels
        /// </summary>
        public static void UnregisterAll()
        {
            viewModelViewDictionary.Clear();
        }

        /// <summary>
        /// Creates a Page object and returns the Page, based on the given View and ViewModel class
        /// </summary>
        /// <param name="viewModel">[Optional] Init method</param>
        /// <param name="args">[Optional] The view arguments</param>
        /// <typeparam name="TViewModel">Type of ViewModel to create the page for</typeparam>
        /// <returns>A Page object</returns>
        public static Page CreateView<TViewModel>(TViewModel viewModel = null, params object[] args)
            where TViewModel : BaseViewModel
        {
#if DEBUG
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var lastElapsed = 0L;
            var timing = "";
#endif
            // Get the type of the view model
            var viewModelType = typeof(TViewModel);

            // Check if mapping exists
            if (!viewModelViewDictionary.ContainsKey(viewModelType))
                throw new InvalidOperationException($"No registration found for viewmodel {viewModelType.Name}");

            // Get the type from the dictionary
            Type viewType = viewModelViewDictionary[viewModelType];

            Page view;
            try
            {
                // Create the page 
                view = (Page)Activator.CreateInstance(viewType, args);

#if DEBUG
                timing += $"Creating new '{viewType.Name}': {watch.ElapsedMilliseconds - lastElapsed}ms";
                lastElapsed = watch.ElapsedMilliseconds;
#endif
                // Sets right to left orientation if region arabic
                if (Settings.IsRightToLeftLanguage)
                    view.FlowDirection = (FlowDirection)LayoutDirection.RightToLeft;
                // Create the view model if doesn't exists
                using var scope = App.Container.CreateScope();
                viewModel ??= scope.ServiceProvider.GetService<TViewModel>();
            }
            catch (Exception exception)
            {
                Debugger.Break();
                throw new InvalidOperationException($"Could not create instance of type {viewModelType}", exception);
            }

            // NOTE These events are unhooked in MainNavigationPage
            view.Appearing += viewModel.AppearingHandler;
            view.Disappearing += viewModel.DisappearingHandler;

            // Set the binding context
            view.BindingContext = viewModel;

            if (view is IViewResizer viewResizer)
                ViewSizeManager.ResizeView(viewResizer);

#if DEBUG
            watch.Stop();
            Debug.WriteLine(timing, "ViewCreation");
#endif
            return view;
        }

        /// <summary>
        /// Views the type for model.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No registration found for viewmodel {typeof(TViewModel).Name}</exception>
        public static Type ViewTypeForModel<TViewModel>()
        {
            Type returnType;

            if (viewModelViewDictionary.ContainsKey(typeof(TViewModel)))
                returnType = viewModelViewDictionary[typeof(TViewModel)];
            else
                throw new InvalidOperationException($"No registration found for viewmodel {typeof(TViewModel).Name}.");

            return returnType;
        }

        public static Type GetView(Type type)
        {
            bool isSuccess = viewModelViewDictionary.TryGetValue(type, out Type pageType);
            if (!isSuccess) return null;
            return pageType;
        }
    }
}