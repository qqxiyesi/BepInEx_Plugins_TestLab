using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityStandardAssets.ImageEffects;
using ParadoxNotion.Serialization;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;

namespace StudioEffectSettings
{
	[BepInProcess("CharaStudio")]
	[BepInPlugin(GUID, PluginName, Version)]
	public partial class StudioEffectSettings : BaseUnityPlugin
	{
		public const string GUID = "StudioEffectSettings";
		public const string PluginName = "Studio Effect Settings";
		public const string Version = "1.0.0.0";

		internal static ConfigEntry<bool> _cfgEnableDataLoad;

		string _filePath = Path.Combine(Paths.ConfigPath, "StudioEffectSettings.json");
		internal static List<Type> _listEffectTypes = new List<Type>()
		{
			typeof(GlobalFog),
			typeof(AmplifyOcclusionEffect),
			typeof(BloomAndFlares),
			typeof(AmplifyColorEffect),
			typeof(SunShafts),
			typeof(VignetteAndChromaticAberration),
			typeof(DepthOfField),
		};

		private void Awake()
		{
			_cfgEnableDataLoad = Config.Bind("Debug", "Enable Data Load", true, "Enable loading the plugin data from SD");
		}

		private void Start()
		{
			StudioAPI.StudioLoadedChanged += (sender, e) => RegisterStudioControls();

			StudioSaveLoadApi.RegisterExtraBehaviour<StudioEffectSettingsController>(GUID);
		}

		public class EffectSettings
		{
			public Dictionary<Type, Dictionary<string, float>> FloatFields = new Dictionary<Type, Dictionary<string, float>>();
			public Dictionary<Type, Dictionary<string, bool>> BoolFields = new Dictionary<Type, Dictionary<string, bool>>();
			public EffectSettings(Dictionary<Type, Dictionary<string, float>> _floats, Dictionary<Type, Dictionary<string, bool>> _bools)
			{
				FloatFields = _floats;
				BoolFields = _bools;
			}
		}

		private void RegisterStudioControls()
		{
			if (!File.Exists(_filePath))
				SaveDefault();

			ScrollRect mainlist = SetupList("StudioScene/Canvas Main Menu/04_System");
			CreateMainButton("tglSESload", "Load Effect Def", mainlist, () =>
			{
				if (!File.Exists(_filePath))
				{
					Logger.LogMessage("StudioEffectSettings preset doesn't exist");
					return;
				}
				LoadDefault();
				Logger.LogMessage("StudioEffectSettings preset loaded");
			});

			CreateMainButton("tglSESsave", "Save Effect Def", mainlist, () =>
			{
				SaveDefault();
				Logger.LogMessage("StudioEffectSettings preset saved");
			});
		}

		private void LoadDefault()
		{
			EffectSettings _data = JSONSerializer.Deserialize<EffectSettings>(File.ReadAllText(_filePath));
			Dictionary<Type, Dictionary<string, float>> _floatFields = _data.FloatFields;
			Dictionary<Type, Dictionary<string, bool>> _boolFields = _data.BoolFields;

			foreach (KeyValuePair<Type, Dictionary<string, float>> x in _floatFields)
			{
				Component _comp = Camera.main.GetComponent(x.Key);
				Traverse _traverse = Traverse.Create(_comp);
				foreach (KeyValuePair<string, float> y in x.Value)
					_traverse.Field<float>(y.Key).Value = y.Value;
			}

			foreach (KeyValuePair<Type, Dictionary<string, bool>> x in _boolFields)
			{
				Component _comp = Camera.main.GetComponent(x.Key);
				Traverse _traverse = Traverse.Create(_comp);
				foreach (KeyValuePair<string, bool> y in x.Value)
					_traverse.Field<bool>(y.Key).Value = y.Value;
			}
		}

		private void SaveDefault()
		{
			Dictionary<Type, Dictionary<string, float>> _floatFields = new Dictionary<Type, Dictionary<string, float>>();
			Dictionary<Type, Dictionary<string, bool>> _boolFields = new Dictionary<Type, Dictionary<string, bool>>();

			foreach (Type _type in _listEffectTypes)
			{
				Component _comp = Camera.main.GetComponent(_type);

				_floatFields[_type] = new Dictionary<string, float>();
				_boolFields[_type] = new Dictionary<string, bool>();

				foreach (FieldInfo x in _comp.GetType().GetFields())
				{
					if (x.FieldType == typeof(float))
						_floatFields[_type][x.Name] = (float)x.GetValue(_comp);
					else if (x.FieldType == typeof(bool))
						_boolFields[_type][x.Name] = (bool)x.GetValue(_comp);
				}
			}

			EffectSettings _data = new EffectSettings(_floatFields, _boolFields);
			string _json = JSONSerializer.Serialize(typeof(EffectSettings), _data, true);
			File.WriteAllText(_filePath, _json);
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
