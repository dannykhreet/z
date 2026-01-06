using EZGO.Api.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Ultimo.Models
{
    /// <summary>
    /// UltimoConfig; Based on base config. Ultimo uses a specific application element id (e.g. a JOB) as extra configuration.
    /// This needs to be added to the headers with the API key.
    /// </summary>
    public class UltimoConfig : BaseConfig
    {
        public string ApplicationElementId { get; set; }
    }
}
