using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ChaCustom;
using UniRx;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using MoreAccessoriesKOI;

using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using KKAPI.Utilities;

namespace ParentSwitch
{
	[BepInPlugin(GUID, Name, Version)]
	public partial class ParentSwitch : BaseUnityPlugin
	{
		public const string GUID = "ParentSwitch";
		public const string Name = "ParentSwitch";
		public const string Version = "1.0.0.0";

		internal static ManualLogSource _logger;
		internal static ParentSwitch _instance;
		internal static ParentSwitchUI _makerConfigWindow;
		internal static Harmony _hooksInstance;
		internal static SidebarToggle _sidebarToggleEnable;

		internal static ConfigEntry<float> _cfgMakerWinX;
		internal static ConfigEntry<float> _cfgMakerWinY;
		internal static ConfigEntry<bool> _cfgDragPass;

		private void Awake()
		{
			_logger = base.Logger;
			_instance = this;

			_cfgDragPass = Config.Bind("Maker", "Drag Pass Mode", false, new ConfigDescription("Setting window will not block mouse dragging", null, new ConfigurationManagerAttributes { Order = 15 }));
			_cfgMakerWinX = Config.Bind("Maker", "Config Window Startup X", 525f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 19 }));
			_cfgMakerWinX.SettingChanged += (_sender, _args) =>
			{
				if (_makerConfigWindow == null) return;
				if (_makerConfigWindow._windowPos.x != _cfgMakerWinX.Value)
				{
					_makerConfigWindow._windowPos.x = _cfgMakerWinX.Value;
				}
			};
			_cfgMakerWinY = Config.Bind("Maker", "Config Window Startup Y", 80f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 18 }));
			_cfgMakerWinY.SettingChanged += (_sender, _args) =>
			{
				if (_makerConfigWindow == null) return;
				if (_makerConfigWindow._windowPos.y != _cfgMakerWinY.Value)
				{
					_makerConfigWindow._windowPos.y = _cfgMakerWinY.Value;
				}
			};
		}

		private void Start()
		{
			MakerAPI.RegisterCustomSubCategories += (_sender, _args) =>
			{
				_accessoriesByChar = Traverse.Create(MoreAccessories._self).Field("_accessoriesByChar").GetValue<Dictionary<ChaFile, MoreAccessories.CharAdditionalData>>();
				_makerConfigWindow = _instance.gameObject.AddComponent<ParentSwitchUI>();

				_sidebarToggleEnable = _args.AddSidebarControl(new SidebarToggle(Name, false, _instance));
				_sidebarToggleEnable.ValueChanged.Subscribe(_value =>
				{
					if (_makerConfigWindow.enabled != _value)
						_makerConfigWindow.enabled = _value;
				});

				_hooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
			};
			MakerAPI.MakerExiting += (_sender, _args) =>
			{
				_hooksInstance.UnpatchAll(_hooksInstance.Id);
				_hooksInstance = null;

				Destroy(_makerConfigWindow);

				_sidebarToggleEnable = null;
				_accessoriesByChar = new Dictionary<ChaFile, MoreAccessories.CharAdditionalData>();
			};
		}

		internal static ChaControl _chaCtrl => CustomBase.Instance?.chaCtrl;
		internal static Dictionary<ChaFile, MoreAccessories.CharAdditionalData> _accessoriesByChar = new Dictionary<ChaFile, MoreAccessories.CharAdditionalData>();

		internal static List<ChaFileAccessory.PartsInfo> ListPartsInfo()
		{
			List<ChaFileAccessory.PartsInfo> _partInfo = _chaCtrl.nowCoordinate.accessory.parts.ToList();
			_partInfo.AddRange(_accessoriesByChar[_chaCtrl.chaFile].nowAccessories ?? new List<ChaFileAccessory.PartsInfo>());
			return _partInfo;
		}

		internal static class Hooks
		{
			[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
			private static void ChaControl_ChangeCoordinateType_Postfix()
			{
				if (MakerAPI.InsideAndLoaded && _makerConfigWindow != null)
				{
					_makerConfigWindow._checkboxList.Clear();
					_makerConfigWindow._selectedParent = "";
				}
			}
		}
	}
}
