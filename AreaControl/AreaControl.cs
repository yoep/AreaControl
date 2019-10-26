namespace AreaControl
{
    public static class AreaControl
    {
        public const string Name = "Area Control";

        public const string ActionCloseRoad = "Close road";
        public const string ActionCloseRoadDescription = "Close the nearby road";
        public const string ActionOpenRoad = "Open road";
        public const string ActionCloseRoadPreview = "Close road preview " + DebugIndicatorText;
        public const string ActionRemoveCloseRoadPreview = "Remove close road preview " + DebugIndicatorText;

        public const string RoadInfo = "Road info " + DebugIndicatorText;
        public const string RoadPreview = "Road preview " + DebugIndicatorText;
        public const string RoadPreviewRemove = "Remove road preview " + DebugIndicatorText;
        public const string NearbyRoadsPreview = "Nearby roads preview " + DebugIndicatorText;
        public const string NearbyRoadsPreviewRemove = "Remove nearby roads preview " + DebugIndicatorText;

        public const string RedirectTraffic = "Redirect traffic";
        public const string RedirectTrafficDescription = "Redirect the traffic away from the player";
        public const string RedirectTrafficRemove = "Stop redirecting traffic";
        public const string RedirectTrafficPreview = "Redirect traffic preview " + DebugIndicatorText;
        public const string RedirectTrafficPreviewRemove = "Remove redirect traffic preview " + DebugIndicatorText;
        
        public const string ClearArea = "Clear surrounding area";
        public const string ClearAreaDescription = "Clear the surrounding area fo death bodies and wrecks";
        public const string CreateCrimeScene = "Create crime scene";
        public const string RemoveCrimeScene = "Remove crime scene";
        public const string CrimeScenePreview = "Crime scene preview " + DebugIndicatorText;
        public const string CrimeScenePreviewRemove = "Remove crime scene preview " + DebugIndicatorText;
        public const string CrimeSceneDescription = "Create a crime scene in the nearby area";
        
        public const string SlowDownTraffic = "Slow down traffic";

        private const string DebugIndicatorText = "[DEBUG]";
    }
}