using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Interfaces.Utils
{
    //TODO move interface to utils.
    public interface ICryptography
    {
        string Encrypt(string unprotectedValue);
        string Decrypt(string protectedValue);
        string GetRandomSalt();
        string GetHash();

    }
}
