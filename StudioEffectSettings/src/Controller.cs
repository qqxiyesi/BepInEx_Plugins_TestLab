using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using Studio;
using ParadoxNotion.Serialization;

using HarmonyLib;
using ExtensibleSaveFormat;

using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;

namespace StudioEffectSettings
{
	public partial class StudioEffectSettings
	{
		public class StudioEffectSettingsController : SceneCustomFunctionController
		{
			protected override void OnSceneSave()
			{
				PluginData _pluginData = new PluginData();
				Dictionary<Type, Dictionary<string, float>> _floatFields = new Dictionary<Type, Dictionary<string, float>>();
				Dictionary<Type, Dictionary<string, bool>> _boolFields = new Dictionary<Type, Dictionary<string, bool>>();

				foreach (Type _type in _listEffectTypes)
				{
					Component _comp = Camera.main.GetComponent(_type);

					_floatFields[_type] = new Dictionary<string, float>();
					_boolFields[_type] = new Dictionary<string, bool>();

					foreach (FieldInfo x in _comp.GetType().GetFields())
					{
						if (x.FieldType == typeof(float))
							_floatFields[_type][x.Name] = (float) x.GetValue(_comp);
						else if (x.FieldType == typeof(bool))
							_boolFields[_type][x.Name] = (bool) x.GetValue(_comp);
					}
				}

				EffectSettings _data = new EffectSettings(_floatFields, _boolFields);
				_pluginData.data["EffectSettings"] = JSONSerializer.Serialize(typeof(EffectSettings), _data);

				SetExtendedData(_pluginData);
			}

			protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
			{
				if (!_cfgEnableDataLoad.Value) return;

				if (operation == SceneOperationKind.Load)
				{
					PluginData _pluginData = GetExtendedData();
					if (_pluginData?.data == null) return;

					Dictionary<Type, Dictionary<string, float>> _floatFields = new Dictionary<Type, Dictionary<string, float>>();
					Dictionary<Type, Dictionary<string, bool>> _boolFields = new Dictionary<Type, Dictionary<string, bool>>();
					EffectSettings _data = null;

					if (_pluginData.data.TryGetValue("EffectSettings", out object _loadEffectSettings) && _loadEffectSettings != null)
					{
						_data = JSONSerializer.Deserialize<EffectSettings>((string) _loadEffectSettings);
						_floatFields = _data.FloatFields;
						_boolFields = _data.BoolFields;
					}

					foreach (KeyValuePair<Type, Dictionary<string, float>> x in _floatFields)
					{
						Component _comp = Camera.main.GetComponent(x.Key);
						Traverse _traverse = Traverse.Create(_comp);
						foreach (KeyValuePair<string, float> y in x.Value)
							_traverse.Field<float>(y.Key).Value = y.Value;
					}

					foreach (KeyValuePair<Type, Dictionary<string, bool>> x in _boolFields)
					{
						Component _comp = Camera.main.GetComponent(x.Key);
						Traverse _traverse = Traverse.Create(_comp);
						foreach (KeyValuePair<string, bool> y in x.Value)
							_traverse.Field<bool>(y.Key).Value = y.Value;
					}
				}
			}
		}
	}
}
