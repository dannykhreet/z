using System;
namespace WebApp.Models.Properties
{
    public class PropertyModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public int FieldType { get; set; }
        public int FieldKindType { get; set; }
        public int ValueType { get; set; }
        public int DisplayValueType { get; set; }

    }
}
