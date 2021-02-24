using System.Collections.Generic;
using UnityEngine;

namespace Specter.GUIModel
{
	public class GuiModel : MonoBehaviour
	{
		public class SliderModel
		{
			public SliderGeneral[] sliderGeneral;
			public List<SliderGeneral[]> sliderGeneralList = new List<SliderGeneral[]>();

			public void Init()
			{
				sliderGeneralList.Clear();
			}

			public SliderGeneral[] General(float currentValue)
			{
				return General(currentValue, hasNegativeMinValue: false);
			}

			public SliderGeneral[] General(float currentValue, bool hasNegativeMinValue)
			{
				sliderGeneral = new SliderGeneral[1];
				sliderGeneral[0] = new SliderGeneral(currentValue, hasNegativeMinValue);
				sliderGeneralList.Add(sliderGeneral);
				return sliderGeneral;
			}

			public SliderGeneral[] General(Vector3 currentValue)
			{
				return General(currentValue, hasNegativeMinValue: false);
			}

			public SliderGeneral[] General(Vector3 currentValue, float minVal, float maxVal)
			{
				sliderGeneral = new SliderGeneral[3];
				sliderGeneral[0] = new SliderGeneral(currentValue.x, minVal, maxVal);
				sliderGeneral[1] = new SliderGeneral(currentValue.y, minVal, maxVal);
				sliderGeneral[2] = new SliderGeneral(currentValue.z, minVal, maxVal);
				sliderGeneralList.Add(sliderGeneral);
				return sliderGeneral;
			}

			public SliderGeneral[] General(Vector3 currentValue, bool hasNegativeMinValue)
			{
				sliderGeneral = new SliderGeneral[3];
				sliderGeneral[0] = new SliderGeneral(currentValue.x, hasNegativeMinValue);
				sliderGeneral[1] = new SliderGeneral(currentValue.y, hasNegativeMinValue);
				sliderGeneral[2] = new SliderGeneral(currentValue.z, hasNegativeMinValue);
				sliderGeneralList.Add(sliderGeneral);
				return sliderGeneral;
			}
		}

		public class SliderGeneral
		{
			public float currentValue = 0f;
			public float defaultValue = 0f;
			public float minValue = 0f;
			public float maxValue = 0f;
			public bool isChanged = false;

			public SliderGeneral(float currentValue, bool hasNegativeMinValue)
			{
				this.currentValue = currentValue;
				defaultValue = currentValue;
				minValue = SetMinValue(currentValue, hasNegativeMinValue);
				maxValue = SetMaxValue(currentValue);
			}

			public SliderGeneral(float currentValue, float minVal, float maxVal)
			{
				this.currentValue = currentValue;
				defaultValue = currentValue;
				minValue = minVal;
				maxValue = maxVal;
			}

			private float SetMinValue(float value, bool hasNegative)
			{
				if (hasNegative)
					return (int)value + ((int)(value / 10f) + 1) * -3;
				return 0f;
			}

			public float SetMaxValue(float value)
			{
				return ((int)value + (int)(value / 10f) + 1) * 3;
			}
		}

		public SliderModel sliderModel = new SliderModel();
	}
}
