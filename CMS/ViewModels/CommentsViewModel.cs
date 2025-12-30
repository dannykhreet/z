using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Models.Action;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class CommentsViewModel : BaseViewModel
    {
        public UserProfile CurrentUser { get; set; }

        public List<ActionCommentModel> Comments { get; set; }

        public List<UserProfile> Resources { get; set; }

        public List<ActionCommentModel> CommentsWithImages()
        {
            List<ActionCommentModel> result = new List<ActionCommentModel>();
            Comments.ForEach(x =>
            {
                var pic = Resources.FirstOrDefault(r => r.Id == x.UserId)?.Picture;
                x.UserPicture = !string.IsNullOrEmpty(pic) ? WebApp.Helpers.MediaHelpers.GetMediaImageUrl(this.ApplicationSettings, Resources.FirstOrDefault(r => r.Id == x.UserId)?.Picture) : "/assets/img/user-placeholder.jpg";
                result.Add(x);
            });

            return result;
        }
    }
}
