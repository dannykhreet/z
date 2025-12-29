using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Logic.Interfaces
{
    public interface IScoreColorCalculator
    {
        string GetColor(int score);

        string GetColor(int score, int minScore, int maxScore);
    }
}
