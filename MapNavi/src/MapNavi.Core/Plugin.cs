using UnityEngine;

using BepInEx;
using BepInEx.Configuration;

using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;

namespace MapNavi
{
	[BepInProcess(Constants.StudioProcessName)]
	[BepInPlugin(GUID, PluginName, Version)]
	public partial class MapNavi : BaseUnityPlugin
	{
		public const string GUID = "MapNavi";
		public const string PluginName = "MapNavi";
		public const string Version = "1.0.1.0";

		internal static ConfigEntry<KeyboardShortcut> _cfgShortcut;
		internal static ConfigEntry<float> _cfgMakerWinX;
		internal static ConfigEntry<float> _cfgMakerWinY;
		internal static ConfigEntry<float> _cfgPosIncValue;
		internal static ConfigEntry<float> _cfgRotIncValue;

		internal static MapNaviUI _studioConfigWindow;
		internal static ToolbarToggle _ttConfigWindow;

		private void Awake()
		{
			_cfgShortcut = Config.Bind("Config", "Keyboard Shortcut", new KeyboardShortcut(KeyCode.None));
			_cfgMakerWinX = Config.Bind("Config", "Config Window Startup X", 95f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 10 }));
			_cfgMakerWinX.SettingChanged += (_sender, _args) =>
			{
				if (_studioConfigWindow != null)
				{
					if (_studioConfigWindow._windowPos.x != _cfgMakerWinX.Value)
						_studioConfigWindow._windowPos.x = _cfgMakerWinX.Value;
				}
			};
			_cfgMakerWinY = Config.Bind("Config", "Config Window Startup Y", 440f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 9 }));
			_cfgMakerWinY.SettingChanged += (_sender, _args) =>
			{
				if (_studioConfigWindow != null)
				{
					if (_studioConfigWindow._windowPos.y != _cfgMakerWinY.Value)
						_studioConfigWindow._windowPos.y = _cfgMakerWinY.Value;
				}
			};
			_cfgPosIncValue = Config.Bind("Config", "Position Increament", 0.1f, new ConfigDescription("Position increament/decrement initiial setting", new AcceptableValueList<float>(0.001f, 0.01f, 0.1f, 1f), new ConfigurationManagerAttributes { Order = 8 }));
			_cfgRotIncValue = Config.Bind("Config", "Rotation Increament", 1f, new ConfigDescription("Rotation increament/decrement initiial setting", new AcceptableValueList<float>(0.1f, 1f, 5f, 10f), new ConfigurationManagerAttributes { Order = 7 }));

			StudioAPI.StudioLoadedChanged += (_sender, _args) => _studioConfigWindow = gameObject.AddComponent<MapNaviUI>();

			Texture2D _iconTex = TextureUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("toolbar_icon.png"));
			_ttConfigWindow = CustomToolbarButtons.AddLeftToolbarToggle(_iconTex, false, _value => _studioConfigWindow.enabled = _value);
		}

		private void Update()
		{
			if (_cfgShortcut.Value.IsDown())
			{
				_studioConfigWindow.enabled = !_studioConfigWindow.enabled;
			}
		}
	}
}