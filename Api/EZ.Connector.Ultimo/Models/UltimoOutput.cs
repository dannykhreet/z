using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Ultimo.Models
{
    /// <summary>
    /// UltimoOutput; Ultimo output object. NOTE! casing must be the same as output. DO NOT CHANGE.
    /// Possible outputs of api:
    ///  /* success output
    ///     {
    ///        "properties": {
    ///             "JobId": "0005446"
    ///        }
    ///     }
    ///
    ///     other output:
    ///     {
    ///        "message": "Job cannot be created...",
    ///        "type": 3,
    ///        "code": "3399"
    ///     }
    ///
    ///     other output:
    ///     {
    ///       "message": "Missing API key",
    ///       "code": "MissingApiKey"
    ///     }
    ///   */
    /// </summary>
    public class UltimoOutput
    {
        public string message { get; set; }
        public int? type { get; set; }
        public string code { get; set; }
        public List<UltimoOutputPropertyItem> properties { get; set; }
    }

    /// <summary>
    /// UltimoOutputPropertyItem; property item.
    /// </summary>
    public class UltimoOutputPropertyItem
    {
        public string JobId { get; set; }
    }



}
