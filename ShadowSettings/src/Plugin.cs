using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Studio.SaveLoad;

namespace ShadowSettings
{
	[BepInProcess("CharaStudio")]
	[BepInPlugin(GUID, PluginName, Version)]
	public partial class ShadowSettings : BaseUnityPlugin
	{
		public const string GUID = "ShadowSettings";
		public const string PluginName = "Shadow Settings";
		public const string Version = "1.0.1.0";

		internal static ConfigEntry<ShadowQuality> ShadowQuality { get; set; }
		internal static ConfigEntry<ShadowResolution> ShadowResolution { get; set; }
		internal static ConfigEntry<ShadowProjection> ShadowProjection { get; set; }
		internal static ConfigEntry<int> ShadowCascades { get; set; }
		internal static ConfigEntry<float> ShadowDistance { get; set; }
		internal static ConfigEntry<float> ShadowNearPlaneOffset { get; set; }

		internal static Dropdown uiShadowQuality;
		internal static Dropdown uiShadowResolution;
		internal static Dropdown uiShadowProjection;
		internal static Dropdown uiShadowCascades;
		internal static Slider uiShadowDistance;
		internal static Slider uiShadowNearPlaneOffset;

		internal static new ManualLogSource Logger;
		internal static ShadowSettings Instance;
		internal static Harmony HooksInstance = null;

		internal static List<string> listShadowCascadesOptions = new List<string>() { "0", "2", "4" };

		private void Start()
		{
			Logger = base.Logger;
			Instance = this;

			ShadowQuality = Config.Bind("General", "Shadow quality", UnityEngine.ShadowQuality.All, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 5 }));
			ShadowResolution = Config.Bind("General", "Shadow resolution", UnityEngine.ShadowResolution.VeryHigh, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 4 }));
			ShadowProjection = Config.Bind("General", "Shadow projection", UnityEngine.ShadowProjection.CloseFit, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
			ShadowCascades = Config.Bind("General", "Shadow cascades", 4, new ConfigDescription("", new AcceptableValueList<int>(0, 2, 4), new ConfigurationManagerAttributes { Order = 2 }));
			ShadowDistance = Config.Bind("General", "Shadow distance", 50f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f), new ConfigurationManagerAttributes { Order = 1 }));
			ShadowNearPlaneOffset = Config.Bind("General", "Shadow near plane offset", 2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 4f), new ConfigurationManagerAttributes { Order = 0 }));

			InitSetting(ShadowQuality, () => QualitySettings.shadows = ShadowQuality.Value);
			InitSetting(ShadowResolution, () => QualitySettings.shadowResolution = ShadowResolution.Value);
			InitSetting(ShadowProjection, () => QualitySettings.shadowProjection = ShadowProjection.Value);
			InitSetting(ShadowCascades, () => QualitySettings.shadowCascades = ShadowCascades.Value);
			InitSetting(ShadowDistance, () => QualitySettings.shadowDistance = ShadowDistance.Value);
			InitSetting(ShadowNearPlaneOffset, () => QualitySettings.shadowNearPlaneOffset = ShadowNearPlaneOffset.Value);

			StudioSaveLoadApi.RegisterExtraBehaviour<ShadowSettingsController>(GUID);
		}

		internal void InitSetting<T>(ConfigEntry<T> configEntry, Action setter)
		{
			setter();
			configEntry.SettingChanged += (sender, args) => setter();
		}

		internal sealed class ConfigurationManagerAttributes
		{
			public int? Order;
		}
	}
}
