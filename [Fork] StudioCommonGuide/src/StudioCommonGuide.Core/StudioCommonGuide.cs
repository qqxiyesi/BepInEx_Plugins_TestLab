using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Specter.Assist
{
	public class StudioCommonGuide : MonoBehaviour
	{
		private GameObject currentGuideGo;
		private GameObject commGuideGo;
		private Rect guiRect = new Rect(Screen.width * 0.85f, Screen.height * 0.95f, Screen.width * 0.14f, 20f);
		private Dictionary<string, Vector3> guidePosTable = new Dictionary<string, Vector3>();
		private float currentFov;
		private float commonGuideRatio = Specter.ConfigRatio.Value;

		private bool VisibleTranslation = false;

		private void OnGUI()
		{
			if (!Specter.ConfigEnable.Value)
				return;
			if (!Singleton<Studio.Studio>.Instance.workInfo.visibleAxis || !(commGuideGo != null))
				return;
			if (IsOnMouse(guiRect))
				Singleton<Studio.Studio>.Instance.cameraCtrl.noCtrlCondition = () => true;
			else
				Singleton<Studio.Studio>.Instance.cameraCtrl.noCtrlCondition = () => false;
			GUILayout.BeginArea(guiRect);
			GUILayout.BeginHorizontal();
			commonGuideRatio = Mathf.Round(GUILayout.HorizontalSlider(commonGuideRatio, 1f, 10f));
			GUILayout.Label($"x{commonGuideRatio}", GUILayout.Width(40f));
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private bool IsOnMouse(Rect rect)
		{
			bool result = false;
			int num = 0;
			if (Input.mousePosition.x > rect.x)
				num |= 8;
			if (Input.mousePosition.x < rect.x + rect.width)
				num |= 4;
			if (Input.mousePosition.y < Screen.height - rect.y)
				num |= 2;
			if (Input.mousePosition.y > Screen.height - rect.y - rect.height)
				num |= 1;
			if ((num & 0xF) == 15)
				result = true;
			return result;
		}

		private GameObject GetCurrentGuide()
		{
			return (from n in guidePosTable
				select GameObject.Find("M Root(Clone)/" + n.Key) into m
				where m != null
				select m).FirstOrDefault();
		}

		private void CreateCommonGuide(GameObject currentGuide)
		{
			Destroy(commGuideGo);
			commGuideGo = Instantiate(currentGuide);
			commGuideGo.transform.position = Camera.main.ScreenToWorldPoint(guidePosTable[currentGuide.name]);
			commGuideGo.transform.parent = Camera.main.transform;
			commGuideGo.layer = 5;
		}

		private void CommonGuideMoveAtCorner(GameObject currentGuide)
		{
			if (commGuideGo != null && Camera.main != null)
				commGuideGo.transform.position = Camera.main.ScreenToWorldPoint(guidePosTable[currentGuide.name]);
		}

		private void Update()
		{
			if (!Studio.Studio.Instance.workInfo.visibleAxis)
				return;
			if (!Specter.ConfigEnable.Value)
			{
				if (commGuideGo != null)
					Destroy(commGuideGo);
				return;
			}
			currentGuideGo = GetCurrentGuide();
			if (commGuideGo == null && currentGuideGo == null)
				return;
			if (commGuideGo != null && currentGuideGo == null)
			{
				Destroy(commGuideGo);
				return;
			}
			if (VisibleTranslation != Studio.Studio.Instance.workInfo.visibleAxisTranslation)
			{
				VisibleTranslation = Studio.Studio.Instance.workInfo.visibleAxisTranslation;
				Destroy(commGuideGo);
			}
			if (commGuideGo == null)
				CreateCommonGuide(currentGuideGo);
			if (commGuideGo != null && commGuideGo.name != currentGuideGo.name + "(Clone)")
				CreateCommonGuide(currentGuideGo);
			if (commGuideGo != null)
			{
				commGuideGo.transform.rotation = currentGuideGo.transform.rotation;
				commGuideGo.transform.localScale = currentGuideGo.transform.localScale * commonGuideRatio;
			}
		}

		private void FixedUpdate()
		{
			currentFov = Camera.main.fieldOfView;
		}

		private void LateUpdate()
		{
			if (currentFov != Camera.main.fieldOfView)
				CommonGuideMoveAtCorner(currentGuideGo);
		}

		private void Start()
		{
			guidePosTable.Add("move", new Vector3(Screen.width * 0.89f, Screen.height * 0.15f, 6f));
			guidePosTable.Add("rotation", new Vector3(Screen.width * 0.9f, Screen.height * 0.15f, 6f));
			guidePosTable.Add("scale", new Vector3(Screen.width * 0.9f, Screen.height * 0.15f, 6f));
		}

		private void Awake()
		{
			VisibleTranslation = Studio.Studio.Instance.workInfo.visibleAxisTranslation;
		}
	}
}
