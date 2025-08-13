using EZGO.Maui.Core;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Users;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Controls.Images;

public partial class UserImageList : SfListView
{
		private const int MAX_USER_COUNT = 4;

		public int MissingUsersCount { get; private set; }

		public static BindableProperty UserIdsProperty = BindableProperty.Create(
			nameof(UserIds),
			typeof(List<int>),
			typeof(UserImageList),
			propertyChanged: UserIdsProperty_Changed);
		private List<UserProfileModel> userProfiles;

		public List<int> UserIds
		{
				get => (List<int>)GetValue(UserIdsProperty);
				set => SetValue(UserIdsProperty, value);
		}

		private static async void UserIdsProperty_Changed(BindableObject bindable, object oldValue, object newValue)
		{
				var userIds = (List<int>)newValue;
				var control = bindable as UserImageList;
				if (control == null)
						return;

				control.IsVisible = false;

				if (userIds?.Count > 0)
				{
						var userModels = new List<UserProfileModel>();
						using var scope = App.Container.CreateScope();
						var userService = scope.ServiceProvider.GetService<IUserService>();
						var users = await userService.GetCompanyUsersAsync();
						foreach (var item in userIds)
						{
								var foundUser = users.FirstOrDefault(x => x.Id == item);
								if (foundUser != null)
										userModels.Add(foundUser);
						}

						control.MoreUsers = userModels.Count > MAX_USER_COUNT;
						control.MissingUsersCount = userModels.Count - MAX_USER_COUNT + 1;
						control.UserProfiles = userModels.Take(MAX_USER_COUNT).ToList();
						control.WidthRequest = control.UserProfiles.Count * 40;
						control.IsVisible = control.userProfiles.Count > 0;

						if (control.MoreUsers)
						{
								control.UserProfiles[3].Id = control.MissingUsersCount;
								control.UserProfiles[3].SuccessorId = -1;
						}
				}
		}

		public List<UserProfileModel> UserProfiles { get => userProfiles; set { userProfiles = value; OnPropertyChanged(); } }

		public bool MoreUsers { get; set; }

		public UserImageList()
		{
				InitializeComponent();
		}
}