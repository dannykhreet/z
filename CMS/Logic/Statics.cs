using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Logic.Services;

namespace WebApp.Logic
{
    public static class Statics
    {
        public static Dictionary<string, Dictionary<string, string>> Languages;

        public static List<string> AvailableLanguages;

        public static string EnvironmentName { get; set; }
    }

    public static class AppUrlsResolver
    {
        public static string Video(string videoPath)
        {
            if (!string.IsNullOrEmpty(videoPath) && videoPath.StartsWith("http"))
            {
                return videoPath;
            }

            return $"{Constants.Action.VideoBaseUrl}{videoPath}";
        }
    }

    public static class ScoreColorCalculatorFactory
    {
        public static IScoreColorCalculator Default(int minScore, int maxScore)
        {
            return new DefaultScoreColorCalculator(minScore, maxScore);
        }
    }
}