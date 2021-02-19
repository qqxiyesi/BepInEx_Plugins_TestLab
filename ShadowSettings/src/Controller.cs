using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UniRx;
using Studio;

using ExtensibleSaveFormat;

using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;

namespace ShadowSettings
{
	public partial class ShadowSettings
	{
		public class ShadowSettingsController : SceneCustomFunctionController
		{
			private void Start() => SceneManager.sceneLoaded += InitStudioUI;

			protected override void OnSceneSave()
			{
				PluginData data = new PluginData();
				data.data["ShadowQuality"] = uiShadowQuality.value;
				data.data["ShadowResolution"] = uiShadowResolution.value;
				data.data["ShadowProjection"] = uiShadowProjection.value;
				data.data["ShadowCascades"] = listShadowCascadesOptions[uiShadowCascades.value];
				data.data["ShadowDistance"] = uiShadowDistance.value;
				data.data["ShadowNearPlaneOffset"] = uiShadowNearPlaneOffset.value;
				SetExtendedData(data);
			}

			protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
			{
				if (operation == SceneOperationKind.Load)
				{
					PluginData data = GetExtendedData();
					if (data?.data == null)
						ResetAll();
					else
					{
						if (data.data.TryGetValue("ShadowQuality", out var loadShadowQuality) && loadShadowQuality != null)
							uiShadowQuality.value = (int) loadShadowQuality;
						if (data.data.TryGetValue("ShadowResolution", out var loadShadowResolution) && loadShadowResolution != null)
							uiShadowResolution.value = (int) loadShadowResolution;
						if (data.data.TryGetValue("ShadowProjection", out var loadShadowProjection) && loadShadowProjection != null)
							uiShadowProjection.value = (int) loadShadowProjection;
						if (data.data.TryGetValue("ShadowCascades", out var loadShadowCascades) && loadShadowCascades != null)
							uiShadowCascades.value = listShadowCascadesOptions.IndexOf((string) loadShadowCascades);
						if (data.data.TryGetValue("ShadowDistance", out var loadShadowDistance) && loadShadowDistance != null)
							uiShadowDistance.value = (float) loadShadowDistance;
						if (data.data.TryGetValue("ShadowNearPlaneOffset", out var loadShadowNearPlaneOffset) && loadShadowNearPlaneOffset != null)
							uiShadowNearPlaneOffset.value = (float) loadShadowNearPlaneOffset;
						RefreshDropdown();
					}
				}
				else if (operation == SceneOperationKind.Clear)
					ResetAll();
			}

			internal void ResetAll()
			{
				uiShadowQuality.value = (int) ShadowQuality.Value;
				uiShadowResolution.value = (int) ShadowResolution.Value;
				uiShadowProjection.value = (int) ShadowProjection.Value;
				uiShadowCascades.value = listShadowCascadesOptions.IndexOf(ShadowCascades.Value.ToString());
				uiShadowDistance.value = ShadowDistance.Value;
				uiShadowNearPlaneOffset.value = ShadowNearPlaneOffset.Value;
				RefreshDropdown();
			}

			internal void RefreshDropdown()
			{
				uiShadowQuality.RefreshShownValue();
				uiShadowResolution.RefreshShownValue();
				uiShadowProjection.RefreshShownValue();
				uiShadowCascades.RefreshShownValue();
			}

			internal class UniqueDropdownEventHandler : MonoBehaviour, IPointerClickHandler
			{
				private Dropdown owner;
				internal List<Dropdown> group;

				private void Awake()
				{
					owner = gameObject.GetComponent<Dropdown>();
				}

				public void OnPointerClick(PointerEventData pointerEventData)
				{
					foreach (Dropdown x in group)
						if (x != owner)
							x.Hide();
				}
			}

			internal void InitStudioUI(Scene s, LoadSceneMode lsm)
			{
				if (s.name != "Studio") return;
				SceneManager.sceneLoaded -= InitStudioUI;

				SceneEffectsCategory menu = new SceneEffectsCategory("ShadowSettings", "Shadow Settings");
				List<Dropdown> group = new List<Dropdown>();

				{
					List<string> options = Enum.GetNames(typeof(ShadowQuality)).ToList();
					uiShadowQuality = menu.AddDropdonw("ShadowQuality", "Quality", options);
					uiShadowQuality.onValueChanged.AddListener(delegate (int value)
					{
						QualitySettings.shadows = (ShadowQuality) value;
					});
				}

				{
					List<string> options = Enum.GetNames(typeof(ShadowResolution)).ToList();
					uiShadowResolution = menu.AddDropdonw("ShadowResolution", "Resolution", options);
					uiShadowResolution.onValueChanged.AddListener(delegate (int value)
					{
						QualitySettings.shadowResolution = (ShadowResolution) value;
					});
				}

				{
					List<string> options = Enum.GetNames(typeof(ShadowProjection)).ToList();
					uiShadowProjection = menu.AddDropdonw("ShadowProjection", "Projection", options);
					uiShadowProjection.onValueChanged.AddListener(delegate (int value)
					{
						QualitySettings.shadowProjection = (ShadowProjection) value;
					});
				}

				{
					List<string> options = listShadowCascadesOptions;
					uiShadowCascades = menu.AddDropdonw("ShadowCascades", "Cascades", options);
					uiShadowCascades.onValueChanged.AddListener(delegate (int value)
					{
						QualitySettings.shadowCascades = int.Parse(options[value]);
					});
				}

				group.Add(uiShadowQuality);
				group.Add(uiShadowResolution);
				group.Add(uiShadowProjection);
				group.Add(uiShadowCascades);
				foreach (Dropdown x in group)
					x.gameObject.AddComponent<UniqueDropdownEventHandler>().group = group;

				uiShadowDistance = menu.AddSlider("ShadowDistance", "Distance", ShadowDistance.Value, 0f, 100f);
				uiShadowDistance.onValueChanged.AddListener(delegate (float value)
				{
					QualitySettings.shadowDistance = value;
				});

				uiShadowNearPlaneOffset = menu.AddSlider("ShadowNearPlaneOffset", "Near Plane Offset", ShadowNearPlaneOffset.Value, 0f, 4f);
				uiShadowNearPlaneOffset.onValueChanged.AddListener(delegate (float value)
				{
					QualitySettings.shadowNearPlaneOffset = value;
				});

				ResetAll();
			}
		}
	}
}
