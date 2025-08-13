using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Tasks;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Local
{
    public class LocalTaskTemplateModel
    {
        public int Id { get; set; }
        public TaskStatusEnum? Status { get; set; }
        public int? Score { get; set; }
        public bool HasPictureProof { get; set; }


        /// <summary>
        /// Holds the user values for properties in this task templates
        /// </summary>
        public List<PropertyUserValue> PropertyUserValues { get; set; }

        public List<MediaItem> PictureProofMediaItems { get; set; }

        public List<CommentModel> Comments { get; set; }

        public ScoreModel NewScore { get; set; }

        public SignatureModel Signature { get; set; }

        public static LocalTaskTemplateModel FromBasic(BasicTaskTemplateModel another)
        {
            return new LocalTaskTemplateModel()
            {
                Id = another.Id,
                Status = another.FilterStatus,
                Score = another.Score,
                PropertyUserValues = another.PropertyValues,
                Comments = another.LocalComments,
                PictureProofMediaItems = another.PictureProofMediaItems,
                HasPictureProof = another.HasPictureProof,
                Signature = another.Signature
            };
        }

    }
}
