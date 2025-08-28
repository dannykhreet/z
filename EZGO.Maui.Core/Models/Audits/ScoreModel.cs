using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Models.Audits
{
    public class ScoreModel
    {
        public int Number{ get; set; }
        public int NumberOfScores { get; set; }
        public int MinimalScore { get; set; }

        public Color Color { get; set; }

    }
}