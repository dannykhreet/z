using EZ.Connector.Ultimo.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Ultimo.Models
{

    /// <summary>
    /// UltimoAction; Action for sending to UltimoAPI.
    /// Need to be mapped to the following structure:
    ///
    /// {
    ///     "Description": "EZGO: test action job",
    ///     "ReportText": "ReportText"
    ///     "ProcessFunctionId": "",
    ///     "EquipmentId": "",
    ///     "ReportDate": "",
    ///     "SpaceId": "",
    ///     "SiteId": "",
    ///     "Context": "",
    ///     "WorkOrderTypeId": ""
    /// }
    ///
    /// </summary>
    public class UltimoAction
    {
        /// <summary>
        /// Description: Description of a ultimo action. Based on action.description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ReportText: Report text of a ultimo action. Will be mapped to action.comment
        /// </summary>
        public string ReportText { get; set; }

        //TODO set correct formatting
        /// <summary>
        /// StatusCreatedReportDate; Date of creation for reporting purposes, will be mapped to action.createdat in the following format (ddmmyyy HHmm)
        /// </summary>

        public string StatusCreatedReportDate { get; set; }

        /// <summary>
        /// Status; Mandatory field for submitting actions to Ultimo!
        /// 1: open
        /// 2: goedgekeurd
        /// 4: actief
        /// TBA
        /// </summary>
        public int Status { get; set; } = 1;

        /// <summary>
        /// Context; Mandatory field for submitting actions to Ultimo!
        /// </summary>
        public int Context { get; set; } = 1;
        /// <summary>
        /// ExternalId; The Id of the action in EZ-GO
        /// </summary>
        public int? ExternalId { get; set; }
        public UltimoAction()
        {
        }
    }
}


