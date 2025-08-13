using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.ModelInterfaces
{
    public interface IBase<T>
    {
        T ToBasic();
    }
}
