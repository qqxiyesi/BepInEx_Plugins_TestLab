using BepInEx;
using HarmonyLib;
using ParadoxNotion.Serialization;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DefaultParamEditor.Koikatu
{
	[BepInProcess(Constants.StudioProcessName)]
	[BepInPlugin(GUID, PluginName, Version)]
	public class DefaultParamEditor : BaseUnityPlugin
	{
		public const string GUID = "keelhauled.defaultparameditor";
		public const string PluginName = "DefaultParamEditor";
		public const string Version = "1.1.1.1";

		private static string savePath = Path.Combine(Paths.ConfigPath, "DefaultParamEditorData.json");
		private static ParamData data = new ParamData();

		private void Awake()
		{
			Log.SetLogSource(Logger);
			Harmony.CreateAndPatchAll(typeof(Hooks));

			if (File.Exists(savePath))
			{
				try
				{
					var json = File.ReadAllText(savePath);
					data = JSONSerializer.Deserialize<ParamData>(json);
				}
				catch (Exception ex)
				{
					Log.Error($"Failed to load settings from {savePath} with error: " + ex);
					data = new ParamData();
				}
			}

			CharacterParam.Init(data.charaParamData);
			SceneParam.Init(data.sceneParamData);
		}

		private static void SaveToFile()
		{
			var json = JSONSerializer.Serialize(data.GetType(), data, true);
			File.WriteAllText(savePath, json);
		}

		private class Hooks
		{
			[HarmonyPostfix, HarmonyPatch(typeof(Studio.Studio), nameof(Studio.Studio.Init))]
			public static void CreateUI()
			{
				var mainlist = SetupList("StudioScene/Canvas Main Menu/04_System");
				CreateMainButton("tglDPEsl", "Load scene param", mainlist, SceneParam.LoadDefaults);
				CreateMainButton("tglDPEss", "Save scene param", mainlist, () =>
				{
					SceneParam.Save();
					SaveToFile();
				});

				var charalist = SetupList("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root");
				CreateCharaButton("tglDPEcs", "Save chara param", charalist, () =>
				{
					CharacterParam.Save();
					SaveToFile();
				});
			}

			private static ScrollRect SetupList(string goPath)
			{
				var listObject = GameObject.Find(goPath);
				var scrollRect = listObject.GetComponent<ScrollRect>();
				scrollRect.content.gameObject.GetOrAddComponent<VerticalLayoutGroup>();
				scrollRect.content.gameObject.GetOrAddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
				scrollRect.scrollSensitivity = 25;

				foreach (Transform item in scrollRect.content.transform)
				{
					var layoutElement = item.gameObject.GetOrAddComponent<LayoutElement>();
					layoutElement.preferredHeight = 40;
				}

				return scrollRect;
			}

			private static Button CreateMainButton(string name, string label, ScrollRect scrollRect, UnityAction onClickEvent)
			{
				return CreateButton(name, label, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/04_System/Viewport/Content/End");
			}

			private static Button CreateCharaButton(string name, string label, ScrollRect scrollRect, UnityAction onClickEvent)
			{
				return CreateButton(name, label, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root/Viewport/Content/State");
			}

			private static Button CreateButton(string name, string label, ScrollRect scrollRect, UnityAction onClickEvent, string goPath)
			{
				var template = GameObject.Find(goPath);
				var newObject = Instantiate(template, scrollRect.content.transform);
				newObject.name = name;
				var textComponent = newObject.GetComponentInChildren<Text>();
				textComponent.text = label;
				var buttonComponent = newObject.GetComponent<Button>();
				for (int i = 0; i < buttonComponent.onClick.GetPersistentEventCount(); i++)
					buttonComponent.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
				buttonComponent.onClick.ActuallyRemoveAllListeners();
				buttonComponent.onClick.AddListener(onClickEvent);
				return buttonComponent;
			}
		}
	}
}
