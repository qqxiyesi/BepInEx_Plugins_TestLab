using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UniRx;
using ParadoxNotion.Serialization;
using ChaCustom;

using BepInEx;
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
	[BepInDependency("marco.kkapi", "1.1.5")]
	[BepInDependency("com.deathweasel.bepinex.materialeditor", "2.5")]
	public class MaterialEditorHelper : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.meh";
		public const string PluginName = "Material Editor Helper";
		public const string Version = "1.0.0.0";

		internal static new ManualLogSource Logger;
		internal static MaterialEditorHelper Instance;

		internal static string SavePath = "";

		internal static MakerDropdown ddList;
		internal static List<string> ddListLabel = new List<string>() { "Renderer", "Shader", "Float Property", "Color Property", "Texture Property" };
		internal static List<string> ddListNames = new List<string>() { "RendererPropertyList", "MaterialShaderList", "MaterialFloatPropertyList", "MaterialColorPropertyList", "MaterialTexturePropertyList" };

		private void Awake()
		{
			Logger = base.Logger;
			Instance = this;

			SavePath = Path.Combine(Paths.GameRootPath, "Temp");
		}

		private void Start()
		{
			BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.materialeditor", out PluginInfo PluginInfo);
			//System.Type MaterialEditorPluginBase = PluginInfo.Instance.GetType().Assembly.GetType("MaterialEditorAPI.MaterialEditorPluginBase");

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				ChaControl chaCtrl = MakerAPI.GetCharacterControl();
				MaterialEditorCharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<MaterialEditorCharaController>();

				MakerCategory category = new MakerCategory("05_ParameterTop", "tglMEHelper", MakerConstants.Parameter.Attribute.Position + 1, "MEH");

				ddList = new MakerDropdown("List", ddListLabel.ToArray(), category, 0, this);
				ev.AddControl(ddList);

				MakerButton btnPrint = new MakerButton("Print", category, Instance);
				ev.AddControl(btnPrint);
				btnPrint.OnClick.AddListener(delegate
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
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					Logger.LogInfo($"{ddListNames[ddList.Value]}\n{json}");
				});
				MakerButton btnExport = new MakerButton($"Export", category, Instance);
				ev.AddControl(btnExport);
				btnExport.OnClick.AddListener(delegate
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
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					File.WriteAllText(ExportFilePath, json);
					Logger.LogMessage($"{ddListNames[ddList.Value]} export to {ExportFilePath}");
				});
				MakerButton btnReset = new MakerButton($"Reset", category, Instance);
				ev.AddControl(btnReset);
				btnReset.OnClick.AddListener(delegate
				{
					Traverse.Create(pluginCtrl).Field(ddListNames[ddList.Value]).Method("Clear").GetValue();
					Logger.LogMessage($"{ddListNames[ddList.Value]} reset");
				});
				MakerButton btnImport = new MakerButton("Import", category, Instance);
				ev.AddControl(btnImport);
				btnImport.OnClick.AddListener(delegate
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
					Traverse.Create(pluginCtrl).Field(ddListNames[ddList.Value]).Method("AddRange", new object[] { data }).GetValue();
					Logger.LogMessage($"{ddListNames[ddList.Value]} import from {ExportFilePath}");
				});

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerText("TextureDictionary", category, this));

				MakerButton btnTexExport = ev.AddControl(new MakerButton("Export Texture", category, this));
				btnTexExport.OnClick.AddListener(delegate
				{
					Dictionary<int, TextureContainer> TextureDictionary = Traverse.Create(pluginCtrl).Field("TextureDictionary").GetValue<Dictionary<int, TextureContainer>>();
					string now = $"{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
					string ExportPath = Path.Combine(MaterialEditorAPI.MaterialEditorPluginBase.ExportPath, now);
					Directory.CreateDirectory(ExportPath);

					int i = 0;
					foreach (KeyValuePair<int, TextureContainer> x in TextureDictionary)
					{
						Texture2D tex = x.Value.Texture;
						string filename = Path.Combine(ExportPath, $"{x.Key}_{tex.width}x{tex.height}.png");
						Traverse.Create(MaterialEditorAPI.MaterialEditorPluginBase.Instance.GetType()).Method("SaveTex", new object[] { tex, filename, RenderTextureFormat.Default, RenderTextureReadWrite.Default }).GetValue();
						i++;
						Logger.LogInfo($"Texture exported: {filename}");
					}
					Logger.LogMessage($"Total {i} files exported to {ExportPath}");

					List<MaterialTextureProperty> MaterialTexturePropertyList = Traverse.Create(pluginCtrl).Field("MaterialTexturePropertyList").GetValue<List<MaterialTextureProperty>>();
					List<MaterialTextureProperty> data = MaterialTexturePropertyList
						.Where(x => x.TexID != null)
						.OrderBy(x => x.TexID)
						.ThenBy(x => x.ObjectType)
						.ThenBy(x => x.CoordinateIndex)
						.ThenBy(x => x.Slot)
						.ThenBy(x => x.MaterialName)
						.ThenBy(x => x.Property)
						.ToList();
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					File.WriteAllText(Path.Combine(ExportPath, "List.json"), json);
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerButton btnReload = new MakerButton("Reload", category, Instance);
				ev.AddControl(btnReload);
				btnReload.OnClick.AddListener(delegate
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

				ev.AddSubCategory(category);
			};
		}

		internal static List<T> FetchList<T>(MaterialEditorCharaController pluginCtrl, string name)
		{
			var list = Traverse.Create(pluginCtrl).Field(name).GetValue<List<T>>();
			var data = (from row in list
						orderby Traverse.Create(row).Field("ObjectType").GetValue<ObjectType>(),
						Traverse.Create(row).Field("CoordinateIndex").GetValue<int>(),
						Traverse.Create(row).Field("Slot").GetValue<int>()
						select row).ToList();
			return data;
		}
	}
}
