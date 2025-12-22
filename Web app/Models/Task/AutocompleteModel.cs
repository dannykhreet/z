using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebApp.Models.Task
{
    public class AutocompleteModel
    {
        [JsonPropertyName("label")]
        public string label { get; set; }
        [JsonPropertyName("value")]
        public string value { get; set; }
    }
}
