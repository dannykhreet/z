using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    [ObsoleteAttribute("This object is obsolete. Use PropertyValueKind instead.", false)]
    public class MeasurementUnitKind : PropertyValueKind
    {
        public List<PropertyValue> MeasurementUnits {
            get {
                return this.PropertyValues;
            }
            set {
                this.PropertyValues = value;
            }
        }
    }
}
