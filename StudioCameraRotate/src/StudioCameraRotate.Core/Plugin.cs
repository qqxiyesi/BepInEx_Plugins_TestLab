using UnityEngine;

using BepInEx;
using BepInEx.Configuration;

namespace StudioCameraRotate
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInProcess(Constants.StudioProcessName)]
	public partial class StudioCameraRotate : BaseUnityPlugin
	{
		public const string GUID = "StudioCameraRotate";
		public const string Name = "Studio Camera Rotate";
		public const string Version = "1.0.0.0";

		private static ConfigEntry<KeyboardShortcut> CfgShortcut, CfgReverse;

		internal void Awake()
		{
			CfgShortcut = Config.Bind("Config", "Keyboard Shortcut", new KeyboardShortcut(KeyCode.F12));
			CfgReverse = Config.Bind("Config", "Keyboard Shortcut Reverse", new KeyboardShortcut(KeyCode.F12, KeyCode.LeftShift));
		}

		private void Update()
		{
			if (CfgShortcut.Value.IsDown())
			{
				float num = 90f;
				Studio.Studio.Instance.cameraCtrl.cameraAngle += new Vector3(0f, 0f, num);
			}
			else if (CfgReverse.Value.IsDown())
			{
				float num = -90f;
				Studio.Studio.Instance.cameraCtrl.cameraAngle += new Vector3(0f, 0f, num);
			}
		}
	}
}
