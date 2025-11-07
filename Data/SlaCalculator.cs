using FixItNR.Api.Models;
using System.Collections.Concurrent;


namespace FixItNR.Api.Data
{


    public static class SlaCalculator
    {
        public static int GetSlaHours(string category, string priority)
        {
            var cat = (category ?? "IT").ToLowerInvariant();
            var pr = (priority ?? "Medium").ToLowerInvariant();


            int baseHours = cat switch
            {
                "it" => 24,
                "электрика" => 48,
                "уборка" => 36,
                _ => 36
            };


            int adjust = pr switch { "low" => +12, "high" => -12, _ => 0 };
            return Math.Max(6, baseHours + adjust);
        }
    }
}