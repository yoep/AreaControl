namespace AreaControl
{
    public static class AreaControl
    {
        public const string Name = "Area Control";

        public const string ActionCloseRoad = "Close road";
        public const string ActionOpenRoad = "Open road";
        public const string ActionCloseRoadPreview = "Close road preview " + DebugIndicatorText;
        public const string ActionRemoveCloseRoadPreview = "Remove close road preview " + DebugIndicatorText;

        public const string RoadInfo = "Road info " + DebugIndicatorText;
        public const string RoadPreview = "Road preview " + DebugIndicatorText;
        public const string RoadPreviewRemove = "Remove road preview " + DebugIndicatorText;

        public const string RedirectTraffic = "Redirect traffic";
        public const string RedirectTrafficDescription = "Distance to place the redirect traffic";
        public const string RedirectTrafficRemove = "Stop redirecting traffic";
        public const string RedirectTrafficPreview = "Redirect traffic preview " + DebugIndicatorText;
        public const string RedirectTrafficPreviewRemove = "Remove redirect traffic preview " + DebugIndicatorText;

        private const string DebugIndicatorText = "[DEBUG]";
    }
}