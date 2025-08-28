using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Actions
{
    public class FilterModel : ITreeDropdownFilterItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ITreeDropdownFilterItem> Children { get; set; }

        public FilterModel(string name)
        {
            Name = name;
        }

        public FilterModel(string name, int id) : this(name)
        {
            Id = id;
        }
    }
}
