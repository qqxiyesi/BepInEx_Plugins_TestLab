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
	public partial class MaterialEditorHelper
	{
		private void TextureDictionaryUI(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ChaControl chaCtrl = MakerAPI.GetCharacterControl();
			MaterialEditorCharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<MaterialEditorCharaController>();

			ev.AddControl(new MakerText("TextureDictionary", category, this));

			ev.AddControl(new MakerButton("Export Texture", category, this)).OnClick.AddListener(delegate
			{
				Dictionary<int, TextureContainer> TextureDictionary = Traverse.Create(pluginCtrl).Field("TextureDictionary").GetValue<Dictionary<int, TextureContainer>>();
				string now = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
				string ExportPath = Path.Combine(MaterialEditorAPI.MaterialEditorPluginBase.ExportPath, now);
				Directory.CreateDirectory(ExportPath);

				int i = 0;
				foreach (KeyValuePair<int, TextureContainer> x in TextureDictionary)
				{
					Texture tex = x.Value.Texture;
					string filename = Path.Combine(ExportPath, $"{x.Key}_{tex.width}x{tex.height}.png");
					//Traverse.Create(MaterialEditorAPI.MaterialEditorPluginBase.Instance.GetType()).Method("SaveTex", new object[] { tex, filename, RenderTextureFormat.Default, RenderTextureReadWrite.Default }).GetValue();
					Traverse.Create(typeof(MaterialEditorAPI.MaterialEditorPluginBase)).Method("SaveTex", new object[] { tex, filename, RenderTextureFormat.Default, RenderTextureReadWrite.Default }).GetValue();
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
		}
	}
}
