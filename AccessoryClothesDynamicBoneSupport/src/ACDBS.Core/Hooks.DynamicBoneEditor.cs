using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using BepInEx;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace ACDBS
{
	public partial class ACDBS
	{
		private void Start()
		{
			BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("madevil.kk.AAAPK", out PluginInfo _pluginInfoAAAPK);

			BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.dynamicboneeditor", out PluginInfo _pluginInfoDBE);
			if (_pluginInfoDBE?.Instance != null && _pluginInfoAAAPK?.Instance == null)
			{
				DynamicBoneEditorUI = _pluginInfoDBE.Instance.GetType().Assembly.GetType("KK_Plugins.DynamicBoneEditor.UI");
				_hooksInstance.Patch(DynamicBoneEditorUI.GetMethod("ShowUI", AccessTools.all, null, new[] { typeof(int) }, null), transpiler: new HarmonyMethod(typeof(Hooks), nameof(Hooks.UI_ShowUI_Transpiler)));
				_hooksInstance.Patch(DynamicBoneEditorUI.GetMethod("ToggleButtonVisibility", AccessTools.all), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.UI_ToggleButtonVisibility_Prefix)));

				Type CharaController = _pluginInfoDBE.Instance.GetType().Assembly.GetType("KK_Plugins.DynamicBoneEditor.CharaController");
				_hooksInstance.Patch(AccessTools.Method(AccessTools.Inner(CharaController, "<ApplyData>d__12"), "MoveNext"), transpiler: new HarmonyMethod(typeof(Hooks), nameof(Hooks.CharaController_ApplyData_Transpiler)));
			}
		}

		internal static GameObject GetObjAccessory(ChaControl _chaCtrl, int _slotIndex)
		{
			return _chaCtrl.GetComponentsInChildren<ListInfoComponent>(true)?.FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name == $"ca_slot{_slotIndex:00}")?.gameObject;
		}

		internal static partial class Hooks
		{
			internal static IEnumerable<CodeInstruction> CharaController_ApplyData_Transpiler(IEnumerable<CodeInstruction> _instructions)
			{
				MethodInfo _getComponentsInChildrenMethod = AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponentsInChildren), generics: new Type[] { typeof(DynamicBone) });
				if (_getComponentsInChildrenMethod == null)
				{
					_logger.LogError("Failed to get methodinfo for UnityEngine.GameObject.GetComponentsInChildren<DynamicBone>, CharaController_ApplyData_Transpiler will not patch");
					return _instructions;
				}

				CodeMatcher _codeMatcher = new CodeMatcher(_instructions)
					.MatchForward(useEnd: false,
						new CodeMatch(OpCodes.Callvirt, operand: _getComponentsInChildrenMethod),
						new CodeMatch(OpCodes.Stloc_S))
					.Advance(1)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Hooks), nameof(CharaController_ApplyData_Method))));

				_codeMatcher.ReportFailure(MethodBase.GetCurrentMethod(), error => _logger.LogError(error));
#if DEBUG
				System.IO.File.WriteAllLines($"{nameof(CharaController_ApplyData_Transpiler)}.txt", _codeMatcher.Instructions().Select(x => x.ToString()).ToArray());
#endif
				return _codeMatcher.Instructions();
			}

			internal static DynamicBone[] CharaController_ApplyData_Method(DynamicBone[] _getFromStack)
			{
				List<DynamicBone> _result = _getFromStack == null ? new List<DynamicBone>() : _getFromStack.Where(x => x.m_Root != null).ToList();

				if (_result?.Count == 0)
					return _result.ToArray();

				_result.RemoveAll(x => x.m_Root == null || x.m_Root.name.StartsWith("cf_j_sk_") || x.m_Root.name.StartsWith("cf_d_sk_"));
				return _result.ToArray();
			}

			internal static IEnumerable<CodeInstruction> UI_ShowUI_Transpiler(IEnumerable<CodeInstruction> _instructions)
			{
				MethodInfo _toListMethod = AccessTools.Method(typeof(Enumerable), nameof(Enumerable.ToList), generics: new Type[] { typeof(DynamicBone) });
				if (_toListMethod == null)
				{
					_logger.LogError("Failed to get methodinfo for System.Linq.Enumerable.ToList<DynamicBone>, UI_ShowUI_Transpiler will not patch");
					return _instructions;
				}

				CodeMatcher _codeMatcher = new CodeMatcher(_instructions)
					.MatchForward(useEnd: false,
						new CodeMatch(OpCodes.Call, operand: _toListMethod),
						new CodeMatch(OpCodes.Stloc_2),
						new CodeMatch(OpCodes.Ldloc_2))
					.Advance(1)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Hooks), nameof(UI_ShowUI_Method))));

				_codeMatcher.ReportFailure(MethodBase.GetCurrentMethod(), error => _logger.LogError(error));
#if DEBUG
				System.IO.File.WriteAllLines($"{nameof(UI_ShowUI_Transpiler)}.txt", _codeMatcher.Instructions().Select(x => x.ToString()).ToArray());
#endif
				return _codeMatcher.Instructions();
			}

			internal static List<DynamicBone> UI_ShowUI_Method(List<DynamicBone> _getFromStack)
			{
				List<DynamicBone> _result = _getFromStack.ToList();

				if (_result.Count == 0)
					return _getFromStack;

				_result.RemoveAll(x => x.m_Root == null || x.m_Root.name.StartsWith("cf_j_sk_") || x.m_Root.name.StartsWith("cf_d_sk_"));
				return _result;
			}

			internal static bool UI_ToggleButtonVisibility_Prefix()
			{
				if (!MakerAPI.InsideMaker)
					return true;

				MakerButton DynamicBoneEditorButton = Traverse.Create(DynamicBoneEditorUI).Field("DynamicBoneEditorButton").GetValue<MakerButton>();
				if (DynamicBoneEditorButton == null)
					return true;

				GameObject _ca_slot = GetObjAccessory(ChaCustom.CustomBase.Instance.chaCtrl, AccessoriesApi.SelectedMakerAccSlot);
				if (_ca_slot == null)
					return true;

				List<DynamicBone> _result = _ca_slot.GetComponentsInChildren<DynamicBone>(true)?.Where(x => x.m_Root != null && !x.m_Root.name.StartsWith("cf_j_sk_") && !x.m_Root.name.StartsWith("cf_d_sk_")).ToList();

				DynamicBoneEditorButton.Visible.OnNext(_result?.Count > 0);

				return false;
			}
		}
	}
}
