using System.Collections.Generic;

using AIChara;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static void ActPacking()
		{
			if (btnLock)
				return;
			btnLock = true;

			List<QueueItem> Queue = new List<QueueItem>();
			int nowAccCount = MoreAccessories.nowAccessories().Count;
			int dstSlot = 0;

			for (int srcSlot = 0; srcSlot < (20 + nowAccCount); srcSlot++)
			{
				ChaFileAccessory.PartsInfo part = MoreAccessories.GetPartsInfo(srcSlot);
				if (part.type == 350)
					continue;

				if (srcSlot != dstSlot)
					Queue.Add(new QueueItem(srcSlot, dstSlot));

				dstSlot++;
			}

			if (Queue.Count == 0)
			{
				Logger.LogMessage("Nothing to do");
				btnLock = false;
				return;
			}

			ProcessQueue(Queue);

			RefreshMakerUI();

			btnLock = false;
		}
	}
}
