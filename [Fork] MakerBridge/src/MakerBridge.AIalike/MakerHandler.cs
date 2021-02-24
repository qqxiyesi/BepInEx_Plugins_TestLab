using CharaCustom;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KeelPlugins
{
	internal class MakerHandler : HandlerCore
	{
		private void Start()
		{
			watcher = CharaCardWatcher.Watch(MakerBridge.OtherCardPath, LoadChara);
			watcher.EnableRaisingEvents = true;
		}

		private void Update()
		{
			if (MakerBridge.SendChara.Value.IsDown())
				SaveCharacter(MakerBridge.MakerCardPath);
		}

		private void SaveCharacter(string path)
		{
			var customBase = CustomBase.Instance;
			if (customBase)
			{
				var empty = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				empty.SetPixel(0, 0, Color.black);
				empty.Apply();

				var charFile = customBase.chaCtrl.chaFile;
				charFile.pngData = empty.EncodeToPNG();

				customBase.chaCtrl.chaFile.SaveCharaFile(path, byte.MaxValue, false);
			}
		}

		private void LoadChara(string path)
		{
			var cvsCharaLoad = FindObjectOfType<CvsO_CharaLoad>();
			var traverse = Traverse.Create(cvsCharaLoad);
			var charaLoadWin = traverse.Field("charaLoadWin");
			var tglLoadOption = charaLoadWin.Field("tglLoadOption").GetValue<Toggle[]>();
			int num = 0;

			if (tglLoadOption != null)
			{
				if (tglLoadOption[0].isOn) num |= 1;
				if (tglLoadOption[1].isOn) num |= 2;
				if (tglLoadOption[2].isOn) num |= 4;
				if (tglLoadOption[3].isOn) num |= 8;
				if (tglLoadOption[4].isOn) num |= 16;
			}

			charaLoadWin.GetValue<CustomCharaWindow>().onClick03.Invoke(new CustomCharaFileInfo { FullPath = path }, num);
		}
	}
}