using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Properties
{
    public class PropertyValueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueSymbol { get; set; }
        public string ValueAbbreviation { get; set; }
        public string ResourceKeyname { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int PropertyValueKindId { get; set; }
        public PropertyValueTypeEnum DefaultValueType { get; set; }
    }
}