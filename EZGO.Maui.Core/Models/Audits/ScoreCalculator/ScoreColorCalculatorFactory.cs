using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Audits
{
    public static class ScoreColorCalculatorFactory
    {
        public static IScoreColorCalculator Default(int minScore, int maxScore)
        {
            return new DefaultScoreColorCalculator(minScore, maxScore);
        }
    }
}
