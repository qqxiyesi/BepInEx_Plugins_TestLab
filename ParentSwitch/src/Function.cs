using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using MessagePack;
using ChaCustom;

using KKAPI.Maker;

using MoreAccessoriesKOI;

namespace ParentSwitch
{
	public partial class ParentSwitch
	{
		internal static void ChangeParent(int slotNo, string parentStr)
		{
			ChaControl chaCtrl = _chaCtrl;
			MoreAccessories.CharAdditionalData additionalData = _accessoriesByChar[chaCtrl.chaFile];

			ChaFileAccessory.PartsInfo part = new ChaFileAccessory.PartsInfo();
			if (slotNo < 20)
				part = chaCtrl.nowCoordinate.accessory.parts[slotNo];
			else
				part = additionalData.nowAccessories.ElementAtOrDefault(slotNo - 20);

			if (part == null || part.type == 120 || part.parentKey == parentStr) return;

			_logger.LogInfo($"Changing Slot{slotNo + 1:00} from {part.parentKey} to {parentStr}");

			GameObject gameObject = chaCtrl.GetAccessoryObject(slotNo);
			if (gameObject == null)
			{
				_logger.LogMessage($"Skip Slot{slotNo + 1:00} because it doesn't have a valid GameObject");
				return;
			}
			ChaReference.RefObjKey key = (ChaReference.RefObjKey)Enum.Parse(typeof(ChaReference.RefObjKey), parentStr);
			GameObject referenceInfo = chaCtrl.GetReferenceInfo(key);
			gameObject.transform.SetParent(referenceInfo.transform, true);

			Transform n_move = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name == "N_move").FirstOrDefault();
			if (n_move == null)
            {
				_logger.LogMessage($"Skip Slot{slotNo + 1:00} because it doesn't have a valid N_move Transform");
				return;
			}
			Vector3 position = n_move.position;
			Quaternion rotation = n_move.rotation;
			n_move.localScale = new Vector3(n_move.localScale.x * gameObject.transform.localScale.x, n_move.localScale.y * gameObject.transform.localScale.y, n_move.localScale.z * gameObject.transform.localScale.z);

			Transform n_move2 = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name == "N_move2").FirstOrDefault();
			Vector3 position2 = n_move2 == null ? Vector3.zero : n_move2.position;
			Quaternion rotation2 = n_move2 == null ? Quaternion.identity : n_move2.rotation;
			if (n_move2 != null)
				n_move2.localScale = new Vector3(n_move2.localScale.x * gameObject.transform.localScale.x, n_move2.localScale.y * gameObject.transform.localScale.y, n_move2.localScale.z * gameObject.transform.localScale.z);

			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;

			part.parentKey = parentStr;
			part.partsOfHead = ChaAccessoryDefine.CheckPartsOfHead(parentStr);

			n_move.position = position;
			n_move.rotation = rotation;
			part.addMove[0, 0] = new Vector3(float.Parse((n_move.localPosition.x * 100f).ToString("f1")), float.Parse((n_move.localPosition.y * 100f).ToString("f1")), float.Parse((n_move.localPosition.z * 100f).ToString("f1")));
			part.addMove[0, 1] = new Vector3(float.Parse(n_move.localEulerAngles.x.ToString("f0")), float.Parse(n_move.localEulerAngles.y.ToString("f0")), float.Parse(n_move.localEulerAngles.z.ToString("f0")));
			part.addMove[0, 2] = new Vector3(float.Parse(n_move.localScale.x.ToString("f2")), float.Parse(n_move.localScale.y.ToString("f2")), float.Parse(n_move.localScale.z.ToString("f2")));

			if (n_move2 != null)
			{
				n_move2.position = position2;
				n_move2.rotation = rotation2;
				part.addMove[1, 0] = new Vector3(float.Parse((n_move2.localPosition.x * 100f).ToString("f1")), float.Parse((n_move2.localPosition.y * 100f).ToString("f1")), float.Parse((n_move2.localPosition.z * 100f).ToString("f1")));
				part.addMove[1, 1] = new Vector3(float.Parse(n_move2.localEulerAngles.x.ToString("f0")), float.Parse(n_move2.localEulerAngles.y.ToString("f0")), float.Parse(n_move2.localEulerAngles.z.ToString("f0")));
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
