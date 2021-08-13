using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Studio;
#if !KK
using TMPro;
#endif

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace StudioCharaReload
{
#if KK
	[BepInProcess("CharaStudio")]
#else
	[BepInProcess("StudioNEOV2")]
#endif
	[BepInPlugin(GUID, PluginName, Version)]
	public partial class StudioCharaReload : BaseUnityPlugin
	{
		public const string GUID = "StudioCharaReload";
		public const string PluginName = "Studio Chara Reload";
		public const string Version = "1.0.3.0";

		private static new ManualLogSource Logger;
		private static bool Loaded = false;

		private static ConfigEntry<KeyboardShortcut> CfgShortcut { get; set; }

		private void Awake()
		{
			Logger = base.Logger;

			CfgShortcut = Config.Bind("Config", "Keyboard Shortcut", new KeyboardShortcut(KeyCode.None));

			SceneManager.sceneLoaded += SceneLoaded;
		}

		private void Update()
		{
			if (CfgShortcut.Value.IsDown())
			{
				ReloadChara();
			}
		}

		private static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (!Loaded && scene.name == "Studio")
			{
				Loaded = true;
				SceneManager.sceneLoaded -= SceneLoaded;

				RegisterStudioControls();
			}
		}

		internal static void RegisterStudioControls()
		{
			ScrollRect charalist = SetupList("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root");

			CreateCharaButton("tglReload", "Reload", charalist, () => ReloadChara());
		}

		internal static void ReloadChara()
		{
			IEnumerable<OCIChar> _OCIChar = GetSelectedCharacters();

			int i = 0;
			foreach (OCIChar CurOCIChar in _OCIChar)
			{
				string CardPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Paths.ExecutablePath) + "_Reload_" + i + ".png");
				if (File.Exists(CardPath))
					File.Delete(CardPath);
				using (FileStream fileStream = new FileStream(CardPath, FileMode.Create, FileAccess.Write))
					CurOCIChar.charInfo.chaFile.SaveCharaFile(fileStream, true);
				CurOCIChar.ChangeChara(CardPath);
				File.Delete(CardPath);
				i++;
			}
		}

		// codes from KKAPI

		internal static IEnumerable<OCIChar> GetSelectedCharacters()
		{
			return GetSelectedObjects().OfType<OCIChar>();
		}

		internal static IEnumerable<ObjectCtrlInfo> GetSelectedObjects()
		{
			if (!Loaded)
				yield break;

			TreeNodeObject[] selectNodes = Studio.Studio.Instance.treeNodeCtrl.selectNodes;
			for (int i = 0; i < selectNodes.Length; i++)
				if (Studio.Studio.Instance.dicInfo.TryGetValue(selectNodes[i], out ObjectCtrlInfo objectCtrlInfo))
					yield return objectCtrlInfo;
		}
	}
}
