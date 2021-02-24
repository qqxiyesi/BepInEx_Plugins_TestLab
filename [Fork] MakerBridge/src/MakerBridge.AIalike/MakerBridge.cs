using System.IO;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace KeelPlugins
{
	[BepInPlugin(GUID, PluginName, Version)]
	public class MakerBridge : BaseUnityPlugin
	{
		public const string GUID = "keelhauled.makerbridge";
		public const string PluginName = "MakerBridge";
		public const string Version = "1.0.2.1";

		internal static new ManualLogSource Logger;

		private const string DESCRIPTION_SENDCHARA = "Sends the selected character to the other open koikatu application.";
		private const string DESCRIPTION_SHOWMSG = "Show on screen messages about things the plugin is doing.";

		internal static string MakerCardPath;
		internal static string OtherCardPath;
		protected static GameObject bepinex;

		internal static ConfigEntry<KeyboardShortcut> SendChara { get; set; }
		internal static ConfigEntry<bool> ShowMessages { get; set; }

		internal static Harmony HooksInstance = null;

		private void Awake()
		{
			Logger = base.Logger;
			bepinex = gameObject;

			SendChara = Config.Bind("Keyboard shortcuts", "Send character", new KeyboardShortcut(KeyCode.B), new ConfigDescription(DESCRIPTION_SENDCHARA));
			ShowMessages = Config.Bind("General", "Show messages", true, new ConfigDescription(DESCRIPTION_SHOWMSG));

			var tempFolder = Path.GetTempPath();
			MakerCardPath = Path.Combine(tempFolder, $"{Constants.Prefix}_makerbridge1.png");
			OtherCardPath = Path.Combine(tempFolder, $"{Constants.Prefix}_makerbridge2.png");

			HooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
		}

		private void OnDestroy()
		{
			HooksInstance.UnpatchAll(HooksInstance.Id);
			HooksInstance = null;
		}

		private class Hooks
		{
			[HarmonyPrefix, HarmonyPatch(typeof(CharaCustom.CharaCustom), "Start")]
			public static void MakerEntrypoint()
			{
				bepinex.GetOrAddComponent<MakerHandler>();
			}

			[HarmonyPrefix, HarmonyPatch(typeof(CharaCustom.CharaCustom), "OnDestroy")]
			public static void MakerEnd()
			{
				Destroy(bepinex.GetComponent<MakerHandler>());
			}

			[HarmonyPrefix, HarmonyPatch(typeof(StudioScene), "Start")]
			public static void StudioEntrypoint()
			{
				bepinex.GetOrAddComponent<StudioHandler>();
			}
		}

		internal static void Log(object data)
		{
			Logger.Log(ShowMessages.Value ? BepInEx.Logging.LogLevel.Message : BepInEx.Logging.LogLevel.Info, data);
		}
	}
}