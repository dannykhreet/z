using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Models.Areas
{
    public class BasicWorkAreaModel : NotifyPropertyChanged, ITreeDropdownFilterItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FullDisplayName { get; set; }

        public string Picture { get; set; }

        public List<ITreeDropdownFilterItem> Children { get; set; }

        public BasicWorkAreaModel Parent { get; set; }

        public bool IsSelected { get; set; }

        public bool HasChildren => Children?.Any() ?? false;

        public bool IsRootExpanded { get; set; }
    }
}
