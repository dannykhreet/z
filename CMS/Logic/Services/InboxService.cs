using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.Logic.Services
{
    public class InboxService : IInboxService
    {
        private readonly IApiConnector _connector;

        public InboxService(IApiConnector connector)
        {
            _connector = connector;
        }
        public async Task<int> GetSharedTemplatesCount()
        {
            var result = await _connector.GetCall(Constants.SharedTemplates.GetInboxCount);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (int.TryParse(result.Message, out int count))
                    return count;
            }
            return 0;
        }
    }
}
