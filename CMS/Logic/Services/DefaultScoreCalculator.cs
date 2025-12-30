using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.Logic.Services
{
    public class DefaultScoreColorCalculator : IScoreColorCalculator
    {
        private IReadOnlyDictionary<int, string> bounds;

        public int MinScore { get; set; }
        public int MaxScore { get; set; }

        public DefaultScoreColorCalculator(int minScore, int maxScore)
        {
            Init(minScore: minScore, maxScore: maxScore);
        }

        public DefaultScoreColorCalculator()
        {
        }

        public string GetColor(int score)
        {
            string result = bounds.First().Value;
            foreach (var bound in bounds)
            {
                if (score >= bound.Key)
                    result = bound.Value;
                else
                    break;
            }

            return result;
        }

        public string GetColor(int score, int minScore, int maxScore)
        {
            Init(minScore, maxScore);

            string result = bounds.First().Value;
            foreach (var bound in bounds)
            {
                if (score >= bound.Key)
                    result = bound.Value;
                else
                    break;
            }

            return result;
        }

        private bool Init(int minScore, int maxScore)
        {
            MinScore = minScore;
            MaxScore = maxScore;

            if (minScore >= maxScore)
                throw new ArgumentException($"\"{nameof(minScore)}\" must be greater than \"{nameof(maxScore)}\"");

            var red = "bg-red";
            var yellow = "bg-warning";
            var green = "bg-green";
            var newBounds = new Dictionary<int, string>();

            // If there's only space for two colors
            if (MaxScore - MinScore == 1)
            {
                // Set only these two colors
                newBounds.Add(MinScore, red);
                newBounds.Add(MaxScore, green);
            }
            // There's space for three colors
            else
            {
                // 2,3,4
                // 3, 
                // lower = 2
                // middle = 2 + 1 = 3
                // upper = 3 + 1 = 4


                // [0, 1, 2, 3], [4, 5, 6, 7], [8, 9, 10]
                // [0, 1, 2, 3], [4, 5, 6, 7], 8, 9, 10]
                // 11
                // lower = 0;
                // middle = 3 + 3 = 4
                // upper = 4 + 4 = 8

                // [3], [4, 5, 6], [7]
                // 
                // 5
                // lower = 3
                // middle = 3 + 1 = 4
                // upper = 4 + 2 = 7
                var numberOfPossibleScores = MaxScore - MinScore + 1;
                var lower = MinScore;
                var middle = lower + (int)Math.Floor(numberOfPossibleScores / 3d);
                var upper = middle + (int)Math.Ceiling(numberOfPossibleScores / 3d);
                newBounds.Add(lower, red);
                newBounds.Add(middle, yellow);
                newBounds.Add(upper, green);
            }

            bounds = newBounds;
            return true;
        }
    }
}