using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Action
{
    public class ActionCommentModel
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        [Required]
        public string Comment { get; set; }
        public List<string> Images { get; set; }
        public string Video { get; set; }
        public string VideoThumbnail { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? UserModifiedUtcAt { get; set; }
        public int UserId { get; set; }
        public string UserPicture { get; set; }
    }
}
