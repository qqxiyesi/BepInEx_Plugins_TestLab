using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using ParadoxNotion.Serialization;

using BepInEx;
using HarmonyLib;

using KKABMX.Core;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace ABMXHelper
{
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("com.deathweasel.bepinex.materialeditor", "2.5")]
	public class ABMXHelper : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.abmxh";
		public const string PluginName = "Boner Helper";
		public const string Version = "1.0.0.0";

		private void Start()
		{
			string _savePath = Path.Combine(Paths.GameRootPath, "Temp");
			string _saveFile = Path.Combine(_savePath, $"ABMXH.json");

			MakerAPI.RegisterCustomSubCategories += (object _sender, RegisterSubCategoriesEvent _args) =>
			{
				MakerCategory _category = new MakerCategory("05_ParameterTop", "tglABMXH", MakerConstants.Parameter.Attribute.Position + 1, "ABMXH");
				_args.AddSubCategory(_category);

				_args.AddControl(new MakerButton("Print", _category, this)).OnClick.AddListener(delegate
				{
					Logger.LogInfo($"\n{ExtractInfo()}");
				});

				_args.AddControl(new MakerButton("Export", _category, this)).OnClick.AddListener(delegate
				{
					if (!Directory.Exists(_savePath))
						Directory.CreateDirectory(_savePath);

					File.WriteAllText(_saveFile, ExtractInfo());
					Logger.LogMessage($"Export to {_saveFile}");
				});

				MakerToggle _tglMakeCoordinateSpecific = new MakerToggle(_category, "Make Coordinate Specific", false, this);

				_args.AddControl(new MakerButton("Import", _category, this)).OnClick.AddListener(delegate
				{
					if (!File.Exists(_saveFile))
					{
						Logger.LogMessage($"Import file {_saveFile} not exist");
						return;
					}

					Dictionary<string, BoneModifierData> _dataDic = JSONSerializer.Deserialize<Dictionary<string, BoneModifierData>>(File.ReadAllText(_saveFile));
					ChaControl _chaCtrl = MakerAPI.GetCharacterControl();
					BoneController _pluginCtrl = _chaCtrl?.gameObject?.GetComponent<BoneController>();

					foreach (KeyValuePair<string, BoneModifierData> x in _dataDic)
					{
						if (_pluginCtrl.GetModifier(x.Key) == null)
							_pluginCtrl.AddModifier(new BoneModifier(x.Key));

						BoneModifier _modifier = _pluginCtrl.GetModifier(x.Key);
						int _coordinateIndex = 0;
						if (_modifier.IsCoordinateSpecific() || _tglMakeCoordinateSpecific.Value)
							_coordinateIndex = _chaCtrl.fileStatus.coordinateType;
						/*
						_modifier.CoordinateModifiers[_coordinateIndex].ScaleModifier = x.Value.ScaleModifier;
						_modifier.CoordinateModifiers[_coordinateIndex].PositionModifier = x.Value.PositionModifier;
						_modifier.CoordinateModifiers[_coordinateIndex].RotationModifier = x.Value.RotationModifier;
						_modifier.CoordinateModifiers[_coordinateIndex].LengthModifier = x.Value.LengthModifier;
						*/
						_modifier.CoordinateModifiers[_coordinateIndex] = x.Value.Clone();
					}

					Traverse.Create(_pluginCtrl).Property("NeedsBaselineUpdate")?.SetValue(true);
					_chaCtrl.ChangeCoordinateType(true);
					ChaCustom.CustomBase.Instance.updateCustomUI = true;

					Logger.LogMessage($"Import done");
				});

				_args.AddControl(_tglMakeCoordinateSpecific);
			};
		}

		internal static string ExtractInfo()
		{
			ChaControl _chaCtrl = MakerAPI.GetCharacterControl();
			BoneController _pluginCtrl = _chaCtrl?.gameObject?.GetComponent<BoneController>();

			Dictionary<string, BoneModifierData> _dataDic = new Dictionary<string, BoneModifierData>();
			List<BoneModifier> _modifiers = _pluginCtrl.Modifiers;
			int _currentCoordinateIndex = _chaCtrl.fileStatus.coordinateType;

			for (int i = 0; i < _modifiers.Count; i++)
			{
				BoneModifier _modifier = _pluginCtrl.GetModifier(_modifiers[i].BoneName);
				int _coordinateIndex = _modifier.IsCoordinateSpecific() ? _currentCoordinateIndex : 0;
				BoneModifierData _data = _modifier.CoordinateModifiers[_coordinateIndex];

				if (CompareModifiers(_data, new BoneModifierData()))
					continue;

				_dataDic[_modifiers[i].BoneName] = _data.Clone();
			}

			_dataDic = _dataDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
			string _json = JSONSerializer.Serialize(_dataDic.GetType(), _dataDic, true);

			return _json;
		}

		internal static bool CompareModifiers(BoneModifierData a, BoneModifierData b)
		{
			if (a.ScaleModifier != b.ScaleModifier)
				return false;
			if (a.LengthModifier != b.LengthModifier)
				return false;
			if (a.PositionModifier != b.PositionModifier)
				return false;
			if (a.RotationModifier != b.RotationModifier)
				return false;
			return true;
		}
	}
}
