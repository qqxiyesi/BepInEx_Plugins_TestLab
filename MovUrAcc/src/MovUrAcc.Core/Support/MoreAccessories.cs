using System.Collections.Generic;
using System.Linq;

using BepInEx;
using HarmonyLib;

using AIChara;
using MessagePack;
using CharaCustom;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class MoreAccessories
		{
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.joan6694.illusionplugins.moreaccessories", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo.Instance;
			}

			internal static List<ChaFileAccessory.PartsInfo> nowAccessories()
			{
				return Traverse.Create(PluginInstance).Field("_makerAdditionalData").Field("parts").GetValue<List<ChaFileAccessory.PartsInfo>>();
			}

			internal static ChaFileAccessory.PartsInfo GetPartsInfo(int slot)
			{
				ChaControl chaCtrl = CustomBase.Instance.chaCtrl;
				if (slot < 20)
					return chaCtrl.nowCoordinate.accessory.parts[slot];
				else
					return nowAccessories().ElementAtOrDefault(slot - 20);
			}

			internal static void ModifyPartsInfo(ChaControl chaCtrl, int srcSlot, int dstSlot)
			{
				bool noShake = false;

				ChaFileAccessory.PartsInfo part = GetPartsInfo(srcSlot);
				byte[] bytes = MessagePackSerializer.Serialize(part);

				if (part.noShake)
					noShake = true;

				Logger.LogDebug($"[srcSlot: {srcSlot + 1:00}] -> [dstSlot: {dstSlot + 1:00}][noShake: {noShake}]");

				ResetPartsInfo(chaCtrl, srcSlot);

				if (dstSlot < 20)
					chaCtrl.nowCoordinate.accessory.parts[dstSlot] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);
				else
					nowAccessories()[dstSlot - 20] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);

				if (noShake)
					Traverse.Create(PluginInstance).Field("_makerCanvasAccessories").Field("tglNoShake").Property("isOn").SetValue(noShake);
			}

			internal static void ResetPartsInfo(ChaControl chaCtrl, int slot)
			{
				if (slot < 20)
					chaCtrl.nowCoordinate.accessory.parts[slot] = new ChaFileAccessory.PartsInfo();
				else
					nowAccessories()[slot - 20] = new ChaFileAccessory.PartsInfo();
			}

			internal static void TrimUnusedSlots()
			{
				if (btnLock)
					return;
				btnLock = true;

				List<ChaFileAccessory.PartsInfo> parts = nowAccessories();
				int n = parts.Count;

				if (n == 0)
				{
					Logger.LogMessage("No MoreAccessories slot, nothing to do");
					btnLock = false;
					return;
				}

				int i = n - 1;
				for (; i >= 0; i--)
				{
					if (parts[i].type > 350)
						break;
				}

				if (i == n - 1)
				{
					Logger.LogMessage("Last slot is being used, nothing to do");
					btnLock = false;
					return;
				}

				Logger.LogDebug($"[TrimUnusedSlots][{i + 1}][{n - 1 - i}]");
				parts.RemoveRange(i + 1, n - 1 - i);
				//Traverse.Create(PluginInstance).Field("_makerSlots").Method("RemoveRange", new object[] { i + 1, n - 1 - i }).GetValue();

				btnLock = false;
			}
		}
	}
}
