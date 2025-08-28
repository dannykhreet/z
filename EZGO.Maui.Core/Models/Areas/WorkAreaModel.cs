using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace EZGO.Maui.Core.Models.Areas
{
    public class WorkAreaModel : EZGO.Api.Models.Area, IBase<BasicWorkAreaModel>
    {
        private string _picture;
        public new string Picture
        {
            get { return !string.IsNullOrEmpty(_picture) ? string.Format(Constants.MediaBaseUrl, _picture.Replace("/media", "")) : "placeholder.png"; }
            set { _picture = value; }
        }

        [JsonIgnore]
        public int WorkAreaHeight { get; set; } = 250;


        [JsonIgnore]
        public new List<WorkAreaModel> Children { get; set; }

        public new string Name { get; set; }

        public new string FullDisplayName { get; set; }

        private string _selectedName;
        [JsonIgnore]
        public string SelectedName
        {
            get => _selectedName ?? Name;
            set { _selectedName = value; }
        }
        [JsonIgnore]
        public bool AreaIsSelected { get; set; } = false;
        [JsonIgnore]
        public bool AreaIsExpanded { get; set; } = false;
        [JsonIgnore]
        public bool HasChildren { get { return Children?.Any() ?? false; } }

        public BasicWorkAreaModel ToBasic()
        {
            var result = new BasicWorkAreaModel
            {
                Id = this.Id,
                Name = this.Name,
                Picture = this.Picture,
                FullDisplayName = this.FullDisplayName
            };
            return result;
        }
    }
}
