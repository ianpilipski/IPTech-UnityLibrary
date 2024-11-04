namespace IPTech.Platform {
    public struct AdImpressionParameters {
        public AdCompletionStatus AdCompletionStatus;
        public AdProvider AdProvider;
        public string PlacementID;
        public string PlacementName;
        public AdPlacementType? PlacementType;
        public double? AdEcpmUsd;
        public string SdkVersion;
        public string AdImpressionID;
        public string AdStoreDstID;
        public string AdMediaType;
        public long? AdTimeWatchedMs;
        public long? AdTimeCloseButtonShownMs;
        public long? AdLengthMs;
        public bool? AdHasClicked;
        public string AdSource;
        public string AdStatusCallback;
    }
}