using System;
namespace WebApp.Models.User
{
    /// <summary>
    ///
    /// </summary>
    public class AllowedAreaModel
    {
        //TODO rename to AreaModel or something generic.
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public string FullDisplayName { get; set; }

        public AllowedAreaModel()
        {
        }
    }
}
