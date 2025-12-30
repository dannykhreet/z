using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Comment;

namespace WebApp.Models.Pdf
{
    public class PdfCommentModel : CommentModel
    {
        public int TaskIndex { get; set; }
    }
}
