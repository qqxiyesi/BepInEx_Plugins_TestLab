using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using ParadoxNotion.Serialization;
using UnityEngine;
using UniRx;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Utilities;

using KK_Plugins.DynamicBoneEditor;
using MoreAccessoriesKOI;

namespace DynamicBoneEditorBatchSettings
{
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("com.deathweasel.bepinex.dynamicboneeditor")]
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
	[BepInPlugin(GUID, Name, Version)]
	public class DynamicBoneEditorBatchSettings : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.dbebs";
		public const string Name = "Dynamic Bone Editor Batch Settings";
		public const string Version = "1.1.0.0";

		internal static ConfigEntry<bool> EnableFreezeAxis;
		internal static ConfigEntry<DynamicBone.FreezeAxis> DefaultFreezeAxis;
		internal static ConfigEntry<bool> EnableWeight;
		internal static ConfigEntry<float> DefaultWeight;
		internal static ConfigEntry<bool> EnableDamping;
		internal static ConfigEntry<float> DefaultDamping;
		internal static ConfigEntry<bool> EnableElasticity;
		internal static ConfigEntry<float> DefaultElasticity;
		internal static ConfigEntry<bool> EnableStiffness;
		internal static ConfigEntry<float> DefaultStiffness;
		internal static ConfigEntry<bool> EnableInertia;
		internal static ConfigEntry<float> DefaultInertia;
		internal static ConfigEntry<bool> EnableRadius;
		internal static ConfigEntry<float> DefaultRadius;

		internal static string SavePath = Path.Combine(Paths.GameRootPath, "Temp");

		private void Start()
		{
			EnableFreezeAxis = Config.Bind("General", "Enable FreezeAxis", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 13 }));
			DefaultFreezeAxis = Config.Bind("General", "Default FreezeAxis", DynamicBone.FreezeAxis.None, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 12 }));
			EnableWeight = Config.Bind("General", "Enable Weight", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 11 }));
			DefaultWeight = Config.Bind("General", "Default Weight", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 10 }));
			EnableDamping = Config.Bind("General", "Enable Damping", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 9 }));
			DefaultDamping = Config.Bind("General", "Default Damping", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 8 }));
			EnableElasticity = Config.Bind("General", "Enable Elasticity", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 7 }));
			DefaultElasticity = Config.Bind("General", "Default Elasticity", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 6 }));
			EnableStiffness = Config.Bind("General", "Enable Stiffness", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 5 }));
			DefaultStiffness = Config.Bind("General", "Default Stiffness", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 4 }));
			EnableInertia = Config.Bind("General", "Enable Inertia", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
			DefaultInertia = Config.Bind("General", "Default Inertia", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
			EnableRadius = Config.Bind("General", "Enable Radius", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultRadius = Config.Bind("General", "Default Radius", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				MakerCategory category = new MakerCategory("05_ParameterTop", "tglDBEBS", MakerConstants.Parameter.Attribute.Position + 2, "DBEBS");
				ev.AddSubCategory(category);

				MakerToggle EnableFreezeAxisToggle = ev.AddControl(new MakerToggle(category, "FreezeAxis", EnableFreezeAxis.Value, this));
				EnableFreezeAxisToggle.ValueChanged.Subscribe(value =>
				{
					EnableFreezeAxis.Value = EnableFreezeAxisToggle.Value;
				});
				List<string> FreezeAxisList = Enum.GetNames(typeof(DynamicBone.FreezeAxis)).ToList();
				MakerDropdown FreezeAxisDropdown = new MakerDropdown("Value", FreezeAxisList.ToArray(), category, (int) DefaultFreezeAxis.Value, this);
				ev.AddControl(FreezeAxisDropdown);
				FreezeAxisDropdown.ValueChanged.Subscribe(Observer.Create<int>(value =>
				{
					DefaultFreezeAxis.Value = (DynamicBone.FreezeAxis) FreezeAxisDropdown.Value;
				}));

				ev.AddControl(new MakerSeparator(category, this));

				MakerToggle EnableWeightToggle = ev.AddControl(new MakerToggle(category, "Weight", EnableWeight.Value, this));
				EnableWeightToggle.ValueChanged.Subscribe(value =>
				{
					EnableWeight.Value = EnableWeightToggle.Value;
				});
				MakerTextbox DefaultWeightTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultWeight.DefaultValue.ToString(), this));
				DefaultWeightTextbox.ValueChanged.Subscribe(value =>
				{
					if (string.IsNullOrEmpty(value))
						DefaultWeightTextbox.Value = DefaultWeight.DefaultValue.ToString();
					else
					{
						if (float.TryParse(value, out float x))
							DefaultWeight.Value = x;
						else
							DefaultWeightTextbox.Value = DefaultWeight.DefaultValue.ToString();
					}
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerToggle EnableDampingToggle = ev.AddControl(new MakerToggle(category, "Damping", EnableDamping.Value, this));
				EnableDampingToggle.ValueChanged.Subscribe(value =>
				{
					EnableDamping.Value = EnableDampingToggle.Value;
				});
				MakerTextbox DefaultDampingTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultDamping.DefaultValue.ToString(), this));
				DefaultDampingTextbox.ValueChanged.Subscribe(value =>
				{
					if (string.IsNullOrEmpty(value))
						DefaultDampingTextbox.Value = DefaultDamping.DefaultValue.ToString();
					else
					{
						if (float.TryParse(value, out float x))
							DefaultDamping.Value = x;
						else
							DefaultDamping.Value = float.Parse(value);
					}
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerToggle EnableElasticityToggle = ev.AddControl(new MakerToggle(category, "Elasticity", EnableElasticity.Value, this));
				EnableElasticityToggle.ValueChanged.Subscribe(value =>
				{
					EnableElasticity.Value = EnableElasticityToggle.Value;
				});
				MakerTextbox DefaultElasticityTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultElasticity.DefaultValue.ToString(), this));
				DefaultElasticityTextbox.ValueChanged.Subscribe(value =>
				{
					if (string.IsNullOrEmpty(value))
						DefaultElasticityTextbox.Value = DefaultElasticity.DefaultValue.ToString();
					else
					{
						if (float.TryParse(value, out float x))
							DefaultElasticity.Value = x;
						else
							DefaultElasticity.Value = float.Parse(value);
					}
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerToggle EnableStiffnessToggle = ev.AddControl(new MakerToggle(category, "Stiffness", EnableStiffness.Value, this));
				EnableStiffnessToggle.ValueChanged.Subscribe(value =>
				{
					EnableStiffness.Value = EnableStiffnessToggle.Value;
				});
				MakerTextbox DefaultStiffnessTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultStiffness.DefaultValue.ToString(), this));
				DefaultStiffnessTextbox.ValueChanged.Subscribe(value =>
				{
					if (string.IsNullOrEmpty(value))
						DefaultStiffnessTextbox.Value = DefaultStiffness.DefaultValue.ToString();
					else
					{
						if (float.TryParse(value, out float x))
							DefaultStiffness.Value = x;
						else
							DefaultStiffness.Value = float.Parse(value);
					}
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerToggle EnableInertiaToggle = ev.AddControl(new MakerToggle(category, "Inertia", EnableInertia.Value, this));
				EnableInertiaToggle.ValueChanged.Subscribe(value =>
				{
					EnableInertia.Value = EnableInertiaToggle.Value;
				});
				MakerTextbox DefaultInertiaTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultInertia.DefaultValue.ToString(), this));
				DefaultInertiaTextbox.ValueChanged.Subscribe(value =>
				{
					if (string.IsNullOrEmpty(value))
						DefaultInertiaTextbox.Value = DefaultInertia.DefaultValue.ToString();
					else
					{
						if (float.TryParse(value, out float x))
							DefaultInertia.Value = x;
						else
							DefaultInertia.Value = float.Parse(value);
					}
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerToggle EnableRadiusToggle = ev.AddControl(new MakerToggle(category, "Radius", EnableRadius.Value, this));
				EnableRadiusToggle.ValueChanged.Subscribe(value =>
				{
					EnableRadius.Value = EnableRadiusToggle.Value;
				});
				MakerTextbox DefaultRadiusTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultRadius.DefaultValue.ToString(), this));
				DefaultRadiusTextbox.ValueChanged.Subscribe(value =>
				{
					if (string.IsNullOrEmpty(value))
						DefaultRadiusTextbox.Value = DefaultRadius.DefaultValue.ToString();
					else
					{
						if (float.TryParse(value, out float x))
							DefaultRadius.Value = x;
						else
							DefaultRadius.Value = float.Parse(value);
					}
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerRadioButtons ModeRadioButtons = ev.AddControl(new MakerRadioButtons(category, this, "Mode", "All", "Hair", "Item"));

				ev.AddControl(new MakerButton("Apply (Outfit)", category, this)).OnClick.AddListener(delegate
				{
					ApplySettings(ModeRadioButtons.Value);
					ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
				});

				ev.AddControl(new MakerSeparator(category, this));

				MakerRadioButtons ExportRadioButtons = ev.AddControl(new MakerRadioButtons(category, this, "Mode", "Chara", "Outfit"));

				ev.AddControl(new MakerButton("Reset", category, this)).OnClick.AddListener(delegate
				{
					ChaControl chaCtrl = MakerAPI.GetCharacterControl();
					CharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<CharaController>();
					if (ExportRadioButtons.Value == 1)
					{
						List<DynamicBoneData> data = Traverse.Create(pluginCtrl).Field("AccessoryDynamicBoneData").GetValue<List<DynamicBoneData>>();
						data.RemoveAll(x => x.CoordinateIndex == chaCtrl.fileStatus.coordinateType);
					}
					else
					{
						Traverse.Create(pluginCtrl).Field("AccessoryDynamicBoneData").Method("Clear").GetValue();
					}

					ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
				});

				ev.AddControl(new MakerButton("Export", category, this)).OnClick.AddListener(delegate
				{
					ChaControl chaCtrl = MakerAPI.GetCharacterControl();
					CharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<CharaController>();
					string ExportFilePath = Path.Combine(SavePath, "DBEBS.json");
					List<DynamicBoneData> data = Traverse.Create(pluginCtrl).Field("AccessoryDynamicBoneData").GetValue<List<DynamicBoneData>>().ToList();
					if (ExportRadioButtons.Value == 1)
					{
						data.RemoveAll(x => x.CoordinateIndex != chaCtrl.fileStatus.coordinateType);
						data = data.OrderBy(x => x.Slot).ThenBy(x => x.BoneName).ToList();
					}
					else
					{
						data = data.OrderBy(x => x.CoordinateIndex).ThenBy(x => x.Slot).ThenBy(x => x.BoneName).ToList();
					}
					string json = JSONSerializer.Serialize(data.GetType(), data, true);
					File.WriteAllText(ExportFilePath, json);
					Logger.LogMessage($"Settings export to {ExportFilePath}");
				});

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerButton("Import (Chara)", category, this)).OnClick.AddListener(delegate
				{
					ChaControl chaCtrl = MakerAPI.GetCharacterControl();
					CharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<CharaController>();
					string ExportFilePath = Path.Combine(SavePath, "DBEBS.json");
					List<DynamicBoneData> data = JSONSerializer.Deserialize<List<DynamicBoneData>>(File.ReadAllText(ExportFilePath));
					Traverse.Create(pluginCtrl).Field("AccessoryDynamicBoneData").Method("AddRange", new object[] { data.ToList() }).GetValue();
					Logger.LogMessage($"Settings import from {ExportFilePath}");
				});
			};
		}

		internal void ApplySettings(int mode)
		{
			ChaControl chaCtrl = MakerAPI.GetCharacterControl();
			CharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<CharaController>();

			for (int i = 0; i < chaCtrl.objAccessory.Length; i++)
				if (chaCtrl.nowCoordinate.accessory.parts[i].type != 120)
				{
					SetValue(pluginCtrl, i, chaCtrl.objAccessory[i], mode);
				}
			for (int i = 0; i < MoreAccessories._self._charaMakerData.nowAccessories.Count; i++)
				if (MoreAccessories._self._charaMakerData.nowAccessories[i].type != 120)
				{
					SetValue(pluginCtrl, (i + 20), MoreAccessories._self._charaMakerData.objAccessory.ElementAtOrDefault(i), mode);
				}
		}

		private void SetValue(CharaController pluginCtrl, int slot, GameObject gameObj, int mode)
		{
			if (gameObj == null)
			{
				Logger.LogWarning($"slot[{slot + 1:00}] GameObject is null");
				return;
			}

			if (mode == 1 && !IsHairAccessory(gameObj))
				return;
			else if (mode == 2 && IsHairAccessory(gameObj))
				return;

			List<DynamicBone> dynamicBones = gameObj.GetComponentsInChildren<DynamicBone>().Where(x => x.m_Root != null).ToList();
			if (dynamicBones.Count == 0)
			{
				Logger.LogWarning($"slot[{slot + 1:00}] does not have Dynamic Bone setting");
				return;
			}

			if (EnableFreezeAxis.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetFreezeAxis(slot, dynamicBone, (DynamicBone.FreezeAxis) DefaultFreezeAxis.Value);

			if (EnableWeight.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetWeight(slot, dynamicBone, DefaultWeight.Value);

			if (EnableDamping.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetDamping(slot, dynamicBone, DefaultDamping.Value);

			if (EnableElasticity.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetElasticity(slot, dynamicBone, DefaultElasticity.Value);

			if (EnableStiffness.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetStiffness(slot, dynamicBone, DefaultStiffness.Value);

			if (EnableInertia.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetInertia(slot, dynamicBone, DefaultInertia.Value);

			if (EnableRadius.Value)
				foreach (DynamicBone dynamicBone in dynamicBones)
					pluginCtrl.SetRadius(slot, dynamicBone, DefaultRadius.Value);
		}

		internal static bool IsHairAccessory(GameObject gameObj) => gameObj?.GetComponent<ChaCustomHairComponent>() != null;
	}
}
