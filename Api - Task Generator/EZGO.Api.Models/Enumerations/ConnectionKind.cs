using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Data.Enumerations
{
    /// <summary>
    /// Can be used as a toggle with certain calls if needed to force a execution (scalar, nonquery, reader, cmd etc) to use a specific kind of connection.
    /// </summary>
    public enum ConnectionKind
    {
        Writer,
        Reader

    }
}
