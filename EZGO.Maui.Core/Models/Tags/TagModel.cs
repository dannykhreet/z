using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using EZGO.Api.Models.Tags;

namespace EZGO.Maui.Core.Models.Tags
{
    public class TagModel : Tag, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsActive { get; set; }
        public List<TagModel> SubTags { get; set; }
        public new bool IsSystemTag { get; set; }
        public new string IconName { get; set; }
        public new string IconStyle { get; set; }

        [JsonIgnore]
        public bool IsExpanded { get; set; }

        public Tag ToTag()
        {
            return new Tag()
            {
                ColorCode = ColorCode,
                GroupGuid = GroupGuid,
                GroupName = GroupName,
                Guid = Guid,
                IconName = IconName,
                IconStyle = IconStyle,
                Id = Id,
                Name = Name
            };
        }
    }
}
