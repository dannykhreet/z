using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Models.LogAuditing
{
    public class AuditingLogCollectionChange
    {
        public string CollectionName { get; set; }
        public List<string> SameObjects { get; set; }
        public List<string> RemovedObjects { get; set; }
        public List<string> AddedObjects { get; set; }

        //public List<JToken> RemovedObjects
        //{
        //    get
        //    {
        //        return OldCollection.Where(oldObject => !NewCollection.Contains(oldObject)).ToList();
        //    }
        //}
        //public List<JToken> AddedObjects
        //{
        //    get
        //    {
        //        return NewCollection.Where(newObject => !OldCollection.Contains(newObject)).ToList();
        //    }
        //}
        //public List<JToken> SameObjects
        //{
        //    get
        //    {
        //        return NewCollection.Where(newObject => OldCollection.Contains(newObject)).ToList();
        //    }
        //}

    }
}
