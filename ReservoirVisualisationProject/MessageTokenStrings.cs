using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservoirVisualisationProject
{
    public static class MessageTokenStrings
    {
        public static string AddPathToManager { get; } = "Path is Parsed";
        public static string AddStagesToManager { get; } = "Stages are Parsed";
        public static string AddDataToManager { get; } = "Data is Parsed";

        public static string AddPathToViewport { get; } = "Add Path to Viewport";
        public static string AddDataToViewport { get; } = "Add Data to Viewport";
        public static string RemoveWellFromViewport { get; } = "Remove Well from Viewport";

        public static string SetupTimeline { get; } = "Set-up Timeline";
        public static string SelectDataByTimestamp { get; } = "Select Data by Timestamp";
    }
}
