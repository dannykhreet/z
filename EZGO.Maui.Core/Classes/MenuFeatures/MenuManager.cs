using Autofac;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.ViewModels.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MenuFeatures
{
    public class MenuManager : NotifyPropertyChanged, IMenuManager
    {
        public ICommand NavigateToSectionCommand { get; set; }

        public ICommand GoToNextPageCommand { get; set; }

        public MenuManager()
        {
            RegisterMenuItems();

            NavigateToSectionCommand = new Command<object>((obj) =>
            {
                var menuLoc = (MenuLocation)Enum.Parse(typeof(MenuLocation), obj.ToString());

                OnlineShiftCheck.IsShiftChangeAllowed = true;
                _ = OnlineShiftCheck.CheckCycleChange();

                using var scope = App.Container.CreateScope();
                var menuViewModel = scope.ServiceProvider.GetService<MenuViewModel>();
                switch (menuLoc)
                {
                    case MenuLocation.Report:
                        menuViewModel.NavigateToReportCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Actions:
                        menuViewModel.NavigateToActionsCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Audits:
                        menuViewModel.NavigateToAuditsCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Checklist:
                        menuViewModel.NavigateToChecklistTemplatesCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Tasks:
                        menuViewModel.NavigateToTasksCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Instructions:
                        menuViewModel.NavigateToInstructionsCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Assessments:
                        menuViewModel.NavigateToAssessmentsCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Feed:
                        menuViewModel.NavigateToFeedCommand.Execute(menuLoc);
                        break;
                    case MenuLocation.Home:
                        menuViewModel.NavigateToHomeCommand.Execute(menuLoc);
                        break;
                    default:
                        break;
                }
            }, (obj) => true);

            GoToNextPageCommand = new Command(() =>
            {
                if (currentPage * PageCapability < MenuItems.Count())
                {
                    currentPage++;
                    CurrentMenuItems = GetMenuItems();
                }
                else if (currentPage * PageCapability != 0)
                {
                    currentPage--;
                    CurrentMenuItems = GetMenuItems();
                }
                SetSelectedMenuItem(false);
            }, () => true);

            SetPageCapability();

            CurrentMenuItems = GetMenuItems();

            SetSelectedMenuItem();
        }

        public IMenuItem SelectedItem { get; set; }

        public List<IMenuItem> MenuItems { get; private set; }

        public List<IMenuItem> CurrentMenuItems { get; private set; }

        public bool IsLastPage { get; set; }

        private int currentPage = 1;

        public static int PageCapability { get; set; } = 6;

        public bool IsArrowVisible => MenuItems.Count > PageCapability;

        public List<IMenuItem> GetMenuItems()
        {
            var itemsToSkip = (currentPage - 1) * PageCapability;
            IsLastPage = (itemsToSkip + PageCapability) > MenuItems.Count();
            return MenuItems.Skip(itemsToSkip).Take(PageCapability).ToList();
        }

        public void SetSelectedMenuItem(bool goToSelectedItem = true)
        {
            if (SelectedItem != null)
                SelectedItem.SelectedColor = Colors.White;

            SelectedItem = CurrentMenuItems.FirstOrDefault(x => x.MenuLocation == Settings.MenuLocation);
            if (goToSelectedItem && SelectedItem == null)
            {
                var menuItemIndex = MenuItems.FindIndex(x => x.MenuLocation == Settings.MenuLocation) + 1;
                currentPage = (int)Math.Ceiling((double)menuItemIndex / PageCapability);
                CurrentMenuItems = GetMenuItems();
                SelectedItem = CurrentMenuItems.FirstOrDefault(x => x.MenuLocation == Settings.MenuLocation);
            }


            if (SelectedItem != null)
            {
                var greenColor = ResourceHelper.GetApplicationResource<Color>("GreenColor");
                SelectedItem.SelectedColor = greenColor;
            }
        }

        public void SetPageCapability()
        {
            var screenHeightInUnits = DeviceSettings.ScreenHeightInUnits;
            var appBarHeight = screenHeightInUnits * 0.127;
            var sideMenuHeight = screenHeightInUnits - appBarHeight - 5 - 120; // screenHeightInUnits - appBarHeight - padding - userButtonRow

            PageCapability = (int)Math.Floor(sideMenuHeight / 80); // 80 - menu item size

            if (IsArrowVisible)
                PageCapability--;
        }

        public void ReloadMenuItemTranslations()
        {
            foreach (var menuItem in MenuItems)
            {
                menuItem.SetTranslatedName();
            }
        }

        public void RegisterMenuItems()
        {
            MenuItems = new List<IMenuItem>
            {
                new MenuItem("SIDEBAR_TITLE_HOME", Enumerations.MenuLocation.Home, selectedImage: ResourceHelper.GetApplicationResource<string>("HomeIcon")),
            };

            if (CompanyFeatures.CompanyFeatSettings.ChecklistsEnabled)
            {
                MenuItems.Add(new MenuItem("SIDEBAR_TITLE_CHECKLIST", Enumerations.MenuLocation.Checklist, selectedImage: ResourceHelper.GetApplicationResource<string>("ChecklistIcon")));
            }
            if (CompanyFeatures.CompanyFeatSettings.TasksEnabled)
            {
                MenuItems.Add(new MenuItem("SIDEBAR_TITLE_TASKS", Enumerations.MenuLocation.Tasks, selectedImage: ResourceHelper.GetApplicationResource<string>("TaskIcon")));
            }

            if (CompanyFeatures.CompanyFeatSettings.AuditsEnabled)
            {
                MenuItems.Add(new MenuItem("SIDEBAR_TITLE_AUDIT", Enumerations.MenuLocation.Audits, selectedImage: ResourceHelper.GetApplicationResource<string>("AuditIcon")));
            }

            if (CompanyFeatures.CompanyFeatSettings.ActionsEnabled)
            {
                MenuItems.Add(new ActionMenuItem("SIDEBAR_TITLE_ACTIONS", Enumerations.MenuLocation.Actions, selectedImage: ResourceHelper.GetApplicationResource<string>("ActionIcon")));
            }

            if (CompanyFeatures.CompanyFeatSettings.ReportsEnabled)
            {
                MenuItems.Add(new MenuItem("SIDEBAR_TITLE_REPORT", Enumerations.MenuLocation.Report, selectedImage: ResourceHelper.GetApplicationResource<string>("ReportIcon")));
            }

            if (CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
            {
                var instructionMenuItem = new MenuItem("SIDEBAR_TITLE_INSTRUCTIONS", Enumerations.MenuLocation.Instructions, selectedImage: ResourceHelper.GetApplicationResource<string>("WorkInstructionIcon"));
                MenuItems.Add(instructionMenuItem);
            }

            if ((UserSettings.RoleType == Api.Models.Enumerations.RoleTypeEnum.Manager || UserSettings.RoleType == Api.Models.Enumerations.RoleTypeEnum.ShiftLeader) && CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
            {
                var assessmentMenuItem = new MenuItem("SIDEBAR_TITLE_ASSESSMENTS", Enumerations.MenuLocation.Assessments, selectedImage: ResourceHelper.GetApplicationResource<string>("AssessmentsIcon"));
                MenuItems.Add(assessmentMenuItem);
            }

            if (CompanyFeatures.CompanyFeatSettings.FactoryFeedEnabled)
            {
                var feedMenuItem = new MenuItem("SIDEBAR_TITLE_FEED", Enumerations.MenuLocation.Feed, selectedImage: ResourceHelper.GetApplicationResource<string>("FeedIcon"));
                MenuItems.Add(feedMenuItem);
            }

            currentPage = 1;
            SetPageCapability();
            CurrentMenuItems = GetMenuItems();
        }
    }
}
