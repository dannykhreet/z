using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    public enum AnnouncementTypeEnum
    {
        All = 0, //all release notes
        ReleaseNotes = 1, //mix of CMS and API release notes based on version number of CMS
        CMSReleaseNotes = 2, //CMS only
        WebClientReleaseNotes = 3, //WebClient only
        XamarinReleaseNotes = 4, //Xamarin release notes (all platforms)
        XamarinIosReleaseNotes = 5, //Xamarin IOS only release notes
        XamarinAndroidReleaseNotes = 6, //Xamarin IOS only release notes
        DashboardReleaseNotes = 7,  //Dashboard release notes.
        PlatformMessage = 999 
    }
}
