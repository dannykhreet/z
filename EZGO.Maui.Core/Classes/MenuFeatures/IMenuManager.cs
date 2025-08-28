using System.Collections.Generic;
using System.Windows.Input;

namespace EZGO.Maui.Core.Classes.MenuFeatures
{
    public interface IMenuManager
    {
        ICommand NavigateToSectionCommand { get; set; }
        ICommand GoToNextPageCommand { get; set; }
        List<IMenuItem> MenuItems { get; }
        List<IMenuItem> CurrentMenuItems { get; }
        List<IMenuItem> GetMenuItems();
        IMenuItem SelectedItem { get; set; }
        bool IsArrowVisible { get; }
        void SetSelectedMenuItem(bool goToSelectedItem = true);
        static int PageCapability { get; set; }
        void RegisterMenuItems();
        void ReloadMenuItemTranslations();
    }
}