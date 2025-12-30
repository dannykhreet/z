using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EZGO.Api.Utils.Tools
{
    public static class Copier
    {
        /// <summary>
        /// DeepCopy; Create a copy of a NpgsqlParameter; Due to the inner workings and static binding on command normal serialization (non-serializable) and shallow copy can not be used.
        /// </summary>
        /// <param name="npgsqlParameters">Collection of items to re-attach</param>
        /// <returns>List of copied items.</returns>
        public static List<Npgsql.NpgsqlParameter> DeepCopy(List<Npgsql.NpgsqlParameter> npgsqlParameters)
        {
            var output = new List<Npgsql.NpgsqlParameter>();

            if(npgsqlParameters != null && npgsqlParameters.Count > 0)
            {
                foreach (var item in npgsqlParameters)
                {
                    output.Add(new Npgsql.NpgsqlParameter { Value = item.Value, DbType = item.DbType, ParameterName = item.ParameterName });
                }
            }
            return output;
        }
    }
}
