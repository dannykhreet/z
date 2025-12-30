using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.General
{
    /// <summary>
    /// Announcement; Announcements for use with announce panels in the clients (CMS)
    /// </summary>
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public List<string> Media { get; set; }
        public AnnouncementTypeEnum AnnouncementType { get; set; }

    }
}
