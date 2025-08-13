using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Areas;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public class WorkAreaFilterControl : NotifyPropertyChanged, IWorkAreaFilterControl
    {
        public List<BasicWorkAreaModel> WorkAreas { get; private set; }

        public List<ITreeDropdownFilterItem> DropdownItems { get; private set; }

        public List<BasicWorkAreaModel> FlattenedWorkAreas { get; private set; }

        public BasicWorkAreaModel SelectedWorkArea { get; private set; }

        public Rect Rect { get; private set; } = new Rect(129, .2, .4, .6);

        private readonly IWorkAreaService _workAreaService;

        public WorkAreaFilterControl(IWorkAreaService workAreaService)
        {
            _workAreaService = workAreaService;
        }

        public async Task LoadWorkAreasAsync(int settingArea)
        {
            WorkAreas ??= await _workAreaService.GetBasicWorkAreasAsync();

            FlattenedWorkAreas ??= _workAreaService.GetFlattenedBasicWorkAreas(WorkAreas);

            if (WorkAreas.Count() > 6)
                Rect = new Rect(113, .8, .4, .9);
            else
                Rect = new Rect(113, .2, .4, .6);

            GetSelectedArea(settingArea);

            DropdownItems = new List<ITreeDropdownFilterItem>(WorkAreas);
        }

        private void GetSelectedArea(int settingArea)
        {
            settingArea = settingArea == 0 ? Settings.WorkAreaId : settingArea;
            SelectedWorkArea = FlattenedWorkAreas.FirstOrDefault(x => x.Id == settingArea);
            SelectedWorkArea ??= WorkAreas.FirstOrDefault();
        }

        public async Task DropdownTapAsync(object workArea, Func<Task> action, int settingArea)
        {
            if ((workArea as Syncfusion.TreeView.Engine.TreeViewNode).Content is BasicWorkAreaModel workAreaModel)
            {
                settingArea = SelectedWorkArea?.Id != 0 ? SelectedWorkArea.Id : Settings.WorkAreaId;
                if (workAreaModel.Id != settingArea)
                {
                    SelectedWorkArea = workAreaModel;
                    await action.Invoke();
                }
            }
        }
    }
}
