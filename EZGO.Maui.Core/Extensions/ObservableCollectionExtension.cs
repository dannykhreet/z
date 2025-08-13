using EZGO.Maui.Core.Models.ModelInterfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static void AddRange<T, A>(this ObservableCollection<T> comments, List<A> actionComments) where A : IBase<T>
        {
            for (int i = 0; i < actionComments.Count; i++)
            {
                comments.Add(actionComments[i].ToBasic());
            }
        }

        public static async Task AddRange<T>(this ObservableCollection<T> comments, List<T> actionComments) where T : class
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < actionComments.Count; i++)
                {
                    comments.Add(actionComments[i]);
                }
            });
        }
    }
}
