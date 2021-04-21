using UnityEngine;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using Specter.Assist;

using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;

namespace Specter
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("marco.kkapi")]
#if KK
	[BepInIncompatibility("ShortcutsKoi")]
	[BepInIncompatibility("shortcutsKoi.guideObjectPort")]
#endif
	[BepInIncompatibility("Specter")]
	[BepInIncompatibility("StudioExtraMoveAxis")]
	[BepInProcess(Constants.StudioProcessName)]
	public class Specter : BaseUnityPlugin
	{
		public const string Name = "Studio Common Guide (from Specter)";
		public const string GUID = "Specter.StudioCommonGuide";
		public const string Version = "1.1.0.0";

		internal static new ManualLogSource Logger;
		internal static Specter Instance;

		internal static ConfigEntry<bool> ConfigEnable { get; set; }
		internal static ConfigEntry<float> ConfigRatio { get; set; }

		private void Awake()
		{
			Logger = base.Logger;
			Instance = this;

			ConfigEnable = Config.Bind("General", "Enable", true);
			ConfigRatio = Config.Bind("General", "UI Ratio", 1f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 10f)));

			StudioAPI.StudioLoadedChanged += (sender, e) => gameObject.GetOrAddComponent<StudioCommonGuide>();

			var iconTex = TextureUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("toolbar_icon.png")); // from StudioExtraMoveAxis
			CustomToolbarButtons.AddLeftToolbarToggle(iconTex, ConfigEnable.Value, value => ConfigEnable.Value = value);
		}
	}
}
