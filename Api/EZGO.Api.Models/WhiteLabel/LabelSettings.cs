using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WhiteLabel
{
    /// <summary>
    /// LabelSettings; Label settings used for white labeling the apps. 
    /// </summary>
    public class LabelSettings
    {
        /// <summary>
        /// Icon; Icon for app
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Logo; For app
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// PrimaryColor; 1st color used
        /// </summary>
        public string PrimaryColor { get; set; }
        /// <summary>
        /// SecondaryColor; 2nd color used
        /// </summary>
        public string SecondaryColor { get; set; }
        /// <summary>
        /// TertiaryColor; 3rd color used;
        /// </summary>
        public string TertiaryColor { get; set; }
        /// <summary>
        /// DisplayTitle; Display Title;
        /// </summary>
        public string DisplayTitle { get; set; }
        /// <summary>
        /// DisplayEntityName; Display Entity Name
        /// </summary>
        public string DisplayEntityName { get; set; }
        /// <summary>
        /// ImageCarrousel; Possible image carousel for display
        /// </summary>
        public List<string> ImageCarrousel { get; set; }
        /// <summary>
        /// BackgroundImage; Background image. 
        /// </summary>
        public string BackgroundImage { get; set; }
    }
}
