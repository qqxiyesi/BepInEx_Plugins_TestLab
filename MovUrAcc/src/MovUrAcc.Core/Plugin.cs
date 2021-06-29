using System.Collections.Generic;

using AIChara;
using CharaCustom;
using UniRx;

using BepInEx;
using BepInEx.Logging;

using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;

namespace MovUrAcc
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
#if HS2
	[BepInProcess("HoneySelect2")]
#elif AI
	[BepInProcess("AI-Syoujyo")]
#endif
	public partial class MovUrAcc : BaseUnityPlugin
	{
#if HS2
		public const string GUID = "madevil.hs2.MovUrAcc";
#elif AI
		public const string GUID = "madevil.ai.MovUrAcc";
#endif
		public const string Name = "MovUrAcc";
		public const string Version = "1.5.2.0";

		internal static new ManualLogSource Logger;
		internal static bool btnLock = false;

		internal SidebarToggle SidebarToggleEnable { get; set; }
		internal MovUrAccUI ConfigWindow;

		private void Start()
		{
			Logger = base.Logger;

			MoreAccessories.InitSupport();
			MaterialEditor.InitSupport();
			DynamicBoneEditor.InitSupport();
			AdditionalAccessoryControls.InitSupport();

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				ConfigWindow = gameObject.AddComponent<MovUrAccUI>();

				SidebarToggleEnable = MakerAPI.AddSidebarControl(new SidebarToggle("MovUrAcc", false, this));
				SidebarToggleEnable.ValueChanged.Subscribe(value =>
				{
					if (ConfigWindow != null)
						ConfigWindow.enabled = value;
				});

				btnLock = false;
			};

			MakerAPI.MakerExiting += (sender, ev) =>
			{
				SidebarToggleEnable = null;
				Destroy(ConfigWindow);
			};
		}

		internal void CatTrimMoreacc(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ev.AddControl(new MakerText("Trim down unused MoreAccessories slots", category, this));

			MakerButton btnMoreAccApply = ev.AddControl(new MakerButton("Go", category, this));
			btnMoreAccApply.OnClick.AddListener(delegate
			{
				MoreAccessories.TrimUnusedSlots();
			});
		}

		internal class QueueItem
		{
			public int srcSlot { get; set; }
			public int dstSlot { get; set; }
			public QueueItem(int src, int dst)
			{
				srcSlot = src;
				dstSlot = dst;
			}
		}

		internal static void ProcessQueue(List<QueueItem> Queue)
		{
			ChaControl chaCtrl = CustomBase.Instance.chaCtrl;
			object MEpluginCtrl = MaterialEditor.GetController(chaCtrl);
			object AACpluginCtrl = AdditionalAccessoryControls.GetController(chaCtrl);
			object DBEpluginCtrl = DynamicBoneEditor.GetController(chaCtrl);

			foreach (QueueItem item in Queue)
			{
				Logger.LogDebug($"{item.srcSlot} -> {item.dstSlot}");

				MoreAccessories.ModifyPartsInfo(chaCtrl, item.srcSlot, item.dstSlot);
				MaterialEditor.ModifySetting(MEpluginCtrl, item.srcSlot, item.dstSlot);
				DynamicBoneEditor.ModifySetting(DBEpluginCtrl, item.srcSlot, item.dstSlot);
				AdditionalAccessoryControls.ModifySetting(AACpluginCtrl, item.srcSlot, item.dstSlot);
			}
		}

		internal static void RefreshMakerUI()
		{
			CustomBase.Instance.chaCtrl.Reload(false, true, true, true);
			CustomBase.Instance.updateCustomUI = true;
			CustomBase.Instance.ChangeAcsSlotName(-1);
			CustomBase.Instance.forceUpdateAcsList = true;
			CustomBase.Instance.updateCvsAccessory = true;
		}

		internal static bool IsHairAccessory(ChaControl chaCtrl, int slot)
		{
			try
			{
				CmpAccessory accessory = chaCtrl.GetAccessoryObject(slot)?.GetComponent<CmpAccessory>();
				if (accessory == null)
					return false;
				return accessory.gameObject?.GetComponent<CmpHair>() != null;
			}
			catch
			{
				return false;
			}
		}
	}
}
