using System;
namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IItemFilter<T>
    {
        T FilterStatus { get; set; }
        string Name { get; set; }
        System.Collections.Generic.List<EZGO.Api.Models.Tags.Tag> Tags { get; set; }
    }
}
