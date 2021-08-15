using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using KK_Plugins;

namespace ACDBS
{
	[BepInDependency("com.deathweasel.bepinex.accessoryclothes")]
	[BepInDependency("com.rclcircuit.bepinex.modboneimplantor")]
	[BepInDependency("marco.kkapi", "1.17")]
	[BepInPlugin(GUID, Name, Version)]
	public partial class ACDBS : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.ACDBS";
		public const string Name = "Accessory Clothes Dynamic Bone Support";
		public const string Version = "1.2.0.0";

		private static ManualLogSource _logger;
		private static Harmony _hooksInstance = null;

		private static Type DynamicBoneEditorUI = null;

		private void Awake()
		{
			_logger = base.Logger;
			_hooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.rclcircuit.bepinex.modboneimplantor", out PluginInfo _pluginInfo);
				Type AssignedAnotherWeightsHooks = _pluginInfo.Instance.GetType().Assembly.GetType("ModBoneImplantor.ModBoneImplantor+AssignedAnotherWeightsHooks");
				_hooksInstance.Patch(AssignedAnotherWeightsHooks.GetMethod("AssignWeightsAndImplantBones", AccessTools.all, null, new[] { typeof(AssignedAnotherWeights), typeof(GameObject), typeof(string), typeof(Bounds), typeof(Transform) }, null), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.AssignWeightsAndImplantBones_prefix)));
			}
		}

		internal static partial class Hooks
		{
			internal static void AssignWeightsAndImplantBones_prefix(ref GameObject obj)
			{
				if (obj.GetComponentsInParent<ChaAccessoryClothes>(true)?.FirstOrDefault() != null)
					obj = obj.GetComponentsInParent<ChaAccessoryClothes>(true).First().gameObject;
			}

			private static Dictionary<ChaAccessoryClothes, Dictionary<DynamicBone, string>> _pool = new Dictionary<ChaAccessoryClothes, Dictionary<DynamicBone, string>>();

			[HarmonyPrefix, HarmonyPatch(typeof(ChaAccessoryClothes), "Start")]
			private static void ChaAccessoryClothes_Start_Prefix(ChaAccessoryClothes __instance)
			{
				Dictionary<DynamicBone, string> dynamicBones = __instance.gameObject.GetComponents<DynamicBone>()?.Where(x => x.m_Root != null).ToDictionary(x => x, x => x.m_Root.name);
				if (dynamicBones?.Count > 0)
					_pool[__instance] = dynamicBones;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(ChaAccessoryClothes), "Start")]
			private static void ChaAccessoryClothes_Start_Postfix(ChaAccessoryClothes __instance)
			{
				if (__instance == null || !_pool.ContainsKey(__instance)) return;
				ChaControl _chaCtrl = __instance.GetComponentInParent<ChaControl>();

				_chaCtrl.StartCoroutine(Wait());

				IEnumerator Wait()
				{
					yield return null;

					foreach (KeyValuePair<DynamicBone, string> _item in _pool[__instance])
					{
						_item.Key.m_Root = _chaCtrl.GetComponentsInChildren<Transform>(true)?.FirstOrDefault(x => x.name == _item.Value);
						if (_item.Key.m_Root == null) continue;
						_item.Key.SetupParticles();
					}
					_pool.Remove(__instance);
				}
			}
		}
	}
}
