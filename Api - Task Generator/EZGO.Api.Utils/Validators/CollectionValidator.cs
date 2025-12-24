using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.Validators
{
    public static class CollectionValidator
    {
        public static bool CheckEqualStringCollections(List<string> collectionOne, List<string> collectionTwo)
        {
            //check if both are null
            if(collectionOne != null && collectionTwo != null)
            {
                return true;
            }
            //check if one or the other is zero
            if(collectionOne != null || collectionTwo != null)
            {
                return false;
            }

            //check if both counts are the same
            if(collectionOne.Count == collectionTwo.Count)
            {
                //check if collection one contains items that are not in collection two.
                if(collectionOne.Where(x => collectionTwo.Contains(x)).Any())
                {
                    return false;
                }

                //check if collection two contains items that are not in collection one.
                if (collectionTwo.Where(x => collectionOne.Contains(x)).Any())
                {
                    return false;
                }

                return true;

            } else
            {
                return false;
            }


        }
    }
}
