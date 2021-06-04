using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using MessagePack;
using ChaCustom;

using KKAPI.Maker;
#if KK
using MoreAccessoriesKOI;
#endif
namespace ParentSwitch
{
	public partial class ParentSwitch
	{
		internal static void ChangeParent(int slotNo, string parentStr)
		{
			ChaControl chaCtrl = CustomBase.Instance.chaCtrl;
#if KK
			List<ChaFileAccessory.PartsInfo> nowAccessories = _accessoriesByChar.RefTryGetValue<MoreAccessories.CharAdditionalData>(_chaCtrl.chaFile)?.nowAccessories ?? new List<ChaFileAccessory.PartsInfo>();
#else
			List<ChaFileAccessory.PartsInfo> nowAccessories = new List<ChaFileAccessory.PartsInfo>();
#endif
			ChaFileAccessory.PartsInfo part = new ChaFileAccessory.PartsInfo();
			if (slotNo < 20)
				part = chaCtrl.nowCoordinate.accessory.parts[slotNo];
			else
				part = nowAccessories.ElementAtOrDefault(slotNo - 20);

			if (part == null || part.type == 120 || part.parentKey == parentStr) return;

			_logger.LogInfo($"Changing Slot{slotNo + 1:00} from {part.parentKey} to {parentStr}");

			GameObject gameObject = chaCtrl.GetAccessoryObject(slotNo);
			if (gameObject == null)
			{
				_logger.LogMessage($"Skip Slot{slotNo + 1:00} because it doesn't have a valid GameObject");
				return;
			}
			ChaReference.RefObjKey key = (ChaReference.RefObjKey) Enum.Parse(typeof(ChaReference.RefObjKey), parentStr);
			GameObject referenceInfo = chaCtrl.GetReferenceInfo(key);
			if (referenceInfo == null)
			{
				_logger.LogMessage($"Skip Slot{slotNo + 1:00} because invalid parent");
				return;
			}

			Vector3 parentScale = gameObject.transform.parent.localScale;

			Transform n_move = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name == "N_move").FirstOrDefault();
			if (n_move == null)
			{
				_logger.LogMessage($"Skip Slot{slotNo + 1:00} because it doesn't have a valid N_move Transform");
				return;
			}
			Vector3 position = n_move.position;
			Quaternion rotation = n_move.rotation;

			Transform n_move2 = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name == "N_move2").FirstOrDefault();
			Vector3 position2 = n_move2 == null ? Vector3.zero : n_move2.position;
			Quaternion rotation2 = n_move2 == null ? Quaternion.identity : n_move2.rotation;
			Vector3 scale2 = n_move2 == null ? Vector3.one : n_move2.localScale;

			gameObject.transform.SetParent(referenceInfo.transform, true);
			Vector3 referenceScale = referenceInfo.transform.localScale;
			Vector3 parentScaleRate = _cfgDebugRatio.Value ? new Vector3(parentScale.x / referenceScale.x, parentScale.y / referenceScale.y, parentScale.z / referenceScale.z) : Vector3.one;

			n_move.localScale = new Vector3(n_move.localScale.x * gameObject.transform.localScale.x * parentScaleRate.x, n_move.localScale.y * gameObject.transform.localScale.y * parentScaleRate.y, n_move.localScale.z * gameObject.transform.localScale.z * parentScaleRate.z);

			bool underN = false;

			if (n_move2 != null)
            {
				underN = n_move.GetComponentsInChildren<Transform>().Any(x => x.name == "N_move2"); // N_move2 under N_move case
				if (underN)
					n_move2.localScale = scale2;
				else
					n_move2.localScale = new Vector3(n_move2.localScale.x * gameObject.transform.localScale.x * parentScaleRate.x, n_move2.localScale.y * gameObject.transform.localScale.y * parentScaleRate.y, n_move2.localScale.z * gameObject.transform.localScale.z * parentScaleRate.z);
			}

			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;

			part.parentKey = parentStr;
			part.partsOfHead = ChaAccessoryDefine.CheckPartsOfHead(parentStr);

			n_move.position = position;
			n_move.rotation = rotation;
			part.addMove[0, 0] = new Vector3(float.Parse((n_move.localPosition.x * 100f).ToString("f2")), float.Parse((n_move.localPosition.y * 100f).ToString("f2")), float.Parse((n_move.localPosition.z * 100f).ToString("f2")));
			part.addMove[0, 1] = new Vector3((float.Parse(n_move.localEulerAngles.x.ToString("f2")) + 360f) % 360f, (float.Parse(n_move.localEulerAngles.y.ToString("f2")) + 360f) % 360f, (float.Parse(n_move.localEulerAngles.z.ToString("f2")) + 360f) % 360f);
			part.addMove[0, 2] = new Vector3(float.Parse(n_move.localScale.x.ToString("f2")), float.Parse(n_move.localScale.y.ToString("f2")), float.Parse(n_move.localScale.z.ToString("f2")));

			if (n_move2 != null && !underN)
			{
				n_move2.position = position2;
				n_move2.rotation = rotation2;
				part.addMove[1, 0] = new Vector3(float.Parse((n_move2.localPosition.x * 100f).ToString("f2")), float.Parse((n_move2.localPosition.y * 100f).ToString("f2")), float.Parse((n_move2.localPosition.z * 100f).ToString("f2")));
				part.addMove[1, 1] = new Vector3((float.Parse(n_move2.localEulerAngles.x.ToString("f2")) + 360f) % 360f, (float.Parse(n_move2.localEulerAngles.y.ToString("f2")) + 360f) % 360f, (float.Parse(n_move2.localEulerAngles.z.ToString("f2")) + 360f) % 360f);
				part.addMove[1, 2] = new Vector3(float.Parse(n_move2.localScale.x.ToString("f2")), float.Parse(n_move2.localScale.y.ToString("f2")), float.Parse(n_move2.localScale.z.ToString("f2")));
			}

			if (slotNo < 20)
			{
				byte[] bytes = MessagePackSerializer.Serialize(part);
				chaCtrl.chaFile.coordinate[chaCtrl.fileStatus.coordinateType].accessory.parts[slotNo] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);
			}
		}

	}
}
