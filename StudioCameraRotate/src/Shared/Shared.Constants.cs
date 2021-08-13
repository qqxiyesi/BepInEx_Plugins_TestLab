internal static class Constants
{
#if AI
	internal const string Prefix = "AI";
	internal const string GameName = "AI Girl";
	internal const string StudioProcessName = "StudioNEOV2";
	internal const string MainGameProcessName = "AI-Syoujyo";
#elif EC
	internal const string Prefix = "EC";
	internal const string GameName = "Emotion Creators";
	internal const string MainGameProcessName = "EmotionCreators";
#elif HS2
	internal const string Prefix = "HS2";
	internal const string GameName = "Honey Select 2";
	internal const string StudioProcessName = "StudioNEOV2";
	internal const string MainGameProcessName = "HoneySelect2";
	internal const string VRProcessName = "HoneySelect2VR";
#elif KK
	internal const string Prefix = "KK";
	internal const string GameName = "Koikatsu";
	internal const string StudioProcessName = "CharaStudio";
	internal const string MainGameProcessName = "Koikatu";
	internal const string MainGameProcessNameSteam = "Koikatsu Party";
	internal const string VRProcessName = "KoikatuVR";
	internal const string VRProcessNameSteam = "Koikatsu Party VR";
#elif KKS
	internal const string Prefix = "KKS";
	internal const string GameName = "Koikatsu Sunshine";
	internal const string MainGameProcessName = "KoikatsuSunshineTrial";
#endif
}
