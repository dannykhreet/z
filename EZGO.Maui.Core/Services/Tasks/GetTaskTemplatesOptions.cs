using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Services.Tasks
{
    /// <summary>
    /// Options to configure when getting <see cref="EZGO.Api.Models.TaskTemplate"/>
    /// </summary>
    public class GetTaskTemplatesOptions
    {
        #region Public Properties

        /// <summary>
        /// Id of the area filter the templates
        /// </summary>
        public int? AreaId { get; private set; }

        /// <summary>
        /// Type of filter for the area to use
        /// </summary>
        public FilterAreaTypeEnum? AreaFilterType { get; private set; }

        /// <summary>
        /// Role based filter
        /// </summary>
        public RoleTypeEnum? RoleType { get; private set; }

        /// <summary>
        /// Task type filter
        /// </summary>
        public TaskTypeEnum? TaskType { get; private set; }

        /// <summary>
        /// Maximum number of items to fetch
        /// </summary>
        /// <value>
        /// <c>0</c> - unlimited
        /// </value>
        public int? Limit { get; private set; }

        /// <summary>
        /// Additional relational objects to include
        /// </summary>
        public IEnumerable<IncludesEnum> Include { get; private set; }

        #endregion

        #region Constructors 

        /// <summary>
        /// Creates default options
        /// </summary>
        public GetTaskTemplatesOptions()
        { }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Filters data to the given area
        /// </summary>
        /// <param name="id">Id of the area</param>
        /// <param name="areaFilter">Type of filter to use. If none specified <see cref="FilterAreaTypeEnum.Single"/> is used</param>
        /// <returns>Current instance of the options for chaining</returns>
        public GetTaskTemplatesOptions FromArea(int id, FilterAreaTypeEnum areaFilter = FilterAreaTypeEnum.Single)
        {
            AreaId = id;
            WithAreaFilterType(areaFilter);
            return this;
        }

        /// <summary>
        /// Sets the area filter method
        /// </summary>
        /// <param name="areaFilter">Type of filter to use</param>
        /// <returns>Current instance of the options for chaining</returns>
        public GetTaskTemplatesOptions WithAreaFilterType(FilterAreaTypeEnum areaFilter)
        {
            AreaFilterType = areaFilter;
            return this;
        }

        /// <summary>
        /// Sets the role filter 
        /// </summary>
        /// <param name="roleType">Value for the role filter</param>
        /// <returns>Current instance of the options for chaining</returns>
        public GetTaskTemplatesOptions WithRole(RoleTypeEnum roleType)
        {
            RoleType = roleType;
            return this;
        }

        /// <summary>
        /// Sets the task type filter
        /// </summary>
        /// <param name="taskType">Value for the task type filter</param>
        /// <returns>Current instance of the options for chaining</returns>
        public GetTaskTemplatesOptions OnlyTaskType(TaskTypeEnum taskType)
        {
            TaskType = taskType;
            return this;
        }

        /// <summary>
        /// Sets the limit option
        /// </summary>
        /// <param name="limit">Maximum number of items that will be fetched. If set to <c>0</c> the entire dataset is returned</param>
        /// <returns>Current instance of the options for chaining</returns>
        public GetTaskTemplatesOptions WithLimit(int limit)
        {
            Limit = limit;
            return this;
        }

        /// <summary>
        /// Sets what relational objects to include
        /// </summary>
        /// <param name="include">The objects to include</param>
        /// <returns>Current instance of the options for chaining</returns>
        public GetTaskTemplatesOptions Includes(params IncludesEnum[] include)
        {
            if (include.Length > 0)
                Include = include;
            return this;
        } 

        #endregion
    }
}
