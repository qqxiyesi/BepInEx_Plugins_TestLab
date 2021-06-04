﻿using System.Linq;
using HarmonyLib;
using Studio;
using UnityEngine;
using UnityEngine.UI;
using Sideloader.AutoResolver;

namespace DefaultParamEditor.Koikatu
{
	internal class SceneParam
	{
		private static ParamData.SceneData _sceneData;

		public static void Init(ParamData.SceneData data)
		{
			_sceneData = data;
			Harmony.CreateAndPatchAll(typeof(Hooks));
		}

		public static void Save()
		{
			var sceneInfo = Studio.Studio.Instance.sceneInfo;
			var systemButtonCtrl = Studio.Studio.Instance.systemButtonCtrl;

			if (sceneInfo != null && systemButtonCtrl != null)
			{
				_sceneData.aceNo = sceneInfo.aceNo;
				_sceneData.aceBlend = sceneInfo.aceBlend;

				var aoe = Traverse.Create(systemButtonCtrl).Field("amplifyOcculusionEffectInfo").Property("aoe").GetValue<AmplifyOcclusionEffect>();
				_sceneData.enableAOE = (bool)aoe.GetType().GetProperty("enabled").GetValue(aoe, null);

				_sceneData.aoeColor = sceneInfo.aoeColor;
				_sceneData.aoeRadius = sceneInfo.aoeRadius;
				_sceneData.enableBloom = sceneInfo.enableBloom;
				_sceneData.bloomIntensity = sceneInfo.bloomIntensity;
				_sceneData.bloomThreshold = sceneInfo.bloomThreshold;
				_sceneData.bloomBlur = sceneInfo.bloomBlur;
				_sceneData.enableDepth = sceneInfo.enableDepth;
				_sceneData.depthFocalSize = sceneInfo.depthFocalSize;
				_sceneData.depthAperture = sceneInfo.depthAperture;
				_sceneData.enableVignette = sceneInfo.enableVignette;
				_sceneData.enableFog = sceneInfo.enableFog;
				_sceneData.fogColor = sceneInfo.fogColor;
				_sceneData.fogHeight = sceneInfo.fogHeight;
				_sceneData.fogStartDistance = sceneInfo.fogStartDistance;
				_sceneData.enableSunShafts = sceneInfo.enableSunShafts;
				_sceneData.sunThresholdColor = sceneInfo.sunThresholdColor;
				_sceneData.sunColor = sceneInfo.sunColor;

				var toggleEnable = Traverse.Create(systemButtonCtrl).Field("selfShadowInfo").Field("toggleEnable").GetValue<Toggle>();
				_sceneData.enableShadow = (bool)toggleEnable.GetType().GetProperty("isOn").GetValue(toggleEnable, null);

				_sceneData.rampG = sceneInfo.rampG;
				_sceneData.ambientShadowG = sceneInfo.ambientShadowG;
				_sceneData.lineWidthG = sceneInfo.lineWidthG;
				_sceneData.lineColorG = sceneInfo.lineColorG;
				_sceneData.ambientShadow = sceneInfo.ambientShadow;
				_sceneData.cameraNearClip = Camera.main.nearClipPlane;
				_sceneData.fov = Studio.Studio.Instance.cameraCtrl.fieldOfView;

				if (sceneInfo.aceNo >= UniversalAutoResolver.BaseSlotID)
				{
					StudioResolveInfo info = UniversalAutoResolver.LoadedStudioResolutionInfo.FirstOrDefault(x => x.LocalSlot == sceneInfo.aceNo);
					if (info != null)
					{
						_sceneData.aceNo = info.Slot;
						_sceneData.aceGUID = info.GUID;
						_sceneData.aceNo_GUID = info.GUID; // to match upstreams format
					}
				}
				if (sceneInfo.rampG >= UniversalAutoResolver.BaseSlotID)
				{
					ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo(ChaListDefine.CategoryNo.mt_ramp, sceneInfo.rampG);
					if (info != null)
					{
						_sceneData.rampG = info.Slot;
						_sceneData.rampGUID = info.GUID;
						_sceneData.rampG_GUID = info.GUID; // to match upstreams format
					}
				}

				_sceneData.saved = true;
				Log.Message("Default scene settings saved");
			}
		}

		public static void Reset()
		{
			_sceneData.saved = false;
		}

		public static void LoadDefaults()
		{
			if (_sceneData.saved && Studio.Studio.Instance)
			{
				Log.Info("Loading scene defaults");
				SetSceneInfoValues(Studio.Studio.Instance.sceneInfo);
				Studio.Studio.Instance.systemButtonCtrl.UpdateInfo();
			}
		}

		private static void SetSceneInfoValues(SceneInfo sceneInfo)
		{
			sceneInfo.aceNo = _sceneData.aceNo;
			sceneInfo.aceBlend = _sceneData.aceBlend;
			sceneInfo.enableAOE = _sceneData.enableAOE;
			sceneInfo.aoeColor = _sceneData.aoeColor;
			sceneInfo.aoeRadius = _sceneData.aoeRadius;
			sceneInfo.enableBloom = _sceneData.enableBloom;
			sceneInfo.bloomIntensity = _sceneData.bloomIntensity;
			sceneInfo.bloomThreshold = _sceneData.bloomThreshold;
			sceneInfo.bloomBlur = _sceneData.bloomBlur;
			sceneInfo.enableDepth = _sceneData.enableDepth;
			sceneInfo.depthFocalSize = _sceneData.depthFocalSize;
			sceneInfo.depthAperture = _sceneData.depthAperture;
			sceneInfo.enableVignette = _sceneData.enableVignette;
			sceneInfo.enableFog = _sceneData.enableFog;
			sceneInfo.fogColor = _sceneData.fogColor;
			sceneInfo.fogHeight = _sceneData.fogHeight;
			sceneInfo.fogStartDistance = _sceneData.fogStartDistance;
			sceneInfo.enableSunShafts = _sceneData.enableSunShafts;
			sceneInfo.sunThresholdColor = _sceneData.sunThresholdColor;
			sceneInfo.sunColor = _sceneData.sunColor;
			sceneInfo.enableShadow = _sceneData.enableShadow;
			sceneInfo.rampG = _sceneData.rampG;
			sceneInfo.ambientShadowG = _sceneData.ambientShadowG;
			sceneInfo.lineWidthG = _sceneData.lineWidthG;
			sceneInfo.lineColorG = _sceneData.lineColorG;
			sceneInfo.ambientShadow = _sceneData.ambientShadow;

			if (!_sceneData.aceGUID.IsNullOrEmpty())
			{
				StudioResolveInfo info = UniversalAutoResolver.LoadedStudioResolutionInfo.FirstOrDefault(x => x.Slot == _sceneData.aceNo && x.GUID == _sceneData.aceGUID);
				if (info != null)
					sceneInfo.aceNo = info.LocalSlot;
			}
			else if (!_sceneData.aceNo_GUID.IsNullOrEmpty()) // to match upstreams format
			{
				StudioResolveInfo info = UniversalAutoResolver.LoadedStudioResolutionInfo.FirstOrDefault(x => x.Slot == _sceneData.aceNo && x.GUID == _sceneData.aceNo_GUID);
				if (info != null)
					sceneInfo.aceNo = info.LocalSlot;
			}
			if (!_sceneData.rampGUID.IsNullOrEmpty())
			{
				ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo(_sceneData.rampG, ChaListDefine.CategoryNo.mt_ramp, _sceneData.rampGUID);
				if (info != null)
					sceneInfo.rampG = info.LocalSlot;
			}
			else if (!_sceneData.rampG_GUID.IsNullOrEmpty()) // to match upstreams format
			{
				ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo(_sceneData.rampG, ChaListDefine.CategoryNo.mt_ramp, _sceneData.rampG_GUID);
				if (info != null)
					sceneInfo.rampG = info.LocalSlot;
			}
		}

		private class Hooks
		{
			[HarmonyPostfix, HarmonyPatch(typeof(SceneInfo), nameof(SceneInfo.Init))]
			public static void HarmonyPatch_SceneInfo_Init(SceneInfo __instance)
			{
				if (_sceneData.saved)
				{
					Log.Debug("Loading defaults for a new scene");
					SetSceneInfoValues(__instance);
					Camera.main.nearClipPlane = _sceneData.cameraNearClip;
					Studio.Studio.Instance.cameraCtrl.fieldOfView = _sceneData.fov;
				}
			}
		}
	}
}
