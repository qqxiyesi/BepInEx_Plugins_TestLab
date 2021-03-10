using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if !KK
using TMPro;
#endif

namespace StudioCharaReload
{
	public partial class StudioCharaReload
	{
		// codes from DefaultParamEditor

		internal static Button CreateCharaButton(string name, string label, ScrollRect scrollRect, UnityAction onClickEvent)
		{
			return CreateButton(name, label, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root/Viewport/Content/State");
		}

		internal static ScrollRect SetupList(string goPath)
		{
			GameObject listObject = GameObject.Find(goPath);
			ScrollRect scrollRect = listObject.GetComponent<ScrollRect>();
			scrollRect.content.gameObject.GetOrAddComponent<VerticalLayoutGroup>();
			scrollRect.content.gameObject.GetOrAddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			scrollRect.scrollSensitivity = 25;

			foreach (Transform item in scrollRect.content.transform)
			{
				LayoutElement layoutElement = item.gameObject.GetOrAddComponent<LayoutElement>();
				layoutElement.preferredHeight = 40;
			}

			return scrollRect;
		}

		internal static Button CreateButton(string name, string label, ScrollRect scrollRect, UnityAction onClickEvent, string goPath)
		{
			GameObject template = GameObject.Find(goPath);
			GameObject newObject = Instantiate(template, scrollRect.content.transform);
			newObject.name = name;
#if !KK
			TextMeshProUGUI textComponent = newObject.GetComponentInChildren<TextMeshProUGUI>();
#else
			Text textComponent = newObject.GetComponentInChildren<Text>();
#endif
			textComponent.text = label;
			Button buttonComponent = newObject.GetComponent<Button>();
			for (int i = 0; i < buttonComponent.onClick.GetPersistentEventCount(); i++)
				buttonComponent.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
			buttonComponent.onClick.RemoveAllListeners();
			buttonComponent.onClick.AddListener(onClickEvent);
			return buttonComponent;
		}
	}
}
