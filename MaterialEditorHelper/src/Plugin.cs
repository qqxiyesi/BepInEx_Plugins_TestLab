using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UniRx;
using ParadoxNotion.Serialization;
using ChaCustom;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

using KK_Plugins;
using KK_Plugins.MaterialEditor;
using static KK_Plugins.MaterialEditor.MaterialEditorCharaController;

namespace MaterialEditorHelper
{
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("com.deathweasel.bepinex.materialeditor", "3.1.4")]
	public partial class MaterialEditorHelper : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.meh";
		public const string PluginName = "Material Editor Helper";
		public const string Version = "1.3.0.0";

		internal static new ManualLogSource Logger;
		internal static MaterialEditorHelper Instance;

		internal static string SavePath = "";

		internal static MakerDropdown ddList;
		internal static List<string> ddListLabel = new List<string>() { "Renderer", "Shader", "Float Property", "Color Property", "Texture Property", "Copy" };
		internal static List<string> ddListNames = new List<string>() { "RendererPropertyList", "MaterialShaderList", "MaterialFloatPropertyList", "MaterialColorPropertyList", "MaterialTexturePropertyList", "MaterialCopyList" };

		internal static ConfigEntry<int> CfgDropdown { get; set; }

		private void Awake()
		{
			Logger = base.Logger;
			Instance = this;

			SavePath = Path.Combine(Paths.GameRootPath, "Temp");
		}

		private void Start()
		{
			CfgDropdown = Config.Bind("General", "Dropdown", 0, new ConfigDescription("", null, new ConfigurationManagerAttributes { Browsable = false }));

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				ChaControl chaCtrl = MakerAPI.GetCharacterControl();
				MaterialEditorCharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<MaterialEditorCharaController>();

				MakerCategory category = new MakerCategory("05_ParameterTop", "tglMEHelper", MakerConstants.Parameter.Attribute.Position + 1, "MEH");
				ev.AddSubCategory(category);

				ddList = ev.AddControl(new MakerDropdown("List", ddListLabel.ToArray(), category, CfgDropdown.Value, this));
				ddList.ValueChanged.Subscribe(value => CfgDropdown.Value = value);

				ev.AddControl(new MakerButton("Print", category, Instance)).OnClick.AddListener(delegate
				{
					object data = null;
					if (ddList.Value == 0)
						data = FetchList<RendererProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 1)
						data = FetchList<MaterialShader>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 2)
						data = FetchList<MaterialFloatProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 3)
						data = FetchList<MaterialColorProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 4)
						data = FetchList<MaterialTextureProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 5)
						data = FetchList<MaterialCopy>(pluginCtrl, ddListNames[ddList.Value]);
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					Logger.LogInfo($"{ddListNames[ddList.Value]}\n{json}");
				});
				ev.AddControl(new MakerButton($"Export", category, Instance)).OnClick.AddListener(delegate
				{
					if (!Directory.Exists(SavePath))
						Directory.CreateDirectory(SavePath);
					string ExportFilePath = Path.Combine(SavePath, $"{ddListNames[ddList.Value]}.json");
					object data = null;
					if (ddList.Value == 0)
						data = FetchList<RendererProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 1)
						data = FetchList<MaterialShader>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 2)
						data = FetchList<MaterialFloatProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 3)
						data = FetchList<MaterialColorProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 4)
						data = FetchList<MaterialTextureProperty>(pluginCtrl, ddListNames[ddList.Value]);
					else if (ddList.Value == 5)
						data = FetchList<MaterialCopy>(pluginCtrl, ddListNames[ddList.Value]);
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					File.WriteAllText(ExportFilePath, json);
					Logger.LogMessage($"{ddListNames[ddList.Value]} export to {ExportFilePath}");
				});
				ev.AddControl(new MakerButton($"Reset", category, Instance)).OnClick.AddListener(delegate
				{
					Traverse.Create(pluginCtrl).Field(ddListNames[ddList.Value]).Method("Clear").GetValue();
					Logger.LogMessage($"{ddListNames[ddList.Value]} reset");
				});
				ev.AddControl(new MakerButton("Import", category, Instance)).OnClick.AddListener(delegate
				{
					string ExportFilePath = Path.Combine(SavePath, $"{ddListNames[ddList.Value]}.json");
					object data = null;
					if (ddList.Value == 0)
						data = JSONSerializer.Deserialize<List<RendererProperty>>(File.ReadAllText(ExportFilePath));
					else if (ddList.Value == 1)
						data = JSONSerializer.Deserialize<List<MaterialShader>>(File.ReadAllText(ExportFilePath));
					else if (ddList.Value == 2)
						data = JSONSerializer.Deserialize<List<MaterialFloatProperty>>(File.ReadAllText(ExportFilePath));
					else if (ddList.Value == 3)
						data = JSONSerializer.Deserialize<List<MaterialColorProperty>>(File.ReadAllText(ExportFilePath));
					else if (ddList.Value == 4)
						data = JSONSerializer.Deserialize<List<MaterialTextureProperty>>(File.ReadAllText(ExportFilePath));
					else if (ddList.Value == 5)
						data = JSONSerializer.Deserialize<List<MaterialCopy>>(File.ReadAllText(ExportFilePath));
					Traverse.Create(pluginCtrl).Field(ddListNames[ddList.Value]).Method("AddRange", new object[] { data }).GetValue();
					Logger.LogMessage($"{ddListNames[ddList.Value]} import from {ExportFilePath}");
				});

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerText("All Settings", category, this));

				ev.AddControl(new MakerButton("Export", category, Instance)).OnClick.AddListener(delegate
				{
					string ExportFilePath = Path.Combine(SavePath, "MaterialEditorHelper.json");
					List<ObjectSetting> data = new List<ObjectSetting>();
					foreach (var item in FetchList<RendererProperty>(pluginCtrl, ddListNames[0]))
					{
						ObjectSetting ObjectSetting = data.Where(x => x.ObjectType == item.ObjectType && x.CoordinateIndex == item.CoordinateIndex && x.Slot == item.Slot).FirstOrDefault();
						if (ObjectSetting == null)
						{
							ObjectSetting = new ObjectSetting(item.ObjectType, item.CoordinateIndex, item.Slot);
							data.Add(ObjectSetting);
						}
						ObjectSetting.RendererPropertyList.Add(item);
					}
					foreach (var item in FetchList<MaterialShader>(pluginCtrl, ddListNames[1]))
					{
						ObjectSetting ObjectSetting = data.Where(x => x.ObjectType == item.ObjectType && x.CoordinateIndex == item.CoordinateIndex && x.Slot == item.Slot).FirstOrDefault();
						if (ObjectSetting == null)
						{
							ObjectSetting = new ObjectSetting(item.ObjectType, item.CoordinateIndex, item.Slot);
							data.Add(ObjectSetting);
						}
						ObjectSetting.MaterialShaderList.Add(item);
					}
					foreach (var item in FetchList<MaterialFloatProperty>(pluginCtrl, ddListNames[2]))
					{
						ObjectSetting ObjectSetting = data.Where(x => x.ObjectType == item.ObjectType && x.CoordinateIndex == item.CoordinateIndex && x.Slot == item.Slot).FirstOrDefault();
						if (ObjectSetting == null)
						{
							ObjectSetting = new ObjectSetting(item.ObjectType, item.CoordinateIndex, item.Slot);
							data.Add(ObjectSetting);
						}
						ObjectSetting.MaterialFloatPropertyList.Add(item);
					}
					foreach (var item in FetchList<MaterialColorProperty>(pluginCtrl, ddListNames[3]))
					{
						ObjectSetting ObjectSetting = data.Where(x => x.ObjectType == item.ObjectType && x.CoordinateIndex == item.CoordinateIndex && x.Slot == item.Slot).FirstOrDefault();
						if (ObjectSetting == null)
						{
							ObjectSetting = new ObjectSetting(item.ObjectType, item.CoordinateIndex, item.Slot);
							data.Add(ObjectSetting);
						}
						ObjectSetting.MaterialColorPropertyList.Add(item);
					}
					foreach (var item in FetchList<MaterialTextureProperty>(pluginCtrl, ddListNames[4]))
					{
						ObjectSetting ObjectSetting = data.Where(x => x.ObjectType == item.ObjectType && x.CoordinateIndex == item.CoordinateIndex && x.Slot == item.Slot).FirstOrDefault();
						if (ObjectSetting == null)
						{
							ObjectSetting = new ObjectSetting(item.ObjectType, item.CoordinateIndex, item.Slot);
							data.Add(ObjectSetting);
						}
						ObjectSetting.MaterialTexturePropertyList.Add(item);
					}
					foreach (var item in FetchList<MaterialCopy>(pluginCtrl, ddListNames[5]))
					{
						ObjectSetting ObjectSetting = data.Where(x => x.ObjectType == item.ObjectType && x.CoordinateIndex == item.CoordinateIndex && x.Slot == item.Slot).FirstOrDefault();
						if (ObjectSetting == null)
						{
							ObjectSetting = new ObjectSetting(item.ObjectType, item.CoordinateIndex, item.Slot);
							data.Add(ObjectSetting);
						}
						ObjectSetting.MaterialCopyList.Add(item);
					}
					data = data.OrderBy(x => x.ObjectType).ThenBy(x => x.CoordinateIndex).ThenBy(x => x.Slot).ToList();
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					File.WriteAllText(ExportFilePath, json);
					Logger.LogMessage($"All settings export to {ExportFilePath}");
				});
				ev.AddControl(new MakerButton($"Reset", category, Instance)).OnClick.AddListener(delegate
				{
					for (int i = 0; i < ddListNames.Count; i++)
						Traverse.Create(pluginCtrl).Field(ddListNames[i]).Method("Clear").GetValue();
					Logger.LogMessage($"All settings reset");
				});
				ev.AddControl(new MakerButton("Import", category, Instance)).OnClick.AddListener(delegate
				{
					string ExportFilePath = Path.Combine(SavePath, $"MaterialEditorHelper.json");
					List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
					List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
					List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
					List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
					List<MaterialShader> MaterialShaderList = new List<MaterialShader>();
					List<MaterialCopy> MaterialCopyList = new List<MaterialCopy>();
					List<ObjectSetting> data = JSONSerializer.Deserialize<List<ObjectSetting>>(File.ReadAllText(ExportFilePath));
					foreach (ObjectSetting item in data)
					{
						if (item.RendererPropertyList.Count > 0)
							Traverse.Create(pluginCtrl).Field("RendererPropertyList").Method("AddRange", new object[] { item.RendererPropertyList }).GetValue();
						if (item.MaterialFloatPropertyList.Count > 0)
							Traverse.Create(pluginCtrl).Field("MaterialFloatPropertyList").Method("AddRange", new object[] { item.MaterialFloatPropertyList }).GetValue();
						if (item.MaterialColorPropertyList.Count > 0)
							Traverse.Create(pluginCtrl).Field("MaterialColorPropertyList").Method("AddRange", new object[] { item.MaterialColorPropertyList }).GetValue();
						if (item.MaterialTexturePropertyList.Count > 0)
							Traverse.Create(pluginCtrl).Field("MaterialTexturePropertyList").Method("AddRange", new object[] { item.MaterialTexturePropertyList }).GetValue();
						if (item.MaterialShaderList.Count > 0)
							Traverse.Create(pluginCtrl).Field("MaterialShaderList").Method("AddRange", new object[] { item.MaterialShaderList }).GetValue();
						if (item.MaterialCopyList.Count > 0)
							Traverse.Create(pluginCtrl).Field("MaterialCopyList").Method("AddRange", new object[] { item.MaterialCopyList }).GetValue();
					}
					Logger.LogMessage($"All settings import from {ExportFilePath}");
				});

				ev.AddControl(new MakerSeparator(category, this));

				TextureDictionaryUI(ev, category);

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerButton("Reload", category, Instance)).OnClick.AddListener(delegate
				{
					string CardPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Paths.ExecutablePath) + "_MaterialEditorHelper.png");
					chaCtrl.chaFile.SaveCharaFile(CardPath, byte.MaxValue, false);

					chaCtrl.chaFile.LoadFileLimited(CardPath);
					if (chaCtrl.chaFile.GetLastErrorCode() != 0)
						throw new Exception("LoadFileLimited failed");
					chaCtrl.ChangeCoordinateType(true);
					chaCtrl.Reload();
					CustomBase.Instance.updateCustomUI = true;
				});
			};
		}

		internal static List<T> FetchList<T>(MaterialEditorCharaController pluginCtrl, string name)
		{
			var list = Traverse.Create(pluginCtrl).Field(name).GetValue<List<T>>();
			var data = (from row in list
						orderby Traverse.Create(row).Field("ObjectType").GetValue<ObjectType>(),
						Traverse.Create(row).Field("CoordinateIndex").GetValue<int>(),
						Traverse.Create(row).Field("Slot").GetValue<int>(),
						Traverse.Create(row).Field("MaterialName").GetValue<string>(),
						Traverse.Create(row).Field("Property").GetValue<string>()
						select row).ToList();
			return data;
		}

		[Serializable]
		public class ObjectSetting
		{
			public ObjectType ObjectType;
			public int CoordinateIndex;
			public int Slot;
			public List<RendererProperty> RendererPropertyList = new List<RendererProperty>();
			public List<MaterialFloatProperty> MaterialFloatPropertyList = new List<MaterialFloatProperty>();
			public List<MaterialColorProperty> MaterialColorPropertyList = new List<MaterialColorProperty>();
			public List<MaterialTextureProperty> MaterialTexturePropertyList = new List<MaterialTextureProperty>();
			public List<MaterialShader> MaterialShaderList = new List<MaterialShader>();
			public List<MaterialCopy> MaterialCopyList = new List<MaterialCopy>();
			public ObjectSetting(ObjectType _ObjectType, int _CoordinateIndex, int _Slot)
			{
				ObjectType = _ObjectType;
				CoordinateIndex = _CoordinateIndex;
				Slot = _Slot;
			}
		}

		internal sealed class ConfigurationManagerAttributes
		{
			public bool? Browsable;
		}
	}
}
