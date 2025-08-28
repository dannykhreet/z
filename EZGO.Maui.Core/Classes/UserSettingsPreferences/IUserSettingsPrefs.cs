using EZGO.Api.Models.Enumerations;

namespace EZGO.Maui.Core.Classes.UserSettingsPreferences
{
    public interface IUserSettingsPrefs
    {
        RoleTypeEnum RoleType { get; set; }
        int CompanyId { get; set; }
        int Id { get; set; }
        string Fullname { get; set; }
        string UserPictureUrl { get; set; }
        string CompanyLogoUrl { get; set; }
        string CompanyName { get; set; }
        string Role { get; set; }
    }
}
