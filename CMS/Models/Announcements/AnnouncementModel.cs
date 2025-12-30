using EZGO.Api.Models.Enumerations;
using System;

namespace WebApp.Models.Announcements
{
    public class AnnouncementModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public AnnouncementTypeEnum AnnouncementType { get; set; }
    }
}