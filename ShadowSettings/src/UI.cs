using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShadowSettings
{
	public partial class ShadowSettings
	{
		internal class SceneEffectsCategory
		{
			internal SceneEffectsCategory(string name, string text)
			{
				CreateTitle(name, text);
				CreateContainer(name);
			}

			private Transform parent = GameObject.Find("StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content").transform;
			private Transform container;
			private float height = 0;

			private void CreateTitle(string name, string text)
			{
				Transform origin = parent.Find("Image Amplify Color Effect");
				Transform copy = Instantiate(origin, parent, false);
				copy.name = $"Image {name}";
				copy.GetComponentInChildren<TextMeshProUGUI>().text = text;
			}

			private void CreateContainer(string name)
			{
				Transform origin = parent.Find("Amplify Color Effect");
				Transform copy = Instantiate(origin, parent, false);
				copy.name = name;

				foreach (Transform child in copy)
				{
					if (child.gameObject != null)
						Destroy(child.gameObject);
				}

				LayoutElement copyLE = copy.GetComponent<LayoutElement>();
				copyLE.preferredHeight = 0;

				container = copy;
			}

			internal Dropdown AddDropdonw(string name, string text, List<string> options)
			{
				{
					Transform copy = Instantiate(parent.Find("Amplify Color Effect/TextMeshPro Lut"), container, false);
					copy.name = $"TextMeshPro {name}";
					copy.GetComponentInChildren<TextMeshProUGUI>().text = text;
					RectTransform copyRT = copy.GetComponent<RectTransform>();
					copyRT.offsetMin = new Vector2(copyRT.offsetMin.x, -1 * height - 20f);
					copyRT.offsetMax = new Vector2(copyRT.offsetMax.x, -1 * height);
				}

				{
					Transform copy = Instantiate(parent.Find("Amplify Color Effect/Dropdown Lut"), container, false);
					copy.name = $"Dropdown {name}";
					copy.GetComponent<Dropdown>().onValueChanged.RemoveAllListeners();
					RectTransform copyRT = copy.GetComponent<RectTransform>();
					copyRT.offsetMin = new Vector2(copyRT.offsetMin.x, -1 * height - 20f);
					copyRT.offsetMax = new Vector2(copyRT.offsetMax.x, -1 * height);

					height += 25f;
					container.GetComponent<LayoutElement>().preferredHeight = height;

					copy.GetComponent<Dropdown>().ClearOptions();
					copy.GetComponent<Dropdown>().options.AddRange(options.Select(x => new Dropdown.OptionData(x)));
					return copy.GetComponent<Dropdown>();
				}
			}

			internal Slider AddSlider(string name, string text, float initialValue, float sliderMinimum, float sliderMaximum)
			{
				{
					Transform copy = Instantiate(parent.Find("Amplify Color Effect/TextMeshPro Blend"), container, false);
					copy.name = $"TextMeshPro {name}";
					copy.GetComponentInChildren<TextMeshProUGUI>().text = text;
					RectTransform copyRT = copy.GetComponent<RectTransform>();
					copyRT.offsetMin = new Vector2(copyRT.offsetMin.x, -1 * height - 20f);
					copyRT.offsetMax = new Vector2(copyRT.offsetMax.x, -1 * height);
				}

				Slider slider;
				{
					Transform copy = Instantiate(parent.Find("Amplify Color Effect/Slider Blend"), container, false);
					copy.name = $"Slider {name}";
					slider = copy.GetComponent<Slider>();
					slider.onValueChanged.RemoveAllListeners();
					slider.minValue = sliderMinimum;
					slider.maxValue = sliderMaximum;
					slider.value = initialValue;
					RectTransform copyRT = copy.GetComponent<RectTransform>();
					copyRT.offsetMin = new Vector2(copyRT.offsetMin.x, -1 * height - 20f);
					copyRT.offsetMax = new Vector2(copyRT.offsetMax.x, -1 * height);
				}

				InputField input;
				{
					Transform copy = Instantiate(parent.Find("Amplify Color Effect/InputField Blend"), container, false);
					copy.name = $"InputField {name}";
					input = copy.GetComponent<InputField>();
					input.onValueChanged.RemoveAllListeners();
					input.onEndEdit.RemoveAllListeners();
					input.text = slider.value.ToString();
					RectTransform copyRT = copy.GetComponent<RectTransform>();
					copyRT.offsetMin = new Vector2(copyRT.offsetMin.x, -1 * height - 20f);
					copyRT.offsetMax = new Vector2(copyRT.offsetMax.x, -1 * height);
				}

				Button button;
				{
					Transform copy = Instantiate(parent.Find("Amplify Color Effect/Button Blend Default"), container, false);
					copy.name = $"Button {name} Default";
					button = copy.GetComponent<Button>();
					button.onClick.RemoveAllListeners();
					RectTransform copyRT = copy.GetComponent<RectTransform>();
					copyRT.offsetMin = new Vector2(copyRT.offsetMin.x, -1 * height - 20f);
					copyRT.offsetMax = new Vector2(copyRT.offsetMax.x, -1 * height);
				}

				height += 25f;
				container.GetComponent<LayoutElement>().preferredHeight = height;

				slider.onValueChanged.AddListener(delegate (float value)
				{
					input.text = value.ToString();
				});
				input.onEndEdit.AddListener(delegate (string value)
				{
					if (!float.TryParse(value, out float valuef))
					{
						input.text = slider.value.ToString();
						return;
					}

					if (valuef < sliderMinimum)
					{
						input.text = sliderMinimum.ToString();
						slider.value = sliderMinimum;
					}
					else if (valuef > sliderMaximum)
					{
						input.text = sliderMaximum.ToString();
						slider.value = sliderMaximum;
					}
					else
						slider.value = valuef;
				});
				button.onClick.AddListener(delegate ()
				{
					input.text = initialValue.ToString();
					slider.value = initialValue;
				});

				return slider;
			}
		}
	}
}
