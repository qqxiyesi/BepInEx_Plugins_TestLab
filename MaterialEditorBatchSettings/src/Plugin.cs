using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

using KKAPI.Maker;
using KKAPI.Maker.UI;

using KK_Plugins.MaterialEditor;
using static KK_Plugins.MaterialEditor.MaterialEditorCharaController;
using MaterialEditorAPI;
using MoreAccessoriesKOI;

namespace MaterialEditorBatchSettings
{
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInDependency("marco.kkapi", "1.1.5")]
	[BepInDependency("com.deathweasel.bepinex.materialeditor", "2.5")]
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
	public class MaterialEditorBatchSettings : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.mebs";
		public const string PluginName = "Material Editor Batch Settings";
		public const string Version = "2.2.2.0";

		internal static ConfigEntry<bool> EnableShadowCastingMode { get; private set; }
		internal static ConfigEntry<ShadowCastingMode> DefaultShadowCastingMode { get; private set; }
		internal static ConfigEntry<bool> EnableReceiveShadows { get; private set; }
		internal static ConfigEntry<ReceiveShadows> DefaultReceiveShadows { get; private set; }

		internal static ConfigEntry<bool> EnableRimV { get; private set; }
		internal static ConfigEntry<float> DefaultRimV { get; private set; }
		internal static ConfigEntry<bool> EnableRimPower { get; private set; }
		internal static ConfigEntry<float> DefaultRimPower { get; private set; }

		internal static ConfigEntry<bool> EnableShadowColor { get; private set; }
		internal static ConfigEntry<float> DefaultShadowColorR { get; private set; }
		internal static ConfigEntry<float> DefaultShadowColorG { get; private set; }
		internal static ConfigEntry<float> DefaultShadowColorB { get; private set; }
		internal static ConfigEntry<float> DefaultShadowColorA { get; private set; }

		internal static ConfigEntry<bool> EnableHairShadowColor { get; private set; }
		internal static ConfigEntry<float> DefaultHairShadowColorR { get; private set; }
		internal static ConfigEntry<float> DefaultHairShadowColorG { get; private set; }
		internal static ConfigEntry<float> DefaultHairShadowColorB { get; private set; }
		internal static ConfigEntry<float> DefaultHairShadowColorA { get; private set; }

		internal static ConfigEntry<bool> EnableSpeclarHeight { get; private set; }
		internal static ConfigEntry<float> DefaultSpeclarHeight { get; private set; }

		internal static ConfigEntry<int> DefaultRenderQueueHairFront { get; private set; }
		internal static ConfigEntry<int> DefaultRenderQueueHair { get; private set; }

		private void Start()
		{
			EnableShadowCastingMode = Config.Bind("Renderer", "Set Shadow Casting Mode", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
			DefaultShadowCastingMode = Config.Bind("Renderer", "Default Shadow Casting Mode", ShadowCastingMode.Off, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
			EnableReceiveShadows = Config.Bind("Renderer", "Set Receive Shadows", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultReceiveShadows = Config.Bind("Renderer", "Default Receive Shadows", ReceiveShadows.Off, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			EnableRimV = Config.Bind("Rim", "Set rimV", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
			DefaultRimV = Config.Bind("Rim", "Default rimV", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
			EnableRimPower = Config.Bind("Rim", "Set rimpower", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultRimPower = Config.Bind("Rim", "Default rimpower", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			EnableShadowColor = Config.Bind("ShadowColor", "Set Shadow Color", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 10 }));
			DefaultShadowColorR = Config.Bind("ShadowColor", "ShadowColorR", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
			DefaultShadowColorG = Config.Bind("ShadowColor", "ShadowColorG", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
			DefaultShadowColorB = Config.Bind("ShadowColor", "ShadowColorB", 0f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultShadowColorA = Config.Bind("ShadowColor", "ShadowColorA", 1f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			EnableHairShadowColor = Config.Bind("ShadowColorHair", "Set Hair Shadow Color", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 10 }));
			DefaultHairShadowColorR = Config.Bind("ShadowColorHair", "ShadowColorR", 0.9f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
			DefaultHairShadowColorG = Config.Bind("ShadowColorHair", "ShadowColorG", 0.9f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
			DefaultHairShadowColorB = Config.Bind("ShadowColorHair", "ShadowColorB", 0.9f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultHairShadowColorA = Config.Bind("ShadowColorHair", "ShadowColorA", 1f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			EnableSpeclarHeight = Config.Bind("SpeclarHeight", "Set SpeclarHeight", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultSpeclarHeight = Config.Bind("SpeclarHeight", "Default SpeclarHeight", 1f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			DefaultRenderQueueHairFront = Config.Bind("RenderQueue", "Hair Front", -1, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
			DefaultRenderQueueHair = Config.Bind("RenderQueue", "Hair", -1, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				MakerCategory category = new MakerCategory("05_ParameterTop", "tglMEBS", MakerConstants.Parameter.Attribute.Position + 1, "MEBS");

				#region Renderer
				var EnableShadowCastingModeToggle = new MakerToggle(category, "Shadow Casting Mode", EnableShadowCastingMode.Value, this);
				ev.AddControl(EnableShadowCastingModeToggle);
				EnableShadowCastingModeToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableShadowCastingMode.Value = EnableShadowCastingModeToggle.Value;
				}));
				var ShadowCastingModeList = Enum.GetNames(typeof(ShadowCastingMode)).ToList();
				var ShadowCastingModeDropdown = new MakerDropdown("Value", ShadowCastingModeList.ToArray(), category, (int) DefaultShadowCastingMode.Value, this);
				ev.AddControl(ShadowCastingModeDropdown);
				ShadowCastingModeDropdown.ValueChanged.Subscribe(Observer.Create<int>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						DefaultShadowCastingMode.Value = (ShadowCastingMode) ShadowCastingModeDropdown.Value;
				}));

				var EnableReceiveShadowsToggle = new MakerToggle(category, "Receive Shadows", EnableReceiveShadows.Value, this);
				ev.AddControl(EnableReceiveShadowsToggle);
				EnableReceiveShadowsToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableReceiveShadows.Value = EnableReceiveShadowsToggle.Value;
				}));
				var ReceiveShadowsList = Enum.GetNames(typeof(ReceiveShadows)).ToList();
				var ReceiveShadowsDropdown = new MakerDropdown("Value", ReceiveShadowsList.ToArray(), category, (int) DefaultReceiveShadows.Value, this);
				ev.AddControl(ReceiveShadowsDropdown);
				ReceiveShadowsDropdown.ValueChanged.Subscribe(Observer.Create<int>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						DefaultReceiveShadows.Value = (ReceiveShadows) ReceiveShadowsDropdown.Value;
				}));
				#endregion

				ev.AddControl(new MakerSeparator(category, this));

				#region ShadowColor
				var EnableShadowColorToggle = new MakerToggle(category, "ShadowColor", EnableShadowColor.Value, this);
				ev.AddControl(EnableShadowColorToggle);
				EnableShadowColorToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableShadowColor.Value = EnableShadowColorToggle.Value;
				}));
				var ShadowColorRTextbox = ev.AddControl(new MakerTextbox(category, "R", DefaultShadowColorR.DefaultValue.ToString(), this));
				ShadowColorRTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						ShadowColorRTextbox.Value = DefaultShadowColorR.DefaultValue.ToString();
					else
						DefaultShadowColorR.Value = float.Parse(s);
				});
				var ShadowColorGTextbox = ev.AddControl(new MakerTextbox(category, "G", DefaultShadowColorG.DefaultValue.ToString(), this));
				ShadowColorGTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						ShadowColorGTextbox.Value = DefaultShadowColorG.DefaultValue.ToString();
					else
						DefaultShadowColorG.Value = float.Parse(s);
				});
				var ShadowColorBTextbox = ev.AddControl(new MakerTextbox(category, "B", DefaultShadowColorB.DefaultValue.ToString(), this));
				ShadowColorBTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						ShadowColorBTextbox.Value = DefaultShadowColorB.DefaultValue.ToString();
					else
						DefaultShadowColorB.Value = float.Parse(s);
				});
				var ShadowColorATextbox = ev.AddControl(new MakerTextbox(category, "A", DefaultShadowColorA.DefaultValue.ToString(), this));
				ShadowColorATextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						ShadowColorATextbox.Value = DefaultShadowColorA.DefaultValue.ToString();
					else
						DefaultShadowColorA.Value = float.Parse(s);
				});
				#endregion

				//ev.AddControl(new MakerSeparator(category, this));

				#region Hair ShadowColor
				var EnableHairShadowColorToggle = new MakerToggle(category, "Hair ShadowColor", EnableHairShadowColor.Value, this);
				ev.AddControl(EnableHairShadowColorToggle);
				EnableHairShadowColorToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableHairShadowColor.Value = EnableHairShadowColorToggle.Value;
				}));
				var HairShadowColorRTextbox = ev.AddControl(new MakerTextbox(category, "R", DefaultHairShadowColorR.DefaultValue.ToString(), this));
				HairShadowColorRTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						HairShadowColorRTextbox.Value = DefaultHairShadowColorR.DefaultValue.ToString();
					else
						DefaultHairShadowColorR.Value = float.Parse(s);
				});
				var HairShadowColorGTextbox = ev.AddControl(new MakerTextbox(category, "G", DefaultHairShadowColorG.DefaultValue.ToString(), this));
				HairShadowColorGTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						HairShadowColorGTextbox.Value = DefaultHairShadowColorG.DefaultValue.ToString();
					else
						DefaultHairShadowColorG.Value = float.Parse(s);
				});
				var HairShadowColorBTextbox = ev.AddControl(new MakerTextbox(category, "B", DefaultHairShadowColorB.DefaultValue.ToString(), this));
				HairShadowColorBTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						HairShadowColorBTextbox.Value = DefaultHairShadowColorB.DefaultValue.ToString();
					else
						DefaultHairShadowColorB.Value = float.Parse(s);
				});
				var HairShadowColorATextbox = ev.AddControl(new MakerTextbox(category, "A", DefaultHairShadowColorA.DefaultValue.ToString(), this));
				HairShadowColorATextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						HairShadowColorATextbox.Value = DefaultHairShadowColorA.DefaultValue.ToString();
					else
						DefaultHairShadowColorA.Value = float.Parse(s);
				});
				#endregion

				ev.AddControl(new MakerSeparator(category, this));

				#region Rim
				var EnableRimVToggle = new MakerToggle(category, "rimV", EnableRimV.Value, this);
				ev.AddControl(EnableRimVToggle);
				EnableRimVToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableRimV.Value = EnableRimVToggle.Value;
				}));
				var RimVTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultRimV.DefaultValue.ToString(), this));
				RimVTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						RimVTextbox.Value = DefaultRimV.DefaultValue.ToString();
					else
						DefaultRimV.Value = float.Parse(s);
				});
				var EnableRimPowerToggle = new MakerToggle(category, "rimpower", EnableRimPower.Value, this);
				ev.AddControl(EnableRimPowerToggle);
				EnableRimPowerToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableRimPower.Value = EnableRimPowerToggle.Value;
				}));
				var RimPowerTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultRimPower.DefaultValue.ToString(), this));
				RimPowerTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						RimPowerTextbox.Value = DefaultRimPower.DefaultValue.ToString();
					else
						DefaultRimPower.Value = float.Parse(s);
				});
				#endregion

				ev.AddControl(new MakerSeparator(category, this));

				#region SpeclarHeight
				var EnableSpeclarHeightToggle = new MakerToggle(category, "SpeclarHeight", EnableSpeclarHeight.Value, this);
				ev.AddControl(EnableSpeclarHeightToggle);
				EnableSpeclarHeightToggle.ValueChanged.Subscribe(Observer.Create<bool>(value =>
				{
					if (MakerAPI.InsideAndLoaded)
						EnableSpeclarHeight.Value = EnableSpeclarHeightToggle.Value;
				}));
				var SpeclarHeightTextbox = ev.AddControl(new MakerTextbox(category, "Value", DefaultSpeclarHeight.DefaultValue.ToString(), this));
				SpeclarHeightTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						SpeclarHeightTextbox.Value = DefaultSpeclarHeight.DefaultValue.ToString();
					else
						DefaultSpeclarHeight.Value = float.Parse(s);
				});
				#endregion

				ev.AddControl(new MakerSeparator(category, this));

				#region RenderQueue
				//ev.AddControl(new MakerText("RenderQueue", category, this));
				var RenderQueueHairFrontTextbox = ev.AddControl(new MakerTextbox(category, "HairFront", DefaultRenderQueueHairFront.DefaultValue.ToString(), this));
				RenderQueueHairFrontTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						RenderQueueHairFrontTextbox.Value = DefaultRenderQueueHairFront.DefaultValue.ToString();
					else
						DefaultRenderQueueHairFront.Value = int.Parse(s);
				});
				var RenderQueueHairTextbox = ev.AddControl(new MakerTextbox(category, "Hair", DefaultRenderQueueHair.DefaultValue.ToString(), this));
				RenderQueueHairTextbox.ValueChanged.Subscribe(s =>
				{
					if (string.IsNullOrEmpty(s))
						RenderQueueHairTextbox.Value = DefaultRenderQueueHair.DefaultValue.ToString();
					else
						DefaultRenderQueueHair.Value = int.Parse(s);
				});
				#endregion

				ev.AddControl(new MakerSeparator(category, this));

				var btnApply = new MakerButton("Apply", category, this);
				ev.AddControl(btnApply);
				btnApply.OnClick.AddListener(delegate
				{
					ApplySettings();
					ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
				});

				ev.AddSubCategory(category);
			};
		}

		internal void ApplySettings()
		{
			ChaControl chaCtrl = MakerAPI.GetCharacterControl();
			MaterialEditorCharaController pluginCtrl = chaCtrl?.gameObject?.GetComponent<MaterialEditorCharaController>();

			for (int i = 0; i < chaCtrl.objClothes.Length; i++)
				SetValue(pluginCtrl, ObjectType.Clothing, i, chaCtrl.objClothes[i]);
			for (int i = 0; i < chaCtrl.objHair.Length; i++)
				SetValue(pluginCtrl, ObjectType.Hair, i, chaCtrl.objHair[i]);
			for (int i = 0; i < chaCtrl.objAccessory.Length; i++)
				if (chaCtrl.nowCoordinate.accessory.parts[i].type != 120)
					SetValue(pluginCtrl, ObjectType.Accessory, i, chaCtrl.objAccessory[i]);
			for (int i = 0; i < MoreAccessories._self._charaMakerData.nowAccessories.Count; i++)
				if (MoreAccessories._self._charaMakerData.nowAccessories[i].type != 120)
					SetValue(pluginCtrl, ObjectType.Accessory, (i + 20), MoreAccessories._self._charaMakerData.objAccessory.ElementAtOrDefault(i));
		}

		private void SetValue(MaterialEditorCharaController pluginCtrl, ObjectType objectType, int slot, GameObject gameObj)
		{
			if (gameObj == null)
			{
				Logger.LogWarning($"slot[{slot + 1:00}] GameObject is null");
				return;
			}

			Color HairShadowColor = new Color(DefaultHairShadowColorR.Value, DefaultHairShadowColorG.Value, DefaultHairShadowColorB.Value, DefaultHairShadowColorA.Value);
			Color GeneralShadowColor = new Color(DefaultShadowColorR.Value, DefaultShadowColorG.Value, DefaultShadowColorB.Value, DefaultShadowColorA.Value);

			List<Renderer> rendList = MaterialAPI.GetRendererList(gameObj).ToList();
			for (int j = 0; j < rendList.Count; j++)
			{
				if (EnableShadowCastingMode.Value)
					pluginCtrl.SetRendererProperty(slot, objectType, rendList[j], MaterialAPI.RendererProperties.ShadowCastingMode, ((int) DefaultShadowCastingMode.Value).ToString(), gameObj);
				if (EnableReceiveShadows.Value)
					pluginCtrl.SetRendererProperty(slot, objectType, rendList[j], MaterialAPI.RendererProperties.ReceiveShadows, ((int) DefaultReceiveShadows.Value).ToString(), gameObj);

				List<Material> mats = MaterialAPI.GetMaterials(gameObj, rendList[j]).ToList();
				for (int k = 0; k < mats.Count; k++)
				{
					Material mat = mats[k];
					if (EnableShadowColor.Value || EnableHairShadowColor.Value)
					{
						if (mat.HasProperty("_ShadowColor"))
						{
							if (mat.shader.name.Contains("main_hair"))
							{
								if (EnableHairShadowColor.Value)
									pluginCtrl.SetMaterialColorProperty(slot, objectType, mat, "ShadowColor", HairShadowColor, gameObj);
							}
							else
							{
								if (EnableShadowColor.Value)
									pluginCtrl.SetMaterialColorProperty(slot, objectType, mat, "ShadowColor", GeneralShadowColor, gameObj);
							}
						}
					}

					if (EnableRimV.Value)
						if (mat.HasProperty("_rimV"))
							pluginCtrl.SetMaterialFloatProperty(slot, objectType, mat, "rimV", DefaultRimV.Value, gameObj);
					if (EnableRimPower.Value)
						if (mat.HasProperty("_rimpower"))
							pluginCtrl.SetMaterialFloatProperty(slot, objectType, mat, "rimpower", DefaultRimPower.Value, gameObj);

					if (EnableSpeclarHeight.Value)
						if (mat.HasProperty("_SpeclarHeight"))
							pluginCtrl.SetMaterialFloatProperty(slot, objectType, mat, "SpeclarHeight", DefaultSpeclarHeight.Value, gameObj);

					if (mat.shader.name.Contains("main_hair"))
					{
						int queue = -1;
						if (mat.shader.name.Contains("_front"))
							queue = DefaultRenderQueueHairFront.Value;
						else
							queue = DefaultRenderQueueHair.Value;

						if (queue > -1)
							pluginCtrl.SetMaterialShaderRenderQueue(slot, objectType, mat, queue, gameObj);
					}
				}
			}
		}

		internal enum ShadowCastingMode
		{
			Off,
			On,
			TwoSided,
			ShadowsOnly,
		}

		internal enum ReceiveShadows
		{
			Off,
			On,
		}

		internal sealed class ConfigurationManagerAttributes
		{
			public int? Order;
		}
	}
}
