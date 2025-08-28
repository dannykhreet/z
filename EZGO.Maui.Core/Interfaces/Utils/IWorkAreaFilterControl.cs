using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Maui.Core.Models.Areas;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IWorkAreaFilterControl
    {
        List<BasicWorkAreaModel> WorkAreas { get; }
        List<ITreeDropdownFilterItem> DropdownItems { get; }
        List<BasicWorkAreaModel> FlattenedWorkAreas { get; }
        BasicWorkAreaModel SelectedWorkArea { get; }
        Rect Rect { get; }

        Task LoadWorkAreasAsync(int settingArea);
        Task DropdownTapAsync(object workArea, Func<Task> action, int settingArea);
    }
}
