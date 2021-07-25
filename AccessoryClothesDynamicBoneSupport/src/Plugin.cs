﻿using System.Collections;
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
	[BepInPlugin(GUID, Name, Version)]
	public class ACDBS : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.ACDBS";
		public const string Name = "Accessory Clothes Dynamic Bone Support";
		public const string Version = "1.0.0.0";

		private static ManualLogSource _logger;
		private static Harmony _hookInstance = null;

		private void Awake()
		{
			_logger = base.Logger;
			_hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
		}

		internal static class Hooks
		{
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
				if (!_pool.ContainsKey(__instance)) return;

				__instance.StartCoroutine(Wait());

				IEnumerator Wait()
				{
					yield return null;

					ChaControl _chaCtrl = __instance.GetComponentInParent<ChaControl>();
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