using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// FilterAreaTypeEnum; Used for setting a filtering setup.
    /// Single = 0; Just one item, based on the supplied AreaId is queried.
    /// RecursiveRootToLeaf = 1; From the root to the leaf AreaId (through parents)
    /// RecursiveLeafToRoot = 2; All items from the leaf to the root AreaId (through children)
    /// When using for submitting to the EZGO API, always use the value (int) for posting.
    /// </summary>
    public enum FilterAreaTypeEnum
    {
        Single = 0,
        RecursiveRootToLeaf = 1,
        RecursiveLeafToRoot = 2
    }
}
