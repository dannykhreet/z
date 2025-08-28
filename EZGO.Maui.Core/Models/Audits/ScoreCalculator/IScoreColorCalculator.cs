using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Models.Audits
{
    public interface IScoreColorCalculator
    {
        Color GetColor(int score);
    }
}
