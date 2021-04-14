using System;

using BepInEx;
using HarmonyLib;

using AIChara;

using KKAPI.Maker;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class AdditionalAccessoryControls
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("orange.spork.additionalaccessorycontrolsplugin", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo?.Instance;
				if (PluginInstance != null) Installed = true;
			}

			internal static object GetController(ChaControl chaCtrl)
			{
				if (!Installed) return null;

				Type AdditionalAccessoryControlsController = PluginInstance.GetType().Assembly.GetType("AdditionalAccessoryControls.AdditionalAccessoryControlsController");
				return chaCtrl.gameObject.GetComponent(AdditionalAccessoryControlsController);
			}

			internal static void ModifySetting(object pluginCtrl, int srcSlot, int dstSlot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("UpdateCharacterAccessories", new object[] { null, new AccessoryTransferEventArgs(srcSlot, dstSlot) }).GetValue();
				RemoveSetting(pluginCtrl, srcSlot);
			}

			internal static void RemoveSetting(object pluginCtrl, int slot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("UpdateCharacterAccessories", new object[] { null, new AccessorySlotEventArgs(slot) }).GetValue();
			}
		}
	}
}
