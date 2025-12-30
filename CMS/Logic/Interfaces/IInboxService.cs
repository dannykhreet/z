using System.Threading.Tasks;

namespace WebApp.Logic.Interfaces
{
    public interface IInboxService
    {
        Task<int> GetSharedTemplatesCount();
    }
}
