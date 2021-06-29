using UnityEngine;

using BepInEx;
using BepInEx.Configuration;

using KKAPI;
using KKAPI.Studio.SaveLoad;

namespace StartupCameraRecall
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInProcess(Constants.StudioProcessName)]
	[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
	[BepInIncompatibility("com.gebo.BepInEx.studioinitialcamera")]
	public partial class StartupCameraRecall : BaseUnityPlugin
	{
		public const string GUID = "StartupCameraRecall";
		public const string Name = "Startup Camera Recall";
		public const string Version = "1.0.0.0";

		private static ConfigEntry<KeyboardShortcut> Shortcut;
		private static Studio.CameraControl.CameraData StartupCamera = new Studio.CameraControl.CameraData();

		internal void Awake()
		{
			Shortcut = Config.Bind("Config", "Keyboard Shortcut", new KeyboardShortcut(KeyCode.BackQuote));
			StudioSaveLoadApi.SceneLoad += OnSceneLoad;
		}

		private void Update()
		{
			if (Shortcut.Value.IsDown())
				Studio.Studio.Instance.cameraCtrl.Import(StartupCamera);
		}

		private static void OnSceneLoad(object sender, SceneLoadEventArgs e)
		{
			if (e.Operation == SceneOperationKind.Load)
				StartupCamera.Copy(Studio.Studio.Instance.sceneInfo.cameraSaveData);
		}
	}
}
