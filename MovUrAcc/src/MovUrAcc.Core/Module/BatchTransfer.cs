using System.Collections.Generic;
using System.Linq;

using AIChara;
using CharaCustom;

using HarmonyLib;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static void ActBatchTransfer(int start, int end, int newstart, int mode)
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

			if (newstart < 0)
				newstart = 0;

			if (mode == 0 && start == newstart)
			{
				Logger.LogMessage($"Start and new start are the same, nothing to do");
				btnLock = false;
				return;
			}

			if (start > end)
			{
				Logger.LogMessage($"End value must be greater than start value");
				btnLock = false;
				return;
			}

			int amount = newstart - start;
			Logger.LogDebug($"[mode: {mode}][start: {start + 1:00}][end: {end + 1:00}][newstart: {newstart + 1:00}][amount: {amount}]");

			List<QueueItem> Queue = new List<QueueItem>();
			int newSlot = newstart;
			if (mode == 0)
			{
				for (int i = start; i <= end; i++)
				{
					newSlot = i + amount;
					Queue.Add(new QueueItem(i, newSlot));
				}
			}
			else
			{
				ChaControl chaCtrl = CustomBase.Instance.chaCtrl;

				for (int i = start; i <= end; i++)
				{
					ChaFileAccessory.PartsInfo part = MoreAccessories.GetPartsInfo(i);
					if (part.type == 350)
						continue;
					if (mode == 1 && !IsHairAccessory(chaCtrl, i))
						continue;
					else if (mode == 2 && IsHairAccessory(chaCtrl, i))
						continue;

					Queue.Add(new QueueItem(i, newSlot));
					newSlot++;
				}
				newSlot -= 1;
			}

			if (Queue.Count == 0)
			{
				Logger.LogMessage($"Nothing to do");
				btnLock = false;
				return;
			}

			if (amount > 0)
			{
				if (newSlot > 19)
				{
					Logger.LogDebug($"Expand MoreAccessories slots from {nowAccCount} to {end + amount - 19}");

					for (int i = 0; i < (newSlot - 19 - nowAccCount); i++)
						Traverse.Create(MoreAccessories.PluginInstance).Method("AddOneSlot").GetValue();
				}

				Queue = Queue.OrderByDescending(x => x.srcSlot).ToList();
			}

			ProcessQueue(Queue);

			btnLock = false;

			RefreshMakerUI();
		}
	}
}
