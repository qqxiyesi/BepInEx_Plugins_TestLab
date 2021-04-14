using AIChara;
using CharaCustom;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static void ActBatchRemove(int start, int end, int mode)
		{
			if (btnLock)
				return;
			btnLock = true;

			int nowAccCount = MoreAccessories.nowAccessories().Count;

			if (start < 0)
				start = 0;

			if (end < 0)
				end = nowAccCount + 19;
			else if (end > nowAccCount + 19)
				end = nowAccCount + 19;

			if (start > end)
			{
				Logger.LogMessage($"End value must be greater than start value");
				btnLock = false;
				return;
			}

			ChaControl chaCtrl = CustomBase.Instance.chaCtrl;
			object MEpluginCtrl = MaterialEditor.GetController(chaCtrl);
			object DBEpluginCtrl = DynamicBoneEditor.GetController(chaCtrl);
			object AACpluginCtrl = AdditionalAccessoryControls.GetController(chaCtrl);

			for (int i = start; i <= end; i++)
			{
				ChaFileAccessory.PartsInfo part = MoreAccessories.GetPartsInfo(i);
				if (part.type == 350)
					continue;
				if (mode == 1 && !IsHairAccessory(chaCtrl, i))
					continue;
				else if (mode == 2 && IsHairAccessory(chaCtrl, i))
					continue;

				MaterialEditor.RemoveSetting(MEpluginCtrl, i);
				DynamicBoneEditor.RemoveSetting(DBEpluginCtrl, i);
				AdditionalAccessoryControls.RemoveSetting(AACpluginCtrl, i);
				MoreAccessories.ResetPartsInfo(chaCtrl, i);
			}

			btnLock = false;

			RefreshMakerUI();
		}
	}
}
