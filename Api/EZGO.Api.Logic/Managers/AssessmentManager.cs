using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Mappers;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class AssessmentManager : BaseManager<AssessmentManager>, IAssessmentManager
    {
        #region - properties -
        private string culture;
        public string Culture
        {
            get { return culture; }
            set { culture = _tagManager.Culture = value; }
        }
        private static readonly StringSplitOptions IncOpts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        private static HashSet<string> ParseIncludes(string include) => new HashSet<string>((include ?? string.Empty).ToLowerInvariant().Split(',', IncOpts));

        #endregion

        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IAreaManager _areaManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ITagManager _tagManager;
        private readonly IGeneralManager _generalManager;
        private readonly IFlattenAssessmentManager _flattenedAssessmentManager;
        private readonly IUserManager _userManager;
        private readonly IUserStandingManager _userStandingManager;
        #endregion

        #region - constructor(s) -
        public AssessmentManager(IUserStandingManager userStandingManager, IGeneralManager generalManager, IFlattenAssessmentManager flattenedAssessmentManager, IDatabaseAccessHelper manager, IUserManager userManager, ITagManager tagManager, IConfigurationHelper configurationHelper, IAreaManager areaManager, IDataAuditing dataAuditing, ILogger<AssessmentManager> logger) : base(logger)
        {
            _manager = manager;
            _areaManager = areaManager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;
            _tagManager = tagManager;
            _generalManager = generalManager;
            _flattenedAssessmentManager = flattenedAssessmentManager;
            _userManager = userManager;
            _userStandingManager = userStandingManager;
        }
        #endregion

        #region - public assessment -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="assessmentId"></param>
        /// <param name="include"></param>
        /// <param name="connectionKind"></param>
        /// <param name="useStatic"></param>
        /// <returns></returns>
        public async Task<Assessment> GetAssessmentAsync(int companyId, int assessmentId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false)
        {
            var assessment = new Assessment();
            bool useStaticStorage = useStatic;

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", assessmentId));

                using (dr = await _manager.GetDataReader(useStaticStorage ? "get_assessment_static" : "get_assessment", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        if (useStaticStorage)
                        {
                            assessment = CreateOrFillStaticAssessmentFromReader(dr, assessment: assessment);
                        }
                        else
                        {
                            assessment = CreateOrFillAssessmentFromReader(dr, assessment: assessment, include: include);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetAssessmentAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (assessment?.Id > 0)
            {
                if (!useStaticStorage)
                {
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) assessment.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, id: assessment.Id, objectType: ObjectTypeEnum.Assessment, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower())) assessment.SkillInstructions = await GetSkillInstructionsWithAssessmentAsync(companyId: companyId, assessment.Id, include: include, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) assessment = await AppendAreaPathsToAssessmentAsync(companyId: companyId, assessment: assessment, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));

                    if (!string.IsNullOrEmpty(assessment.Version) && assessment.Version != await _flattenedAssessmentManager.RetrieveLatestAvailableVersion(assessment.TemplateId, companyId) && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
                    {
                        assessment = await ApplyTemplateVersionToAssessment(assessment, companyId, include);
                    }
                }

                return assessment;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="assessment"></param>
        /// <returns></returns>
        public async Task<int> AddAssessmentAsync(int companyId, int userId, Assessment assessment)
        {
            List<NpgsqlParameter> addParameters = new List<NpgsqlParameter>();

            //make sure there is a maximum of only 1 assessment open for the selected participant / template
            var assessments = await GetAssessmentsAsync(companyId: companyId, userId: null,
                                                        filters: new AssessmentFilters() { CompletedForId = assessment.CompletedForId, TemplateId = assessment.TemplateId, IsCompleted = false },
                                                        include: null, useStatic: false);

            if (assessments.Count > 0)
            {
                return -1;
            }

            addParameters.AddRange(GetNpgsqlParametersFromAssessment(assessment: assessment, companyId: companyId, userId: userId));

            //if assessment lacks a version, and fallback is enabled, use latest version of template.
            if (assessment.TemplateId > 0 && string.IsNullOrEmpty(assessment.Version) && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA_FALLBACK"))
            {
                assessment.Version = await _flattenedAssessmentManager.RetrieveLatestAvailableVersion(assessment.TemplateId, companyId);
            }

            if (!string.IsNullOrEmpty(assessment.Version))
            {
                addParameters.Add(new NpgsqlParameter("@_version", assessment.Version));
            }

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_assessment", parameters: addParameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                assessment.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, ObjectTypeEnum.AssessmentTemplate, id: assessment.TemplateId);
                await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.Assessment, id: possibleId, tags: assessment.Tags, companyId: companyId, userId: userId);

                if (assessment.SkillInstructions != null && assessment.SkillInstructions.Count > 0)
                {
                    foreach (var skillinstruction in assessment.SkillInstructions)
                    {
                        skillinstruction.AssessmentTemplateId = assessment.TemplateId;
                        skillinstruction.AssessmentId = possibleId;
                    }
                    var rowsEffected = await ChangeAssessmentAddOrChangeSkillInstructionsAsync(companyId: companyId, userId: userId, assessmentId: possibleId, assessment.SkillInstructions);
                }

                decimal score = await SetAssessmentCalculatedScoreAsync(companyId: companyId, userId: userId, assessmentId: possibleId);

                if (assessment.IsCompleted && assessment.CompletedForId.HasValue)
                {
                    int updatedUserValuesRowsCount = await _userStandingManager.UpdateUserSkillValuesWithAssessmentAsync(companyId: companyId, userId: userId, assessmentId: possibleId, assessmentCompletedForId: assessment.CompletedForId.Value);
                }
            }

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.assessments.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added assessment.");

            }

            return possibleId;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="assessmentId"></param>
        /// <param name="assessment"></param>
        /// <returns></returns>
        public async Task<bool> ChangeAssessmentAsync(int companyId, int userId, int assessmentId, Assessment assessment)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), assessmentId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessment(assessment: assessment, companyId: companyId, assessmentId: assessmentId, userId: userId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_assessment", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (assessmentId > 0 && assessment.SkillInstructions != null)
            {
                var rowsEffected = await ChangeAssessmentAddOrChangeSkillInstructionsAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, assessment.SkillInstructions);
            }

            decimal score = await SetAssessmentCalculatedScoreAsync(companyId: companyId, userId: userId, assessmentId: assessmentId);
            if (assessment.IsCompleted && assessment.CompletedForId.HasValue)
            {
                int updatedUserValuesRowsCount = await _userStandingManager.UpdateUserSkillValuesWithAssessmentAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, assessmentCompletedForId: assessment.CompletedForId.Value);
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), assessmentId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessments.ToString(), objectId: assessmentId, userId: userId, companyId: companyId, description: "Changed assessment.");

            }
            return rowseffected > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="assessmentId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public async Task<bool> SetAssessmentActiveAsync(int companyId, int userId, int assessmentId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), assessmentId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", assessmentId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_assessment_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), assessmentId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessments.ToString(), objectId: assessmentId, userId: userId, companyId: companyId, description: "Changed assessment active state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="assessmentId"></param>
        /// <returns></returns>
        public async Task<bool> FreeLinkedAssessmentInstruction(int companyId, int assessmentId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_templateid", assessmentId));
            var dataeffected = Convert.ToBoolean(await _manager.ExecuteScalarAsync("remove_linked_assessment", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return dataeffected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="assessmentId"></param>
        /// <param name="isCompleted"></param>
        /// <returns></returns>
        //TODO Implement
        public async Task<bool> SetAssessmentCompletedAsync(int companyId, int userId, int assessmentId, bool isCompleted = true)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
        #endregion

        #region - public assessments -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="filters"></param>
        /// <param name="include"></param>
        /// <param name="useStatic"></param>
        /// <returns></returns>
        public async Task<List<Assessment>> GetAssessmentsAsync(int companyId, int? userId = null, AssessmentFilters? filters = null, string include = null, bool useStatic = false)
        {
            //NOTE! get_assessments_static NOT IMPLEMENTED

            var output = new List<Assessment>();
            bool useStaticStorage = useStatic;

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.StartTimestamp.HasValue && filters.Value.StartTimestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_starttimestamp", filters.Value.StartTimestamp.Value));
                    }

                    if (filters.Value.EndTimestamp.HasValue && filters.Value.EndTimestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_endtimestamp", filters.Value.EndTimestamp.Value));
                    }

                    if (filters.Value.IsCompleted.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_iscompleted", filters.Value.IsCompleted.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }

                    if (filters.Value.CompletedForId.HasValue && filters.Value.CompletedForId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_completedforid", filters.Value.CompletedForId.Value));
                    }

                    if (filters.Value.AssessorId.HasValue && filters.Value.AssessorId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assessorid", filters.Value.AssessorId.Value));
                    }

                    if (filters.Value.TemplateId.HasValue && filters.Value.TemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_templateid", filters.Value.TemplateId.Value));
                    }

                    if (filters.Value.TimespanType.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timespanindays", (int)filters.Value.TimespanType.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (filters.Value.AssessorIds != null && filters.Value.AssessorIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assessorids", filters.Value.AssessorIds));
                    }

                    if (filters.Value.SortByModifiedAt != null && filters.Value.SortByModifiedAt.Value)
                    {
                        parameters.Add(new NpgsqlParameter(@"_sortbymodifiedat", filters.Value.SortByModifiedAt.Value));
                    }

                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }
                }

                using (dr = await _manager.GetDataReader(useStaticStorage ? "get_assessments_static" : "get_assessments", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var audit = useStaticStorage ? CreateOrFillStaticAssessmentFromReader(dr) : CreateOrFillAssessmentFromReader(dr, include: include);
                        if (audit != null && audit.Id > 0)
                        {
                            output.Add(audit);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetAssessmentsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Count > 0)
            {
                if (!useStaticStorage)
                {
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToAssessmentsAsync(assessments: output, companyId: companyId);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower())) output = await AppendSkillInstructionsToAssessmentsAsync(companyId: companyId, assessments: output, filters: filters, userId: userId, include: include);

                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
                    {
                        output = await ApplyTemplateVersionToAssessments(output, companyId, include);
                    }
                }
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToAssessmentsAsync(companyId: companyId, assessments: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
            }

            return output;
        }
        #endregion

        #region - private assessment (retrieval methods) -
        /// <summary>
        /// AppendAreaPathsToAssessmentsAsync; Add the AreaPath to the Assessment. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="assessment">Assessment.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>Assessments including area full path. </returns>
        private async Task<Assessment> AppendAreaPathsToAssessmentAsync(int companyId, Assessment assessment, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {

                var area = areas?.Where(x => x.Id == assessment.AreaId)?.FirstOrDefault();
                if (area != null)
                {
                    if (addAreaPath) assessment.AreaPath = area.FullDisplayName;
                    if (addAreaPathIds) assessment.AreaPathIds = area.FullDisplayIds;
                }

            }
            return assessment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="assessmentId"></param>
        /// <param name="include"></param>
        /// <param name="connectionKind"></param>
        /// <returns></returns>
        private async Task<List<AssessmentSkillInstruction>> GetSkillInstructionsWithAssessmentAsync(int companyId, int assessmentId, string include, ConnectionKind connectionKind)
        {
            var output = new List<AssessmentSkillInstruction>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_assessmentid", assessmentId));


                using (dr = await _manager.GetDataReader("get_assessment_skillinstructions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var skillInstruction = CreateOrFillSkillInstructionFromReader(dr, include: include);
                        output.Add(skillInstruction);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetSkillInstructionsWithAssessmentAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToSkillInstructionsAsync(companyId: companyId, assessmentId: assessmentId, assessmentSkillInstructions: output);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionItems.ToString().ToLower()))) output = await AppendInstructionItemsWithSkillInstructionsAsync(companyId: companyId, assessmentId: assessmentId, skillInstructions: output, include: include, connectionKind: connectionKind);
            }

            return output;

        }

        private async Task<List<AssessmentSkillInstruction>> AppendInstructionItemsWithSkillInstructionsAsync(int companyId, int assessmentId, string include, List<AssessmentSkillInstruction> skillInstructions, ConnectionKind connectionKind)
        {
            var instructionItems = new List<InstructionItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_assessmentid", assessmentId));


                using (dr = await _manager.GetDataReader("get_assessment_skillinstruction_items", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var instructionItem = await CreateOrFillInstructionItemFromReader(dr, include: include);
                        instructionItems.Add(instructionItem);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.AppendInstructionItemsWithSkillInstructionsAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (instructionItems.Count > 0)
            {
                foreach (var item in skillInstructions)
                {
                    item.InstructionItems = instructionItems.Where(x => x.CompanyId == companyId && x.AssessmentSkillInstructionId == item.Id).OrderBy(y => y.Index).ToList();
                }
            }

            return skillInstructions;
        }

        private async Task<decimal> SetAssessmentCalculatedScoreAsync(int companyId, int userId, int assessmentId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), assessmentId);

            List<NpgsqlParameter> parameters = [
                new("@_companyid", companyId),
                new("@_userid", userId),
                new("@_assessmentid", assessmentId)
            ];

            var score = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_assessment_calculated_score", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessments.ToString(), assessmentId);
            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessments.ToString(), objectId: assessmentId, userId: 0, companyId: companyId, description: "Changed assessment score.");

            return score;
        }
        #endregion

        #region - private assessments (retrieval methods) -
        /// <summary>
        /// AppendAreaPathsToAssessmentsAsync; Add the AreaPath to the Assessment. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="assessments">List of assessments.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>Assessments including area full path. </returns>
        private async Task<List<Assessment>> AppendAreaPathsToAssessmentsAsync(int companyId, List<Assessment> assessments, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var assessment in assessments)
                {
                    var area = areas?.Where(x => x.Id == assessment.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        if (addAreaPath) assessment.AreaPath = area.FullDisplayName;
                        if (addAreaPathIds) assessment.AreaPathIds = area.FullDisplayIds;
                    }
                }
            }
            return assessments;
        }

        private async Task<List<Assessment>> AppendSkillInstructionsToAssessmentsAsync(int companyId, List<Assessment> assessments, AssessmentFilters? filters, int? userId, string include)
        {
            var skillInstructions = new List<AssessmentSkillInstruction>();

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                var assessmentIds = new List<int>();
                if (assessments != null && assessments.Count > 0)
                {
                    assessmentIds = assessments.Select(s => s.Id).ToList();
                }

                if (assessmentIds.Count > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_assessmentids", assessmentIds));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_assessmentids", DBNull.Value));
                }

                using (dr = await _manager.GetDataReader("get_assessment_skillinstructions_v2", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var skillInstruction = CreateOrFillSkillInstructionFromReader(dr, include: include);
                        skillInstructions.Add(skillInstruction);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetAssessmentsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (skillInstructions.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) skillInstructions = await AppendTagsToSkillInstructionsAsync(companyId: companyId, assessmentSkillInstructions: skillInstructions);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.InstructionItems.ToString().ToLower())) skillInstructions = await AppendSkillInstructionItemsToSkillInstructionsAsync(companyId: companyId, skillInstructions: skillInstructions, filters: filters, include: include, userId: userId);
            }

            if (assessments.Count > 0)
            {
                foreach (var assessment in assessments)
                {
                    assessment.SkillInstructions = skillInstructions.Where(x => x.CompanyId == companyId && x.AssessmentId == assessment.Id).OrderBy(y => y.Index).ToList();
                }
            }

            return assessments;
        }

        #endregion

        #region - private skill instructions (retrieval methods) -
        private async Task<List<AssessmentSkillInstruction>> AppendSkillInstructionItemsToSkillInstructionsAsync(int companyId, List<AssessmentSkillInstruction> skillInstructions, AssessmentFilters? filters, int? userId, string include)
        {
            var instructionItems = new List<InstructionItem>();

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                var skillInstructionIds = new List<int>();
                if (skillInstructions != null && skillInstructions.Count > 0)
                {
                    skillInstructionIds = skillInstructions.Select(s => s.Id).ToList();
                }

                if (skillInstructionIds.Count > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_assessmentinstructionids", skillInstructionIds));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_assessmentinstructionids", DBNull.Value));
                }

                using (dr = await _manager.GetDataReader("get_assessment_skillinstruction_items_v2", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var instructionItem = await CreateOrFillInstructionItemFromReader(dr, include: include);
                        instructionItems.Add(instructionItem);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.AppendSkillInstructionItemsToSkillInstructionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            if (instructionItems.Count > 0)
            {
                foreach (var skillInstruction in skillInstructions)
                {
                    skillInstruction.InstructionItems = instructionItems.Where(x => x.CompanyId == companyId && x.AssessmentSkillInstructionId == skillInstruction.Id && x.AssessmentId == skillInstruction.AssessmentId).OrderBy(y => y.Index).ToList();
                }
            }

            return skillInstructions;
        }
        #endregion

        #region - private assessment mutation methods -
        private async Task<int> ChangeAssessmentAddOrChangeSkillInstructionsAsync(int companyId, int userId, int assessmentId, List<AssessmentSkillInstruction> skillInstructions)
        {
            //TODO fill
            if (assessmentId > 0 && skillInstructions != null)
            {
                foreach (var skillInstruction in skillInstructions)
                {
                    if (!skillInstruction.TotalScore.HasValue) { skillInstruction.TotalScore = 0; }
                    //Add item to db
                    var instructionOutcome = await ChangeAssessmentAddOrChangeSkillInstructionAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, skillInstruction: skillInstruction);
                    var skillInstructionId = skillInstruction.Id > 0 ? skillInstruction.Id : instructionOutcome;

                    if (skillInstruction.InstructionItems != null)
                    {
                        foreach (var item in skillInstruction.InstructionItems)
                        {
                            if (!item.AssessmentTemplateId.HasValue || item.AssessmentTemplateId.Value <= 0) { item.AssessmentTemplateId = skillInstruction.AssessmentTemplateId; }
                            if (!item.WorkInstructionTemplateId.HasValue || item.WorkInstructionTemplateId.Value <= 0) { item.WorkInstructionTemplateId = skillInstruction.WorkInstructionTemplateId; }
                            if (!item.Score.HasValue) { item.Score = 0; }
                            var rowsEffected = await ChangAssessmentAddOrChangeSkillInstructionItemAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, assessmentInstructionId: skillInstructionId, instructionItem: item);
                        }
                    }
                }

            }

            await Task.CompletedTask;
            return -1;

        }

        private async Task<int> ChangAssessmentAddOrChangeSkillInstructionItemAsync(int companyId, int userId, int assessmentId, int assessmentInstructionId, InstructionItem instructionItem)
        {
            int output = -1;
            if (instructionItem.Id > 0)
            {
                output = await ChangeSkillInstructionItemAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, assessmentInstructionId: assessmentInstructionId, instructionItem: instructionItem);
            }
            else
            {
                if (!instructionItem.AssessmentSkillInstructionId.HasValue || instructionItem.AssessmentSkillInstructionId <= 0) { instructionItem.AssessmentSkillInstructionId = assessmentInstructionId; }
                if (!instructionItem.AssessmentId.HasValue || instructionItem.AssessmentId <= 0) { instructionItem.AssessmentId = assessmentId; }
                output = await AddSkillInstructionItemAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, assessmentInstructionId: assessmentInstructionId, instructionItem: instructionItem);
            }
            return output;
        }

        private async Task<int> AddSkillInstructionItemAsync(int companyId, int userId, int assessmentId, int assessmentInstructionId, InstructionItem instructionItem)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessmentInstructionItem(assessmentInstructionItem: instructionItem, userId: userId, companyId: companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_assessment_skillinstruction_item", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_skillinstruction_items.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.assessment_skillinstruction_items.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added assessment skill instruction item.");

            }

            return possibleId;
        }

        private async Task<int> ChangeSkillInstructionItemAsync(int companyId, int userId, int assessmentId, int assessmentInstructionId, InstructionItem instructionItem)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_skillinstruction_items.ToString(), instructionItem.Id);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessmentInstructionItem(assessmentInstructionItem: instructionItem, userId: userId, companyId: companyId, assessmentItemId: instructionItem.Id));

            var rowsEffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_assessment_skillinstruction_item", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowsEffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_skillinstruction_items.ToString(), instructionItem.Id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessment_skillinstruction_items.ToString(), objectId: instructionItem.Id, userId: userId, companyId: companyId, description: "Changed assessment skill instruction item.");

            }

            return rowsEffected;
        }


        private async Task<int> ChangeAssessmentAddOrChangeSkillInstructionAsync(int companyId, int userId, int assessmentId, AssessmentSkillInstruction skillInstruction)
        {
            int output = -1;
            if (skillInstruction.Id > 0)
            {
                output = await ChangeSkillInstructionAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, skillInstruction: skillInstruction);
            }
            else
            {
                if (skillInstruction.AssessmentId <= 0) { skillInstruction.AssessmentId = assessmentId; }
                output = await AddSkillInstructionAsync(companyId: companyId, userId: userId, assessmentId: assessmentId, skillInstruction: skillInstruction);
            }
            return output;
        }

        private async Task<int> AddSkillInstructionAsync(int companyId, int userId, int assessmentId, AssessmentSkillInstruction skillInstruction)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessmentInstruction(assessmentInstruction: skillInstruction, companyId: companyId, userId: userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_assessment_skillinstruction", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_skillinstructions.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.assessment_skillinstructions.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added assessment skill instructions.");

            }

            return possibleId;

        }

        private async Task<int> ChangeSkillInstructionAsync(int companyId, int userId, int assessmentId, AssessmentSkillInstruction skillInstruction)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_skillinstructions.ToString(), skillInstruction.Id);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessmentInstruction(assessmentInstruction: skillInstruction, companyId: companyId, userId: userId, assessmentInstructionId: skillInstruction.Id));

            var rowsEffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_assessment_skillinstruction", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowsEffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_skillinstructions.ToString(), skillInstruction.Id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessment_skillinstructions.ToString(), objectId: skillInstruction.Id, userId: userId, companyId: companyId, description: "Changed assessment skill instructions.");

            }

            return rowsEffected;

        }
        #endregion

        #region - private assessment db connector methods -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit"></param>
        /// <returns></returns>
        private Assessment CreateOrFillStaticAssessmentFromReader(NpgsqlDataReader dr, Assessment assessment = null)
        {
            if (assessment == null) assessment = new Assessment();

            if (dr["data_object"] != DBNull.Value)
            {
                assessment = dr["data_object"].ToString().ToObjectFromJson<Assessment>();
            }

            return assessment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit"></param>
        /// <returns></returns>
        private Assessment CreateOrFillAssessmentFromReader(NpgsqlDataReader dr, Assessment assessment = null, string include = null)
        {
            if (assessment == null) assessment = new Assessment();

            assessment.Id = Convert.ToInt32(dr["id"]);

            if (dr.HasColumn("signatures") && dr["signatures"] != DBNull.Value)
                assessment.Signatures = dr["signatures"].ToString().ToObjectFromJson<List<Signature>>();

            if (dr.HasColumn("completed_for_id") && dr["completed_for_id"] != DBNull.Value)
                assessment.CompletedForId = Convert.ToInt32(dr["completed_for_id"]);

            if (dr.HasColumn("completed_at") && dr["completed_at"] != DBNull.Value)
                assessment.CompletedAt = Convert.ToDateTime(dr["completed_at"]);

            if (dr.HasColumn("completedfor") && dr["completedfor"] != DBNull.Value)
                assessment.CompletedFor = dr["completedfor"].ToString();

            if (dr.HasColumn("completedfor_picture") && dr["completedfor_picture"] != DBNull.Value)
                assessment.CompletedForPicture = dr["completedfor_picture"].ToString();

            if (dr.HasColumn("assessor_id") && dr["assessor_id"] != DBNull.Value)
                assessment.AssessorId = Convert.ToInt32(dr["assessor_id"]);
            if (dr.HasColumn("assessor") && dr["assessor"] != DBNull.Value)
                assessment.Assessor = dr["assessor"].ToString();
            if (dr.HasColumn("assessor_picture") && dr["assessor_picture"] != DBNull.Value)
                assessment.AssessorPicture = dr["assessor_picture"].ToString();

            if (dr.HasColumn("start_at") && dr["start_at"] != DBNull.Value)
                assessment.StartDate = Convert.ToDateTime(dr["start_at"]);
            if (dr.HasColumn("end_at") && dr["end_at"] != DBNull.Value)
                assessment.EndDate = Convert.ToDateTime(dr["end_at"]);

            if (dr.HasColumn("is_completed") && dr["is_completed"] != DBNull.Value)
                assessment.IsCompleted = Convert.ToBoolean(dr["is_completed"]);

            if (dr.HasColumn("assessment_template_id") && dr["assessment_template_id"] != DBNull.Value)
                assessment.TemplateId = Convert.ToInt32(dr["assessment_template_id"]);

            if (dr.HasColumn("company_id") && dr["company_id"] != DBNull.Value)
                assessment.CompanyId = Convert.ToInt32(dr["company_id"]);

            if (dr.HasColumn("area_id") && dr["area_id"] != DBNull.Value)
                assessment.AreaId = Convert.ToInt32(dr["area_id"]);

            if (dr.HasColumn("role") && dr["role"] != DBNull.Value)
                assessment.Role = (RoleTypeEnum)Convert.ToInt32(dr["role"]);

            if (dr.HasColumn("assessment_type") && dr["assessment_type"] != DBNull.Value)
                assessment.AssessmentType = (AssessmentTypeEnum)Convert.ToInt32(dr["assessment_type"]);

            if (dr.HasColumn("name") && dr["name"] != DBNull.Value)
                assessment.Name = dr["name"].ToString();

            if (dr.HasColumn("description") && dr["description"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["description"].ToString()))
                assessment.Description = dr["description"].ToString();

            if (dr.HasColumn("media") && dr["media"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["media"].ToString()))
                assessment.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();

            if (assessment.Media != null && assessment.Media.Count > 0)
                assessment.Picture = assessment.Media[0];

            if (dr.HasColumn("signature_type") && dr["signature_type"] != DBNull.Value)
                assessment.SignatureType = (RequiredSignatureTypeEnum)Convert.ToInt32(dr["signature_type"]);

            if (dr.HasColumn("signature_required") && dr["signature_required"] != DBNull.Value)
                assessment.SignatureRequired = Convert.ToBoolean(dr["signature_required"]);

            assessment.NumberOfSignatures = assessment.Signatures?.Count ?? 0;

            if (dr.HasColumn("nr_of_skillinstructions") && dr["nr_of_skillinstructions"] != DBNull.Value)
                assessment.NumberOfSkillInstructions = Convert.ToInt32(dr["nr_of_skillinstructions"]);

            if (dr.HasColumn("total_score") && dr["total_score"] != DBNull.Value)
                assessment.TotalScore = Convert.ToInt32(dr["total_score"]);

            if (dr.HasColumn("assessors") && dr["assessors"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["assessors"].ToString()))
                assessment.Assessors = dr["assessors"].ToString().ToObjectFromJson<List<UserBasic>>();

            var inc = ParseIncludes(include);

            if (inc.Contains(nameof(IncludesEnum.MutationInformation).ToLowerInvariant()))
            {
                if (dr.HasColumn("created_at") && dr["created_at"] != DBNull.Value)
                    assessment.CreatedAt = Convert.ToDateTime(dr["created_at"]);
                if (dr.HasColumn("created_by") && dr["created_by"] != DBNull.Value)
                    assessment.CreatedBy = dr["created_by"].ToString();
                if (dr.HasColumn("created_by_id") && dr["created_by_id"] != DBNull.Value)
                    assessment.CreatedById = Convert.ToInt32(dr["created_by_id"]);

                if (dr.HasColumn("modified_at") && dr["modified_at"] != DBNull.Value)
                    assessment.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                if (dr.HasColumn("modified_by") && dr["modified_by"] != DBNull.Value)
                    assessment.ModifiedBy = dr["modified_by"].ToString();
                if (dr.HasColumn("modified_by_id") && dr["modified_by_id"] != DBNull.Value)
                    assessment.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
            }

            if (dr.HasColumn("version") && dr["version"] != DBNull.Value)
                assessment.Version = dr["version"].ToString();

            //set signature type, assume assessor when its not assessee since assessment only has one assessee
            if (assessment.Signatures != null && assessment.Signatures.Count > 0)
            {
                foreach (var signature in assessment.Signatures)
                {
                    if (signature == null || signature.SignedById == null)
                        continue;

                    if (signature.SignedById == assessment.CompletedForId)
                    {
                        signature.SignatureType = SignatureTypeEnum.Assessee;
                    }
                    else
                    {
                        signature.SignatureType = SignatureTypeEnum.Assessor;
                    }
                }
            }

            return assessment;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="skillInstruction"></param>
        /// <returns></returns>
        private AssessmentSkillInstruction CreateOrFillSkillInstructionFromReader(NpgsqlDataReader dr, AssessmentSkillInstruction skillInstruction = null, string include = null)
        {
            if (skillInstruction == null) skillInstruction = new AssessmentSkillInstruction();

            //"id" int4, "assessment_id" int4, "assessment_template_skillinstruction_id" int4, "completed_for_id" int4, "completed_at" timestamp, "completedfor" varchar, "is_completed" bool,
            //"totalscore" int4, "company_id" int4, "assessment_template_id" int4, "workinstruction_template_id" int4, "index" int4, "created_at" timestamp, "modified_at" timestamp, "created_by_id" int4, "modified_by_id" int4, "created_by" varchar, "modified_by" varchar, "media" text, "name" varchar, "description" text) 

            skillInstruction.Id = Convert.ToInt32(dr["id"]);
            skillInstruction.AssessmentId = Convert.ToInt32(dr["assessment_id"]);
            skillInstruction.AssessmentTemplateId = Convert.ToInt32(dr["assessment_template_id"]);
            skillInstruction.WorkInstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);
            skillInstruction.AssessmentTemplateSkillInstructionId = Convert.ToInt32(dr["assessment_template_skillinstruction_id"]);

            if (dr["completed_for_id"] != DBNull.Value)
            {
                skillInstruction.CompletedForId = Convert.ToInt32(dr["completed_for_id"]);
            }
            if (dr["completed_at"] != DBNull.Value)
            {
                skillInstruction.CompletedAt = Convert.ToDateTime(dr["completed_at"]);
            }
            if (dr["completedfor"] != DBNull.Value)
            {
                skillInstruction.CompletedFor = dr["completedfor"].ToString();
            }
            skillInstruction.IsCompleted = Convert.ToBoolean(dr["is_completed"]);
            skillInstruction.TotalScore = Convert.ToInt32(dr["total_score"]);
            skillInstruction.CompanyId = Convert.ToInt32(dr["company_id"]);
            skillInstruction.Index = Convert.ToInt32(dr["index"]);

            if (dr["start_at"] != DBNull.Value)
            {
                skillInstruction.StartDate = Convert.ToDateTime(dr["start_at"]);
            }
            if (dr["end_at"] != DBNull.Value)
            {
                skillInstruction.EndDate = Convert.ToDateTime(dr["end_at"]);
            }

            skillInstruction.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                skillInstruction.Description = dr["description"].ToString();
            }

            if (dr.HasColumn("assessors") && dr["assessors"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["assessors"].ToString()))
                skillInstruction.Assessors = dr["assessors"].ToString().ToObjectFromJson<List<UserBasic>>();

            if (dr["media"] != DBNull.Value)
            {
                skillInstruction.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
            }

            if (skillInstruction.Media != null && skillInstruction.Media.Count > 0)
            {
                skillInstruction.Picture = skillInstruction.Media[0];
            }

            if (!string.IsNullOrEmpty(include))
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MutationInformation.ToString().ToLower()))
                {
                    skillInstruction.CreatedAt = Convert.ToDateTime(dr["created_at"]);
                    skillInstruction.CreatedBy = dr["created_by"].ToString();
                    skillInstruction.CreatedById = Convert.ToInt32(dr["created_by_id"]);

                    skillInstruction.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                    skillInstruction.ModifiedBy = dr["modified_by"].ToString();
                    skillInstruction.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
                }
            }

            return skillInstruction;
        }

        private async Task<InstructionItem> CreateOrFillInstructionItemFromReader(NpgsqlDataReader dr, InstructionItem instructionItem = null, string include = null)
        {
            if (instructionItem == null) instructionItem = new InstructionItem();

            //("id" int4, "assessment_id" int4, "assessment_template_skillinstruction_id" int4, "completed_for_id" int4, "completed_at" timestamp, "completedfor" varchar, "is_completed" bool, "score" int4, "company_id" int4, "assessment_template_id" int4, "workinstruction_template_id" int4, "index" int4, "created_at" timestamp, "modified_at" timestamp, "created_by_id" int4, "modified_by_id" int4, "created_by" varchar, "modified_by" varchar, "media" text, "name" varchar, "description" text) AS $BODY$BEGIN

            instructionItem.Id = Convert.ToInt32(dr["id"]);
            instructionItem.AssessmentId = Convert.ToInt32(dr["assessment_id"]);
            instructionItem.AssessmentTemplateId = Convert.ToInt32(dr["assessment_template_id"]);
            instructionItem.AssessmentSkillInstructionId = Convert.ToInt32(dr["assessment_skillinstruction_id"]);
            instructionItem.WorkInstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);
            instructionItem.WorkInstructionTemplateItemId = Convert.ToInt32(dr["workinstruction_template_item_id"]);

            if (dr["completed_for_id"] != DBNull.Value)
            {
                instructionItem.CompletedForId = Convert.ToInt32(dr["completed_for_id"]);
            }
            if (dr["completed_at"] != DBNull.Value)
            {
                instructionItem.CompletedAt = Convert.ToDateTime(dr["completed_at"]);
            }
            if (dr["completedfor"] != DBNull.Value)
            {
                instructionItem.CompletedFor = dr["completedfor"].ToString();
            }
            if (dr["scored_at"] != DBNull.Value)
            {
                instructionItem.ScoredAt = Convert.ToDateTime(dr["scored_at"]);
            }
            instructionItem.IsCompleted = Convert.ToBoolean(dr["is_completed"]);
            instructionItem.Score = Convert.ToInt32(dr["score"]);
            instructionItem.CompanyId = Convert.ToInt32(dr["company_id"]);
            instructionItem.Index = Convert.ToInt32(dr["index"]);

            instructionItem.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                instructionItem.Description = dr["description"].ToString();
            }

            if (dr["assessor_id"] != DBNull.Value)
            {
                if (instructionItem.Assessor == null)
                    instructionItem.Assessor = new UserBasic();
                
                instructionItem.Assessor.Id = Convert.ToInt32(dr["assessor_id"]);
                
                if (dr["assessorname"] != DBNull.Value)
                    instructionItem.Assessor.Name = Convert.ToString(dr["assessorname"]);
                
                if (dr["assessorpicture"] != DBNull.Value)
                    instructionItem.Assessor.Picture = Convert.ToString(dr["assessorpicture"]);
            }
            if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
            {
                instructionItem.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
                if (instructionItem.Media.Count == 2)
                {
                    instructionItem.VideoThumbnail = instructionItem.Media[0];
                    instructionItem.Video = instructionItem.Media[1];
                }
                else
                {
                    instructionItem.Picture = instructionItem.Media.FirstOrDefault();
                }
            }

            if (dr.HasColumn("attachments"))
            {
                var attachmentsJson = Convert.ToString(dr["attachments"] ?? "");
                if (attachmentsJson == "")
                {
                    instructionItem.Attachments = new List<Attachment>();
                }
                else
                {
                    instructionItem.Attachments = Convert.ToString(dr["attachments"] ?? "").ToObjectFromJson<List<Attachment>>();
                }
            }


            if (!string.IsNullOrEmpty(include))
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MutationInformation.ToString().ToLower()))
                {
                    instructionItem.CreatedAt = Convert.ToDateTime(dr["created_at"]);
                    instructionItem.CreatedBy = dr["created_by"].ToString();
                    instructionItem.CreatedById = Convert.ToInt32(dr["created_by_id"]);

                    instructionItem.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                    instructionItem.ModifiedBy = dr["modified_by"].ToString();
                    instructionItem.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
                }
            }

            return instructionItem;

        }




        /// <summary>
        /// GetNpgsqlParametersFromAssessment; Creates a list of NpgsqlParameters, and fills it based on the supplied Assessment object.
        /// NOTE! intended for use with the audit stored procedures within the database.
        /// </summary>
        /// <param name="assessment">The supplied Audit object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="assessmentId">AssessmentId (DB: audits_audit.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromAssessment(Assessment assessment, int companyId, int userId, int assessmentId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (assessmentId > 0) parameters.Add(new NpgsqlParameter("@_id", assessmentId));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", assessment.TemplateId));

            var signatures = assessment.Signatures != null && assessment.Signatures.Count > 0 ? assessment.Signatures : new List<Signature>();
            parameters.Add(new NpgsqlParameter("@_signatures", signatures.ToJsonFromObject().ToString()));
            if (assessment.CompletedAt != null)
                parameters.Add(new NpgsqlParameter("@_completedat", assessment.CompletedAt));
            if (assessment.CompletedForId != null)
                parameters.Add(new NpgsqlParameter("@_completedforid", assessment.CompletedForId));
            if (assessment.AssessorId != null)
                parameters.Add(new NpgsqlParameter("@_assessorid", assessment.AssessorId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_iscompleted", assessment.IsCompleted));
            if (assessment.TotalScore != null)
                parameters.Add(new NpgsqlParameter("@_totalscore", Convert.ToInt32(assessment.TotalScore)));
            return parameters;
        }


        private List<NpgsqlParameter> GetNpgsqlParametersFromAssessmentInstruction(AssessmentSkillInstruction assessmentInstruction, int companyId, int userId, int assessmentInstructionId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (assessmentInstructionId > 0) parameters.Add(new NpgsqlParameter("@_id", assessmentInstructionId));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_assessmentid", assessmentInstruction.AssessmentId));
            parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", assessmentInstruction.AssessmentTemplateId));
            parameters.Add(new NpgsqlParameter("@_assessmenttemplateskillinstructionid", assessmentInstruction.AssessmentTemplateSkillInstructionId));
            parameters.Add(new NpgsqlParameter("@_iscompleted", assessmentInstruction.IsCompleted));
            if (assessmentInstruction.CompletedAt != null)
                parameters.Add(new NpgsqlParameter("@_completedat", assessmentInstruction.CompletedAt));
            if (assessmentInstruction.CompletedForId != null)
                parameters.Add(new NpgsqlParameter("@_completedforid", assessmentInstruction.CompletedForId));
            if (assessmentInstruction.TotalScore != null)
                parameters.Add(new NpgsqlParameter("@_totalscore", assessmentInstruction.TotalScore));
            if (assessmentInstruction.StartDate != null)
                parameters.Add(new NpgsqlParameter("@_startdate", new DateTime(assessmentInstruction.StartDate.Value.Ticks)));

            return parameters;
        }


        private List<NpgsqlParameter> GetNpgsqlParametersFromAssessmentInstructionItem(InstructionItem assessmentInstructionItem, int companyId, int userId, int assessmentItemId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (assessmentItemId > 0) parameters.Add(new NpgsqlParameter("@_id", assessmentItemId));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_assessmentid", assessmentInstructionItem.AssessmentId));
            parameters.Add(new NpgsqlParameter("@_assessmentskillinstructionid", assessmentInstructionItem.AssessmentSkillInstructionId));
            parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", assessmentInstructionItem.WorkInstructionTemplateId));
            parameters.Add(new NpgsqlParameter("@_workinstructiontemplateitemid", assessmentInstructionItem.WorkInstructionTemplateItemId));
            parameters.Add(new NpgsqlParameter("@_iscompleted", assessmentInstructionItem.IsCompleted));
            if (assessmentInstructionItem.CompletedAt != null)
                parameters.Add(new NpgsqlParameter("@_completedat", assessmentInstructionItem.CompletedAt));
            if (assessmentInstructionItem.CompletedForId != null)
                parameters.Add(new NpgsqlParameter("@_completedforid", assessmentInstructionItem.CompletedForId));
            if (assessmentInstructionItem.Score != null)
                parameters.Add(new NpgsqlParameter("@_score", assessmentInstructionItem.Score));
            if (assessmentInstructionItem.Assessor != null)
                parameters.Add(new NpgsqlParameter("@_assessor_id", assessmentInstructionItem.Assessor.Id));
            if (assessmentInstructionItem.ScoredAt != null)
                parameters.Add(new NpgsqlParameter("@_scored_at", assessmentInstructionItem.ScoredAt.Value));

            return parameters;
        }
        #endregion

        #region - assessment versioning methods -
        private async Task<Assessment> ApplyTemplateVersionToAssessment(Assessment assessment, int companyId, string include = null)
        {
            if (!string.IsNullOrEmpty(assessment.Version))
            {
                AssessmentTemplate versionedTemplate = await _flattenedAssessmentManager.RetrieveFlattenData(templateId: assessment.TemplateId, companyId: companyId, version: assessment.Version);

                if (versionedTemplate != null)
                    assessment.ApplyTemplateVersion(versionedTemplate, include);
                else
                    _logger.LogWarning($"ApplyTemplateVersionToAssessment(); Template version not applied because requested version wasn't found. AssessmentTemplateId: {assessment.TemplateId}, version: {assessment.Version}");
            }
            return assessment;
        }

        private async Task<List<Assessment>> ApplyTemplateVersionToAssessments(List<Assessment> assessments, int companyId, string include = null)
        {
            //cache versioned templates based on template id and version
            Dictionary<KeyValuePair<int, string>, AssessmentTemplate> VersionedAssessmentsCache = new();
            foreach (Assessment assessment in assessments)
            {
                if (!string.IsNullOrEmpty(assessment.Version) && assessment.Version != await _flattenedAssessmentManager.RetrieveLatestAvailableVersion(assessment.TemplateId, companyId))
                {
                    AssessmentTemplate versionedTemplate = null;
                    KeyValuePair<int, string> TemplateIdVersionPair = new(assessment.TemplateId, assessment.Version);

                    if (VersionedAssessmentsCache.ContainsKey(TemplateIdVersionPair))
                    {
                        //get correct version of template from cache if it is already present
                        versionedTemplate = VersionedAssessmentsCache.GetValueOrDefault(TemplateIdVersionPair);
                    }
                    else
                    {
                        //retrieve the correct version of the template from the database and add it to the cache
                        versionedTemplate = await _flattenedAssessmentManager.RetrieveFlattenData(templateId: assessment.TemplateId, companyId: companyId, version: assessment.Version);
                        VersionedAssessmentsCache.Add(TemplateIdVersionPair, versionedTemplate);
                    }

                    if (versionedTemplate != null)
                        assessment.ApplyTemplateVersion(versionedTemplate, include);
                    else
                        _logger.LogWarning($"ApplyTemplateVersionToAssessments(); Template version not applied because requested version wasn't found. AssessmentTemplateId: {assessment.TemplateId}, version: {assessment.Version}");
                }
            }
            return assessments;
        }
        #endregion


        #region - public assessment template -
        public async Task<AssessmentTemplate> GetAssessmentTemplateAsync(int companyId, int assessmentTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var assessmentTemplate = new AssessmentTemplate();

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", assessmentTemplateId));

                using (dr = await _manager.GetDataReader("get_assessmenttemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        assessmentTemplate = CreateOrFillAssessmentTemplateFromReader(dr, assessmentTemplate: assessmentTemplate, include: include);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetAssessmentTemplateAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (assessmentTemplate.Id > 0)
            {

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower())) assessmentTemplate.SkillInstructions = await GetSkillInstructionsWithAssessmentTemplateAsync(companyId: companyId, assessmentTemplate.Id, include: include, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) assessmentTemplate = await AppendAreaPathsToAssessmentTemplateAsync(companyId: companyId, assessmentTemplate: assessmentTemplate, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) assessmentTemplate.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.AssessmentTemplate, id: assessmentTemplateId);

                return assessmentTemplate;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> AddAssessmentTemplateAsync(int companyId, int userId, AssessmentTemplate assessmentTemplate)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessmentTemplate(assessmentTemplate: assessmentTemplate, companyId: companyId, userId: userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_assessmenttemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                assessmentTemplate.Tags ??= new();
                await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.AssessmentTemplate, possibleId, assessmentTemplate.Tags, companyId, userId);

                if (assessmentTemplate.SkillInstructions != null && assessmentTemplate.SkillInstructions.Count > 0)
                {
                    var rowsEffected = await ChangeAssessmentTemplateAddOrChangeSkillInstructionAsync(companyId: companyId, userId: userId, assessmentTemplateId: possibleId, assessmentTemplate.SkillInstructions);
                }


            }

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_templates.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.assessment_templates.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added assessment template.");

            }

            return possibleId;
        }

        public async Task<bool> ChangeAssessmentTemplateAsync(int companyId, int userId, int assessmentTemplateId, AssessmentTemplate assessmentTemplate)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_templates.ToString(), assessmentTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessmentTemplate(assessmentTemplate: assessmentTemplate, companyId: companyId, userId: userId, assessmentTemplateId: assessmentTemplateId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_assessmenttemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (assessmentTemplateId > 0 && assessmentTemplate.SkillInstructions != null)
            {
                var rowsEffected = await ChangeAssessmentTemplateAddOrChangeSkillInstructionAsync(companyId: companyId, userId: userId, assessmentTemplateId: assessmentTemplateId, assessmentTemplate.SkillInstructions);
            }


            if (rowseffected > 0)
            {
                assessmentTemplate.Tags ??= new();
                await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.AssessmentTemplate, assessmentTemplateId, assessmentTemplate.Tags, companyId, userId);

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_templates.ToString(), assessmentTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessment_templates.ToString(), objectId: assessmentTemplateId, userId: userId, companyId: companyId, description: "Changed assessment template.");

            }
            return rowseffected > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="assessmentTemplateId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public async Task<bool> SetAssessmentTemplateActiveAsync(int companyId, int userId, int assessmentTemplateId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_templates.ToString(), assessmentTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", assessmentTemplateId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_assessmenttemplate_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_templates.ToString(), assessmentTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessment_templates.ToString(), objectId: assessmentTemplateId, userId: userId, companyId: companyId, description: "Changed assessment template active state.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - public assessment templates -
        public async Task<List<AssessmentTemplate>> GetAssessmentTemplatesAsync(int companyId, int? userId = null, AssessmentFilters? filters = null, string include = null)
        {
            var output = new List<AssessmentTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (!String.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }
                }

                using (dr = await _manager.GetDataReader("get_assessmenttemplates", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var audittemplate = CreateOrFillAssessmentTemplateFromReader(dr, include: include);
                        output.Add(audittemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetAssessmentTemplatesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToAssessmentTemplatesAsync(companyId: companyId, assessmentTemplates: output);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower()))) output = await AppendSkillInstructionsToAssessmentTemplatesAsync(companyId: companyId, assessmentTemplates: output, filters: filters, userId: userId, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToAssessmentTemplatesAsync(companyId: companyId, assessmentTemplates: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
            }

            return output;
        }


        public async Task<List<int>> GetWorkInstructionConnectedAssessmentTemplateIds(int companyId, int workInstructionTemplateId)
        {
            var output = new List<int>();

            NpgsqlDataReader dr = null;
            try
            {

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", workInstructionTemplateId));

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate_connected_assessmenttemplates", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr.HasColumn("id") && dr["id"] != DBNull.Value)
                        {
                            var assessmentTemplateId = Convert.ToInt32(dr["id"]);
                            output.Add(assessmentTemplateId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetWorkInstructionConnectedAssessmentTemplates(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }
        #endregion

        #region - privates assessment template (retrieval methods) -
        /// <summary>
        /// AppendAreaPathsToAssessmentsAsync; Add the AreaPath to the Assessment. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="assessmentTemplate">Assessment template.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>Assessments including area full path. </returns>
        private async Task<AssessmentTemplate> AppendAreaPathsToAssessmentTemplateAsync(int companyId, AssessmentTemplate assessmentTemplate, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {

                var area = areas?.Where(x => x.Id == assessmentTemplate.AreaId)?.FirstOrDefault();
                if (area != null)
                {
                    if (addAreaPath) assessmentTemplate.AreaPath = area.FullDisplayName;
                    if (addAreaPathIds) assessmentTemplate.AreaPathIds = area.FullDisplayIds;
                }

            }
            return assessmentTemplate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="assessmentTemplateId"></param>
        /// <param name="include"></param>
        /// <param name="connectionKind"></param>
        /// <returns></returns>
        private async Task<List<AssessmentTemplateSkillInstruction>> GetSkillInstructionsWithAssessmentTemplateAsync(int companyId, int assessmentTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<AssessmentTemplateSkillInstruction>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", assessmentTemplateId));


                using (dr = await _manager.GetDataReader("get_assessmenttemplate_skillinstructions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var skillInstructionTemplate = CreateOrFillSkillInstructionTemplateFromReader(dr, include: include);
                        output.Add(skillInstructionTemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.GetSkillInstructionsWithAssessmentTemplateAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionItems.ToString().ToLower()))) output = await AppendSkillInstructionItemsToSkillInstructionTemplatesAsync(companyId: companyId, skillInstructionTemplates: output, assessmentTemplateId: assessmentTemplateId, include: include, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToSkillInstructionTemplatesAsync(companyId: companyId, assessmentTemplateId: assessmentTemplateId, assessmentTemplateSkillInstructions: output);
            }

            return output;
        }
        #endregion

        #region - privates assessment templates (retrieval methods) -
        /// <summary>
        /// AppendTagsToAssessmentTemplatesAsync; append tags to AssessmentTemplate collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="assessmentTemplates">Collection of AssessmentTemplate</param>
        /// <returns>Collection of AssessmentTemplate</returns>
        private async Task<List<AssessmentTemplate>> AppendTagsToAssessmentTemplatesAsync(int companyId, List<AssessmentTemplate> assessmentTemplates)
        {
            var allTagsOnAssessmentTemplates = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.AssessmentTemplate);
            if (allTagsOnAssessmentTemplates != null)
            {
                foreach (var assessmentTemplate in assessmentTemplates)
                {
                    var tagsOnThisAssessmentTemplate = allTagsOnAssessmentTemplates.Where(t => t.ObjectId == assessmentTemplate.Id).ToList();
                    if (tagsOnThisAssessmentTemplate != null && tagsOnThisAssessmentTemplate.Count > 0)
                    {
                        assessmentTemplate.Tags ??= new List<Models.Tags.Tag>();
                        assessmentTemplate.Tags.AddRange(tagsOnThisAssessmentTemplate);
                    }

                }
            }

            return assessmentTemplates;
        }

        private async Task<List<Assessment>> AppendTagsToAssessmentsAsync(int companyId, List<Assessment> assessments)
        {
            var allTagsOnAssessments = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.Assessment);
            if (allTagsOnAssessments != null)
            {
                foreach (var assessment in assessments)
                {
                    var tagsOnThisAssessment = allTagsOnAssessments.Where(t => t.ObjectId == assessment.Id).ToList();
                    if (tagsOnThisAssessment != null && tagsOnThisAssessment.Count > 0)
                    {
                        assessment.Tags ??= new List<Models.Tags.Tag>();
                        assessment.Tags.AddRange(tagsOnThisAssessment);
                    }

                }
            }

            return assessments;
        }

        private async Task<List<AssessmentTemplateSkillInstruction>> AppendTagsToSkillInstructionTemplatesAsync(int companyId, List<AssessmentTemplateSkillInstruction> assessmentTemplateSkillInstructions, int? assessmentTemplateId = null)
        {
            var allTagsOnAssassmentTemplateSkillInstructions = await _tagManager.GetTagsOnAssessmentTemplateSkillInstructionsAsync(companyId: companyId, assessmentTemplateId: assessmentTemplateId);

            if (allTagsOnAssassmentTemplateSkillInstructions != null)
            {
                foreach (var skillInstruction in assessmentTemplateSkillInstructions)
                {
                    var tagsOnThisSkillInstruction = allTagsOnAssassmentTemplateSkillInstructions.Where(t => t.ObjectId == skillInstruction.WorkInstructionTemplateId).ToList();
                    if (tagsOnThisSkillInstruction != null && tagsOnThisSkillInstruction.Count > 0)
                    {
                        skillInstruction.Tags ??= new List<Models.Tags.Tag>();
                        skillInstruction.Tags.AddRange(tagsOnThisSkillInstruction);
                    }

                }
            }

            return assessmentTemplateSkillInstructions;
        }

        private async Task<List<AssessmentSkillInstruction>> AppendTagsToSkillInstructionsAsync(int companyId, List<AssessmentSkillInstruction> assessmentSkillInstructions, int? assessmentId = null)
        {
            var allTagsOnAssassmentSkillInstructions = await _tagManager.GetTagsOnAssessmentSkillInstructions(companyId: companyId, assessmentId: assessmentId);

            if (allTagsOnAssassmentSkillInstructions != null)
            {
                foreach (var skillInstruction in assessmentSkillInstructions)
                {
                    var tagsOnThisSkillInstruction = allTagsOnAssassmentSkillInstructions.Where(t => t.ObjectId == skillInstruction.WorkInstructionTemplateId).ToList();
                    if (tagsOnThisSkillInstruction != null && tagsOnThisSkillInstruction.Count > 0)
                    {
                        skillInstruction.Tags ??= new List<Models.Tags.Tag>();
                        skillInstruction.Tags.AddRange(tagsOnThisSkillInstruction);
                    }

                }
            }

            return assessmentSkillInstructions;
        }

        /// <summary>
        /// AppendAreaPathsToAssessmentTemplatesAsync; Add the AreaPath to the Assessment. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="assessmentTemplates">List of assessment templates.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>Assessments including area full path. </returns>
        private async Task<List<AssessmentTemplate>> AppendAreaPathsToAssessmentTemplatesAsync(int companyId, List<AssessmentTemplate> assessmentTemplates, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var assessmentTemplate in assessmentTemplates)
                {
                    var area = areas?.Where(x => x.Id == assessmentTemplate.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        if (addAreaPath) assessmentTemplate.AreaPath = area.FullDisplayName;
                        if (addAreaPathIds) assessmentTemplate.AreaPathIds = area.FullDisplayIds;
                    }
                }
            }
            return assessmentTemplates;
        }

        private async Task<List<AssessmentTemplate>> AppendSkillInstructionsToAssessmentTemplatesAsync(int companyId, List<AssessmentTemplate> assessmentTemplates, AssessmentFilters? filters, int? userId, string include)
        {
            var instructions = new List<AssessmentTemplateSkillInstruction>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }
                }

                using (dr = await _manager.GetDataReader("get_assessmenttemplate_skillinstructions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var skillInstructionTemplate = CreateOrFillSkillInstructionTemplateFromReader(dr, include: include);
                        instructions.Add(skillInstructionTemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.AppendSkillInstructionsToAssessmentTemplatesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (instructions.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionItems.ToString().ToLower()))) instructions = await AppendSkillInstructionItemsToSkillInstructionTemplatesAsync(companyId: companyId, skillInstructionTemplates: instructions, filters: filters, userId: userId, include: include);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) instructions = await AppendTagsToSkillInstructionTemplatesAsync(companyId: companyId, assessmentTemplateSkillInstructions: instructions);

            }

            if (instructions.Count > 0)
            {
                foreach (var item in assessmentTemplates)
                {
                    item.SkillInstructions = instructions.Where(x => x.CompanyId == companyId && x.AssessmentTemplateId == item.Id).OrderBy(y => y.Index).ToList();
                }
            }

            return assessmentTemplates;
        }
        #endregion

        #region - privates skill instruction templates (retrieval methods) -
        private async Task<List<AssessmentTemplateSkillInstruction>> AppendSkillInstructionItemsToSkillInstructionTemplatesAsync(int companyId, int assessmentTemplateId, List<AssessmentTemplateSkillInstruction> skillInstructionTemplates, string include, ConnectionKind connectionKind)
        {
            var instructionItems = new List<InstructionItemTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", assessmentTemplateId));


                using (dr = await _manager.GetDataReader("get_assessmenttemplate_skillinstruction_items", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var instructionItemTemplate = CreateOrFillInstructionItemTemplateFromReader(dr, include: include);
                        instructionItems.Add(instructionItemTemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.AppendSkillInstructionItemsToSkillInstructionTemplatesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (instructionItems.Count > 0)
            {
                foreach (var item in skillInstructionTemplates)
                {
                    item.InstructionItems = instructionItems.Where(x => x.CompanyId == companyId && x.InstructionTemplateId == item.WorkInstructionTemplateId).OrderBy(y => y.Index).ToList();
                }
            }

            return skillInstructionTemplates;

        }

        private async Task<List<AssessmentTemplateSkillInstruction>> AppendSkillInstructionItemsToSkillInstructionTemplatesAsync(int companyId, List<AssessmentTemplateSkillInstruction> skillInstructionTemplates, AssessmentFilters? filters, int? userId, string include)
        {
            var instructionItems = new List<InstructionItemTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }
                }

                using (dr = await _manager.GetDataReader("get_assessmenttemplate_skillinstruction_items", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var instructionItemTemplate = CreateOrFillInstructionItemTemplateFromReader(dr, include: include);
                        instructionItems.Add(instructionItemTemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AssessmentManager.AppendInstructionTemplateItemsToInstructionTemplatesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            if (instructionItems.Count > 0)
            {
                foreach (var item in skillInstructionTemplates)
                {
                    //TODO fix? check why?
                    item.InstructionItems = instructionItems.Where(x => x.CompanyId == companyId
                                                                   && x.InstructionTemplateId == item.WorkInstructionTemplateId
                                                                   && x.AssessmentTemplateId == item.AssessmentTemplateId).OrderBy(y => y.Index).ToList();

                }
            }

            return skillInstructionTemplates;
        }
        #endregion

        #region - privates assessment template (mutation methods) -
        private async Task<int> ChangeAssessmentTemplateAddOrChangeSkillInstructionAsync(int companyId, int userId, int assessmentTemplateId, List<AssessmentTemplateSkillInstruction> skillInstructionTemplates)
        {
            var output = 0;
            if (skillInstructionTemplates != null)
            {
                var currentSkillInstructionTemplates = await GetSkillInstructionsWithAssessmentTemplateAsync(companyId: companyId, assessmentTemplateId: assessmentTemplateId); //TODO replace with method with only ids
                List<AssessmentTemplateSkillInstruction> toBeRemoved = null;
                if (skillInstructionTemplates.Count == 0)
                {
                    toBeRemoved = currentSkillInstructionTemplates;
                }
                else
                {
                    toBeRemoved = currentSkillInstructionTemplates.Where(x => !skillInstructionTemplates.Select(y => y.Id).Contains(x.Id)).ToList();
                }

                foreach (var skillinstructiontemplate in toBeRemoved)
                {
                    var setInactive = await SetAssessmentTemplateSkillInstructionActiveAsync(companyId: companyId, userId: userId, assessmentTemplateSkillInstructionId: skillinstructiontemplate.Id, assessmentTemplateId: assessmentTemplateId, false);
                    if (setInactive)
                    {
                        output = output + 1;
                    }
                }


                foreach (var instructionTemplate in skillInstructionTemplates)
                {
                    if (instructionTemplate.Id > 0)
                    {
                        var changed = await ChangeAssessmentTemplateSkillInstructionAsync(companyId: companyId, userId: userId, skillInstructionTemplateId: instructionTemplate.Id, instructionTemplate: instructionTemplate);
                        if (changed > 0)
                        {
                            output = output + 1;
                        }
                    }
                    else
                    {
                        //set correct id
                        if (!instructionTemplate.AssessmentTemplateId.HasValue || instructionTemplate.AssessmentTemplateId.Value <= 0) { instructionTemplate.AssessmentTemplateId = assessmentTemplateId; }
                        var added = await AddAssessmentTemplateSkillInstructionAsync(companyId: companyId, userId: userId, assessmentTemplateId: assessmentTemplateId, instructionTemplate: instructionTemplate);
                        if (added > 0)
                        {
                            output = output + 1;
                        }
                    }
                }
            }

            return output;
        }

        private async Task<int> AddAssessmentTemplateSkillInstructionAsync(int companyId, int userId, int assessmentTemplateId, AssessmentTemplateSkillInstruction instructionTemplate)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessentTemplateSkillInstruction(skillInstructionTemplate: instructionTemplate, userId: userId, companyId: companyId));

            //NOTE! add_assessmenttemplate_skillinstruction is a smart add method. If a item is removed previously and added again the record will be recycled and a update will be done to the existing record and activating it again. 
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_assessmenttemplate_skillinstruction", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return possibleId;
        }

        private async Task<int> ChangeAssessmentTemplateSkillInstructionAsync(int companyId, int userId, int skillInstructionTemplateId, AssessmentTemplateSkillInstruction instructionTemplate)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAssessentTemplateSkillInstruction(skillInstructionTemplate: instructionTemplate, userId: userId, companyId: companyId, skillInstructionTemplateId: skillInstructionTemplateId));

            var rowsEffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_assessmenttemplate_skillinstruction", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowsEffected;
        }

        private async Task<bool> SetAssessmentTemplateSkillInstructionActiveAsync(int companyId, int userId, int assessmentTemplateSkillInstructionId, int assessmentTemplateId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_template_skillinstructions.ToString(), assessmentTemplateSkillInstructionId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", assessmentTemplateSkillInstructionId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", assessmentTemplateId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_assessmenttemplate_skillinstruction_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.assessment_template_skillinstructions.ToString(), assessmentTemplateSkillInstructionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.assessment_template_skillinstructions.ToString(), objectId: assessmentTemplateSkillInstructionId, userId: userId, companyId: companyId, description: "Changed assessment template instruction active state.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - private template(s) db connector methods -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit"></param>
        /// <returns></returns>
        private AssessmentTemplate CreateOrFillAssessmentTemplateFromReader(NpgsqlDataReader dr, AssessmentTemplate assessmentTemplate = null, string include = null)
        {
            if (assessmentTemplate == null) assessmentTemplate = new AssessmentTemplate();

            assessmentTemplate.Id = Convert.ToInt32(dr["id"]);
            assessmentTemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["area_id"] != DBNull.Value)
            {
                assessmentTemplate.AreaId = Convert.ToInt32(dr["area_id"]);
            }
            if (dr["role"] != DBNull.Value)
            {
                assessmentTemplate.Role = (RoleTypeEnum)dr["role"];
            }
            assessmentTemplate.AssessmentType = (AssessmentTypeEnum)dr["assessment_type"];
            assessmentTemplate.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                assessmentTemplate.Description = dr["description"].ToString();
            }

            if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
            {
                assessmentTemplate.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
            }

            if (assessmentTemplate.Media != null && assessmentTemplate.Media.Count > 0)
            {
                assessmentTemplate.Picture = assessmentTemplate.Media[0];
            }

            assessmentTemplate.SignatureType = (RequiredSignatureTypeEnum)dr["signature_type"];
            assessmentTemplate.SignatureRequired = Convert.ToBoolean(dr["signature_required"]);

            assessmentTemplate.NumberOfAssessments = Convert.ToInt32(dr["nr_of_assessments"]);
            assessmentTemplate.NumberOfSkillInstructions = Convert.ToInt32(dr["nr_of_skillinstructions"]);
            assessmentTemplate.NumberOfOpenAssessments = Convert.ToInt32(dr["nr_of_open_assessments"]);

            if (dr["assessment_last_activity_date"] != DBNull.Value)
            {
                assessmentTemplate.LastActivityDate = Convert.ToDateTime(dr["assessment_last_activity_date"]);
            }

            if (!string.IsNullOrEmpty(include))
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MutationInformation.ToString().ToLower()))
                {
                    assessmentTemplate.CreatedAt = Convert.ToDateTime(dr["created_at"]);
                    assessmentTemplate.CreatedBy = dr["created_by"].ToString();
                    assessmentTemplate.CreatedById = Convert.ToInt32(dr["created_by_id"]);

                    assessmentTemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                    assessmentTemplate.ModifiedBy = dr["modified_by"].ToString();
                    assessmentTemplate.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
                }
            }

            if (dr.HasColumn("version"))
            {
                if (dr["version"] != DBNull.Value)
                {
                    assessmentTemplate.Version = Convert.ToString(dr["version"]);
                }
            }

            return assessmentTemplate;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit"></param>
        /// <returns></returns>
        private AssessmentTemplateSkillInstruction CreateOrFillSkillInstructionTemplateFromReader(NpgsqlDataReader dr, AssessmentTemplateSkillInstruction skillInstructionTemplate = null, string include = null)
        {
            if (skillInstructionTemplate == null) skillInstructionTemplate = new AssessmentTemplateSkillInstruction();
            //assessment_template_skillinstruction_id
            skillInstructionTemplate.Id = Convert.ToInt32(dr["id"]);
            skillInstructionTemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            skillInstructionTemplate.AssessmentTemplateId = Convert.ToInt32(dr["assessment_template_id"]);
            skillInstructionTemplate.WorkInstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);
            skillInstructionTemplate.Index = Convert.ToInt32(dr["index"]);

            skillInstructionTemplate.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                skillInstructionTemplate.Description = dr["description"].ToString();
            }

            if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
            {
                skillInstructionTemplate.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
            }

            if (skillInstructionTemplate.Media != null && skillInstructionTemplate.Media.Count > 0)
            {
                skillInstructionTemplate.Picture = skillInstructionTemplate.Media[0];
            }

            if (!string.IsNullOrEmpty(include))
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MutationInformation.ToString().ToLower()))
                {
                    skillInstructionTemplate.CreatedAt = Convert.ToDateTime(dr["created_at"]);
                    skillInstructionTemplate.CreatedBy = dr["created_by"].ToString();
                    skillInstructionTemplate.CreatedById = Convert.ToInt32(dr["created_by_id"]);

                    skillInstructionTemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                    skillInstructionTemplate.ModifiedBy = dr["modified_by"].ToString();
                    skillInstructionTemplate.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
                }
            }

            return skillInstructionTemplate;

        }

        private InstructionItemTemplate CreateOrFillInstructionItemTemplateFromReader(NpgsqlDataReader dr, InstructionItemTemplate instructionItemTemplate = null, string include = null)
        {
            if (instructionItemTemplate == null) instructionItemTemplate = new InstructionItemTemplate();

            instructionItemTemplate.Id = Convert.ToInt32(dr["id"]);
            instructionItemTemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            instructionItemTemplate.InstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);
            instructionItemTemplate.AssessmentTemplateId = Convert.ToInt32(dr["assessment_template_id"]);
            instructionItemTemplate.Index = Convert.ToInt32(dr["index"]);

            instructionItemTemplate.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                instructionItemTemplate.Description = dr["description"].ToString();
            }

            if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
            {
                instructionItemTemplate.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
                if (instructionItemTemplate.Media.Count == 2)
                {
                    instructionItemTemplate.VideoThumbnail = instructionItemTemplate.Media[0];
                    instructionItemTemplate.Video = instructionItemTemplate.Media[1];
                }
                else
                {
                    instructionItemTemplate.Picture = instructionItemTemplate.Media.FirstOrDefault();
                }
            }

            if (dr.HasColumn("attachments"))
            {
                var attachmentsJson = Convert.ToString(dr["attachments"] ?? "");
                if (attachmentsJson == "")
                {
                    instructionItemTemplate.Attachments = new List<Attachment>();
                }
                else
                {
                    instructionItemTemplate.Attachments = Convert.ToString(dr["attachments"] ?? "").ToObjectFromJson<List<Attachment>>();
                }
            }

            if (!string.IsNullOrEmpty(include))
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MutationInformation.ToString().ToLower()))
                {
                    instructionItemTemplate.CreatedAt = Convert.ToDateTime(dr["created_at"]);
                    instructionItemTemplate.CreatedBy = dr["created_by"].ToString();
                    instructionItemTemplate.CreatedById = Convert.ToInt32(dr["created_by_id"]);

                    instructionItemTemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                    instructionItemTemplate.ModifiedBy = dr["modified_by"].ToString();
                    instructionItemTemplate.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
                }
            }

            return instructionItemTemplate;



        }
        //

        private List<NpgsqlParameter> GetNpgsqlParametersFromAssessmentTemplate(AssessmentTemplate assessmentTemplate, int companyId, int userId, int assessmentTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (assessmentTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", assessmentTemplateId));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId)); //for company check
            parameters.Add(new NpgsqlParameter("@_userid", userId)); //used for modified / created by ids
            parameters.Add(new NpgsqlParameter("@_areaid", assessmentTemplate.AreaId));
            parameters.Add(new NpgsqlParameter("@_assessmenttype", Convert.ToInt32(assessmentTemplate.AssessmentType)));

            parameters.Add(new NpgsqlParameter("@_name", assessmentTemplate.Name));

            parameters.Add(new NpgsqlParameter("@_signaturetype", Convert.ToInt32(assessmentTemplate.SignatureType)));
            parameters.Add(new NpgsqlParameter("@_signaturerequired", Convert.ToBoolean(assessmentTemplate.SignatureRequired)));


            if (assessmentTemplate.Role.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_role", Convert.ToInt32(assessmentTemplate.Role.Value)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_role", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(assessmentTemplate.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", assessmentTemplate.Description));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }


            if (string.IsNullOrEmpty(assessmentTemplate.Picture))
            {
                assessmentTemplate.Media = null;
            }
            else
            {
                assessmentTemplate.Media = new List<string> { assessmentTemplate.Picture };
            }

            if (assessmentTemplate.Media != null && assessmentTemplate.Media.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_media", assessmentTemplate.Media.ToJsonFromObject()));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_media", DBNull.Value));
            }

            return parameters;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromAssessentTemplateSkillInstruction(AssessmentTemplateSkillInstruction skillInstructionTemplate, int companyId, int userId, int skillInstructionTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            //"_id" int4, "_companyid" int4, "_userid" int4, "_assessmenttemplateid" int4, "_workinstructiontemplateid" int4, "_index" int4=0

            if (skillInstructionTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", skillInstructionTemplateId));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", skillInstructionTemplate.AssessmentTemplateId));
            parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", skillInstructionTemplate.WorkInstructionTemplateId));
            parameters.Add(new NpgsqlParameter("@_index", skillInstructionTemplate.Index));

            return parameters;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_areaManager.GetPossibleExceptions());
                listEx.AddRange(_generalManager.GetPossibleExceptions());
                listEx.AddRange(_flattenedAssessmentManager.GetPossibleExceptions());
                listEx.AddRange(_tagManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion

    }
}
