using System.Collections.Generic;

using ChaCustom;
using UniRx;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using KKAPI.Utilities;
using JetPack;

namespace ParentSwitch
{
#if KK
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInDependency("marco.kkapi", "1.17")]
#elif KKS
	[BepInProcess("KoikatsuSunshine")]
	[BepInDependency("marco.kkapi", "1.23")]
#endif
	[BepInPlugin(GUID, Name, Version)]
	public partial class ParentSwitch : BaseUnityPlugin
	{
		public const string GUID = "ParentSwitch";
		public const string Name = "ParentSwitch";
		public const string Version = "1.1.0.0";

		internal static ManualLogSource _logger;
		internal static ParentSwitch _instance;
		internal static ParentSwitchUI _makerConfigWindow;
		internal static Harmony _hooksInstance;
		internal static SidebarToggle _sidebarToggleEnable;

		internal static ConfigEntry<float> _cfgMakerWinX;
		internal static ConfigEntry<float> _cfgMakerWinY;
		internal static ConfigEntry<bool> _cfgDragPass;

		internal static ConfigEntry<bool> _cfgDebugCamera;
		internal static ConfigEntry<bool> _cfgDebugTpose;
		internal static ConfigEntry<bool> _cfgDebugRatio;

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

			_cfgDebugCamera = Config.Bind("Debug", "Camera Test", false, new ConfigDescription("Don't reset camera on process start", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 17 }));
			_cfgDebugTpose = Config.Bind("Debug", "Tpose Test", false, new ConfigDescription("Don't set to Tpose on process start", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 16 }));
			_cfgDebugRatio = Config.Bind("Debug", "Ratio Test", false, new ConfigDescription("Testing scale calculation formula", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 15 }));
		}

		private void Start()
		{
			MakerAPI.RegisterCustomSubCategories += (_sender, _args) =>
			{
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
			};
		}

		internal static ChaControl _chaCtrl => CustomBase.Instance?.chaCtrl;

		internal static List<ChaFileAccessory.PartsInfo> ListPartsInfo()
		{
			return Accessory.ListNowAccessories(_chaCtrl);
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
					_makerConfigWindow._parents.Clear();
				}
			}
		}
	}
}
