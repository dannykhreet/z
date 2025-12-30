using EZGO.Api.Models;

namespace WebApp.Models.Checklist
{
    public class StageTemplateModel : StageTemplate
    {
        public bool isNew { get; set; } //capitalization is against convetion to be consistent with current models, we should refactor this.
    }
}
