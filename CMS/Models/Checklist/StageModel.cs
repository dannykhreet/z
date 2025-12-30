using EZGO.Api.Models;
using EZGO.Api.Models.Settings;

namespace WebApp.Models.Checklist
{
    public class StageModel : Stage
    {
        public ApplicationSettings ApplicationSettings { get; set; }
    }
}
