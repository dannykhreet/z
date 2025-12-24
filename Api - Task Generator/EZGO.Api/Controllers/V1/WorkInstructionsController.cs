using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// ShiftsController; contains all routes based on shifts.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.WorkInstructions)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class WorkInstructionsController : BaseController<WorkInstructionsController>
    {
        #region - privates -
        private readonly IWorkInstructionManager _manager;
        private readonly IAreaManager _areaManager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - constructor(s) -
        public WorkInstructionsController(IWorkInstructionManager manager, IConfigurationHelper configurationHelper, IToolsManager toolsManager, IAreaManager areaManager, ILogger<WorkInstructionsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _areaManager = areaManager;
            _manager = manager;
            _toolsManager = toolsManager;
        }
        #endregion

        [Route("workinstructions")]
        [HttpGet]
        public async Task<IActionResult> GetWorkInstructions([FromQuery] RoleTypeEnum? role, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] bool? includeavailableforallareas = null )
        {
            _manager.Culture = TranslationLanguage;

            var filters = new WorkInstructionFilters() {
                AreaId = areaid,
                RoleType = role,
                InstructionType = InstructionTypeEnum.BasicInstruction,
                FilterAreaType = filterareatype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_WORKINSTRUCTIONTEMPLATES_RETURN_ITEMS,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                IncludeAvailableForAllAreas = includeavailableforallareas
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetWorkInstructionTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("workinstructions/availableforarea/{areaid}")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableWorkInstructions([FromQuery] RoleTypeEnum? role, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            //normal GetWorkInstructionTemplates but with mandatory area id and set IncludeAvailableForAllAreas to true
            return await GetWorkInstructions(role: role, areaid: areaid, filterareatype: filterareatype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly, includeavailableforallareas: true);
        }

        [Route("workinstruction/{workinstructionid}")]
        [HttpGet]
        public async Task<IActionResult> GetWorkInstruction([FromRoute] int workinstructionid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!WorkInstructionValidators.WorkInstructionIdIsValid(workinstructionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workinstructionid, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            //check basic user area and wi area
            if (!await this.CurrentApplicationUser.CheckAreaRightsForWorkinstruction(workInstructionId: workinstructionid))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), workInstructionTemplateId: workinstructionid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Probably not needed
        /// </summary>
        /// <param name="workInstruction"></param>
        /// <returns></returns>
        [Route("workinstruction/add")]
        [HttpPost]
        public async Task<IActionResult> AddWorkInstruction([FromBody] WorkInstruction workInstruction)
        {
            var output = (await GetMockWorkInstructions(true)).FirstOrDefault();
            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        /// <summary>
        /// Probably not needed
        /// </summary>
        /// <param name="workinstructionid"></param>
        /// <param name="workInstruction"></param>
        /// <returns></returns>
        [Route("workinstruction/change/{workinstructionid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeWorkInstruction([FromBody] WorkInstruction workInstruction, [FromRoute] int workinstructionid)
        {
            var output = (await GetMockWorkInstructions(true)).FirstOrDefault();
            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Route("workinstruction/delete/{workinstructionid}")]
        [HttpPost]
        public async Task<IActionResult> DeleteWorkInstruction([FromRoute] int workinstruction)
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, ("workinstruction/delete/{workinstructionid}").ToJsonFromObject());
        }


        [Route("workinstruction/setviewed/{workinstructionid}")]
        [HttpPost]
        public async Task<IActionResult> SetWorkInstructionViewed([FromRoute] int workinstruction)
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (true).ToJsonFromObject());
        }


        /// <summary>
        /// GetAssessmentsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("workinstructions/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetAssessHealth()
        {
            try
            {
                var result = await _manager.GetWorkInstructionTemplatesAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY),
                                                                       userId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_USER_ID_CONFIG_KEY),
                                                                       filters: new WorkInstructionFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

                if (result.Any() && result.Count > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());
                }
            }
            catch
            {

            }
            return StatusCode((int)HttpStatusCode.Conflict, false.ToJsonFromObject());
        }




        private async Task<List<WorkInstruction>> GetMockWorkInstructions(bool generateValues = false)
        {
            string[] images = new[] {"tasks/30852/799be628-c6cb-4f53-b815-60e18547a4c5.png",
                                     "tasks/30916/a5b2c203-daba-45c7-afb4-e3723fbb0370.jpg",
                                     "tasks/30930/cf9a78c9-0e32-4d0c-8a17-35e427a29bac.jpg",
                                     "tasks/30997/443a5969-e125-4a43-90b6-1a817a637f82.jpg",
                                     "tasks/30921/96c00fb4-bd43-47bb-b6dd-9b4967021fae.png",
                                     "tasks/31132/3c22df27-b055-4d95-b0c2-f4251bfb8e87.jpg",
                                     "tasks/31002/4dc6591f-b14d-49f7-9116-d7236698d1da.png",
                                     "tasks/30926/40ed00e8-5c14-4d77-a3a5-dfec79b71b68.png",
                                     "tasks/31005/2201a974-0733-4d37-929e-b489ceed401f.jpg",
                                     "tasks/30924/5c8b131d-afcc-45ed-a579-ee58c81ae460.jpg",
                                     "tasks/30940/d4ac4b1f-ebf1-4db4-9159-4d09eb7c0310.jpg",
                                     "tasks/30995/7de561ad-9f71-42a1-8b88-4ce10df245ff.png"};

            string[] images_w = new[] {  "lists/1703/00739107-c5ef-41b0-8d65-664f7655c288.jpg",
                                         "lists/1701/7d794da8-e58d-46e6-84c0-ff7ff885a96b.jpg",
                                         "lists/1690/1ad06a7f-d0b9-45ee-a949-6e59c0672014.jpg",
                                         "lists/1669/f6d7fdbe-b916-4f15-a6dc-b42835c0ce50.jpg",
                                         "lists/1623/728c16d5-303e-4813-9531-4b5cefd563a2.jpg"
                                         };

            string[] workInstructionNames = new[] { "Etikettenrol Wissel", "Inspectie & schoonmaak", "PCS - Dag Controle Lijst", "PCS - Shift Control Lijst", "Procesconfirmatie: A) Ochtenddienst", "Procesconfirmatie: B) Middagdienst", "Procesconfirmatie: C) Nachtdienst", "PCS - Dag Controle Lijst", "Werkplek opleiding -Etikettenrol Wissel", "Werkplek opleiding - Inspectie & schoonmaak", "LOTOTO Procedure" };


            int[] scores = new[]
            {
                1,3,5,1,5,1,5,3,4,5,1,2,3,4,5,2,3,2,2,1,5,2,3,4,5,2,1,2,4,5,3,1,5,1,3,1,3,5,1,5,1,5,3,4,5,1,2,3,4,5,2,3,2,2,1,5,2,3,4,5,2,1,2,4,5,3,1,5,1,3,1,3,5,1,5,1,5,3,4,5,1,2,3,4,5,2,3,2,2,1,5,2,3,4,5,2,1,2,4,5,3,1,5,1,3
            };

            var output = new List<WorkInstruction>();

            for (var ii = 1; ii < 11; ii++)
            {
                var w = new WorkInstruction();
                w.AreaId = 2877;
                w.CompanyId = 136;
                w.CreatedAt = DateTime.Now;
                w.Description = string.Concat("This is a description of a work Instructions ", ii);
                w.Id = ii;
                //w.MaxScore = 5;
                //w.MinScore = 1;
                w.ModifiedAt = DateTime.Now;
                w.Name = workInstructionNames[ii];//string.Concat("Work Instruction Name ", ii);
                w.Picture = images[ii]; //"136/lists/0/d01372d3-bc9c-4813-bc89-865b44afba1c.png";
                w.Role = RoleTypeEnum.Basic;
                //w.ScoreType = ScoreTypeEnum.Score;
                w.WorkInstructionType = InstructionTypeEnum.SkillInstruction;

                if (generateValues)
                {
                    w.InstructionItems = new List<InstructionItem>();
                    for (var iii = 1; iii < 11; iii++)
                    {
                        var wi = new InstructionItem();
                        wi.CompanyId = 136;
                        wi.Description = string.Concat("This is a description of a work instructions item ", iii);
                        wi.Id = iii;
                        wi.Name = string.Concat(workInstructionNames[ii], " ", iii);
                        wi.Picture = images[iii];//"136/lists/0/d01372d3-bc9c-4813-bc89-865b44afba1c.png";
                        wi.Score = scores[iii + ii];

                        w.InstructionItems.Add(wi);
                    }

                    w.TotalScore = (int)(w.InstructionItems.Select(x => x.Score).Sum() / w.InstructionItems.Count);
                    w.NumberOfInstructionItems = w.InstructionItems.Count;
                } else
                {
                    w.TotalScore = scores[ii];
                    w.NumberOfInstructionItems = 10;
                }

                output.Add(w);
            };

            output = await AppendAreaPathsAsync(objects: output);

            await Task.CompletedTask;

            return output;
        }

        private async Task<List<WorkInstruction>> AppendAreaPathsAsync(List<WorkInstruction> objects)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: 136, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var item in objects)
                {
                    var area = areas?.Where(x => x.Id == item.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        item.AreaPath = area?.FullDisplayName;
                        item.AreaPathIds = area?.FullDisplayIds;
                    }

                }
            }
            return objects;
        }
    }
}
