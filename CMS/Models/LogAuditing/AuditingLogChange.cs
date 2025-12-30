using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.LogAuditing
{
    public class AuditingLogChange
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> AddedValues { get; set; }
        public List<string> RemovedValues { get; set; }

        public AuditingLogChange() {
            AddedValues = new List<string>();
            RemovedValues = new List<string>();
        }

        //backup
        //public string PropertyName { get; set; }
        //public string OldValue { get; set; }
        //public string NewValue { get; set; }
        //public ChangeType Type
        //{
        //    get
        //    {
        //        if (OldValue == null)
        //        {
        //            OldValue = "";
        //        }
        //        if (NewValue == null)
        //        {
        //            NewValue = "";
        //        }
        //        if (NewValue.Equals(OldValue))
        //            return ChangeType.Nothing;
        //        else if (OldValue.Equals(""))
        //            return ChangeType.Add;
        //        else if (NewValue.Equals(""))
        //            return ChangeType.Remove;
        //        else
        //            return ChangeType.Edit;
        //    }
        //}
        //public enum ChangeType
        //{
        //    Edit,
        //    Add,
        //    Remove,
        //    Nothing
        //}
    }
}
