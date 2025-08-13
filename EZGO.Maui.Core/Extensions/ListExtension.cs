using EZGO.Maui.Core.Models.ModelInterfaces;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Extensions
{
    public static class ListExtension
    {
        public static List<TResult> ToBasicList<TResult, TBase>(this List<TBase> comments) where TBase : IBase<TResult>
        {
            List<TResult> basicComments = new List<TResult>();

            if (comments == null)
                return basicComments;

            foreach (var comment in comments)
            {
                basicComments.Add(comment.ToBasic());
            }

            return basicComments;
        }

        public static bool IsNullOrEmpty<TResult>(this List<TResult> results)
        {
            return results == null || !results.Any();
        }
    }
}
