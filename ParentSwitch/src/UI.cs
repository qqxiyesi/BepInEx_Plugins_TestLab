using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ChaCustom;
using TMPro;

using HarmonyLib;

using KKAPI.Maker;

using MoreAccessoriesKOI;

namespace ParentSwitch
{
	public partial class ParentSwitch
	{
		internal class ParentSwitchUI : MonoBehaviour
		{
			private int _windowRectID;
			private Vector2 _windowSize = new Vector2(290, 375);
			internal Vector2 _windowPos = new Vector2(525, 80);
			private Rect _windowRect, _dragWindowRect;

			private Texture2D _windowBGtex = null;

			private Vector2 _ScreenRes = Vector2.zero;
			private Vector2 _resScaleFactor = Vector2.one;
			private Matrix4x4 _resScaleMatrix;
			private bool _hasFocus = false;
			private bool _passThrough = false;
			private bool _initStyle = true;

			private readonly GUILayoutOption _gloButtonS = GUILayout.Width(20);
			private readonly GUILayoutOption _gloItemName = GUILayout.Width(190);

			private GUIStyle _windowSolid;
			private GUIStyle _labelAlignCenter;
			private GUIStyle _textFieldLabelGrey;
			private GUIStyle _textFieldLabel;
			private GUIStyle _buttonActive;

			private Vector2 _accListScrollPos = Vector2.zero;
			private Vector2 _parentListScrollPos = Vector2.zero;
			internal Dictionary<int, bool> _checkboxList = new Dictionary<int, bool>();
			private HashSet<string> _parents = new HashSet<string>();
			internal string _selectedParent = "";

			private void Awake()
			{
				DontDestroyOnLoad(this);
				enabled = false;

				_windowPos.x = _cfgMakerWinX.Value;
				_windowPos.y = _cfgMakerWinY.Value;
				_passThrough = _cfgDragPass.Value;
				_windowRect = new Rect(_windowPos.x, _windowPos.y, _windowSize.x, _windowSize.y);
				_windowRectID = GUIUtility.GetControlID(FocusType.Passive);
				_windowBGtex = UI.MakeTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.5f, 0.5f, 0.5f, 1f));
			}

			private void OnGUI()
			{
				if (CustomBase.Instance?.chaCtrl == null) return;
				if (CustomBase.Instance.customCtrl.hideFrontUI) return;
				if (!Manager.Scene.Instance.AddSceneName.IsNullOrEmpty() && Manager.Scene.Instance.AddSceneName != "CustomScene") return;

				if (_ScreenRes.x != Screen.width || _ScreenRes.y != Screen.height)
					ChangeRes();

				if (_initStyle)
				{
					ChangeRes();

					_windowSolid = new GUIStyle(GUI.skin.window);
					Texture2D _onNormalBG = _windowSolid.onNormal.background;
					_windowSolid.normal.background = _onNormalBG;

					_labelAlignCenter = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

					_textFieldLabel = new GUIStyle(GUI.skin.label);
					_textFieldLabel.wordWrap = false;
					_textFieldLabel.alignment = TextAnchor.MiddleLeft;

					_textFieldLabelGrey = new GUIStyle(GUI.skin.label);
					_textFieldLabelGrey.wordWrap = false;
					_textFieldLabelGrey.alignment = TextAnchor.MiddleLeft;
					_textFieldLabelGrey.normal.textColor = Color.grey;

					_buttonActive = new GUIStyle(GUI.skin.button);
					_buttonActive.normal.textColor = Color.cyan;
					_buttonActive.hover.textColor = Color.cyan;
					_buttonActive.fontStyle = FontStyle.Bold;

					_initStyle = false;
				}

				GUI.matrix = _resScaleMatrix;

				_dragWindowRect = GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, "", _windowSolid);
				_windowRect.x = _dragWindowRect.x;
				_windowRect.y = _dragWindowRect.y;

				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = false;

				if ((!_passThrough || _hasFocus) && UI.GetResizedRect(_windowRect).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					Input.ResetInputAxes();
			}

			private void OnEnable()
			{
				_hasFocus = true;
			}

			private void OnDisable()
			{
				_initStyle = true;
				_hasFocus = false;
			}

			private void DrawWindowContents(int _windowID)
			{
				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = true;

				GUI.Box(new Rect(0, 0, _windowSize.x, _windowSize.y), _windowBGtex);
				GUI.Box(new Rect(0, 0, _windowSize.x, 30), Name, _labelAlignCenter);

				if (GUI.Button(new Rect(_windowSize.x - 27, 4, 23, 23), new GUIContent("X", "Close this window")))
				{
					CloseWindow();
				}

				if (GUI.Button(new Rect(_windowSize.x - 50, 4, 23, 23), new GUIContent("0", "Config window will not block mouse drag from outside (experemental)"), (_passThrough ? _buttonActive : new GUIStyle(GUI.skin.button))))
				{
					_passThrough = !_passThrough;
					_logger.LogMessage($"Pass through mode: {(_passThrough ? "ON" : "OFF")}");
				}

				if (GUI.Button(new Rect(4, 4, 23, 23), new GUIContent("<", "Reset window position")))
				{
					ResetPos();
				}

				if (GUI.Button(new Rect(27, 4, 23, 23), new GUIContent("T", "Use current window position when reset")))
				{
					_windowPos.x = _dragWindowRect.x;
					_windowPos.y = _dragWindowRect.y;
					_cfgMakerWinX.Value = _windowPos.x;
					_cfgMakerWinY.Value = _windowPos.y;
				}

				GUILayout.BeginVertical();
				{
					GUILayout.Space(10);

					DrawAccListGroup();
					DrawParentListGroup();

					GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
					{
						GUILayout.BeginVertical();
						{
							string _tip = "The character will be set to T-pose";
							if (_selectedParent == "")
								_tip = "Select a parent first";
							else if (!_checkboxList.Any(x => x.Value))
								_tip = "Select a part first";

							if (GUILayout.Button(new GUIContent("Apply", _tip)))
							{
								if (_selectedParent == "")
								{
									_logger.LogMessage("Select a parent first");
									return;
								}

								if (!_checkboxList.Any(x => x.Value))
								{
									_logger.LogMessage("Select a part first");
									return;
								}

								FindObjectOfType<BaseCameraControl_Ver2>().Reset(0);

								TMP_Dropdown _ddPose = Traverse.Create(CustomBase.Instance.customCtrl.cmpDrawCtrl).Field("ddPose").GetValue<TMP_Dropdown>();
								if (_ddPose.options.Count > 66) // some shit dirty hack since LoadAnimation act weird
									_ddPose.value = _ddPose.options.Count - 1;
								else
								{
									if (_ddPose.options.Count > 57)
										_ddPose.value = 57;
								}
								CustomBase.Instance.chaCtrl.LoadAnimation("studio/anime/00.unity3d", "tpose");
								CustomBase.Instance.chaCtrl.AnimPlay("tpose");

								MoreAccessories.CharAdditionalData _additionalData = _accessoriesByChar[_chaCtrl.chaFile];
								int _count = (int) _additionalData?.nowAccessories?.Count + 20;

								for (int i = 0; i < _count; i++)
								{
									if (_checkboxList[i])
										ChangeParent(i, _selectedParent);
								}

								_checkboxList.Clear();
								_selectedParent = "";
								CustomBase.Instance.updateCustomUI = true;
								CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
								_logger.LogMessage($"Done");
							}
						}
						GUILayout.EndVertical();
						/*
						GUILayout.BeginVertical();
						GUILayout.Label("The character will be set to T-pose");
						GUILayout.EndVertical();
						*/
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
					GUILayout.Label(GUI.tooltip);
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				GUI.DragWindow();
			}

			private void DrawParentListGroup()
			{
				GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(85), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					{
						_parentListScrollPos = GUILayout.BeginScrollView(_parentListScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
						{
							if (GUILayout.Button("(reset)"))
							{
								_selectedParent = "";
								_checkboxList.Clear();
							}

							foreach (string _name in _parents)
							{
								GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
								{
									if (_selectedParent == _name)
									{
										GUILayout.Button(_name, _buttonActive);
									}
									else
									{
										if (GUILayout.Button(_name))
										{
											_selectedParent = _name;
											//_checkboxList.Clear();
										}
									}
								}
								GUILayout.EndHorizontal();
							}
						}
						GUILayout.EndScrollView();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			private void DrawAccListGroup()
			{
				//GUILayout.BeginHorizontal(GUILayout.Height(210), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					{
						_accListScrollPos = GUILayout.BeginScrollView(_accListScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
						{
							int _slotIndex = 0;
							foreach (ChaFileAccessory.PartsInfo _part in ListPartsInfo())
							{
								DrawItemRaw(_slotIndex, _part);
								_slotIndex++;
							}
						}
						GUILayout.EndScrollView();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			private void DrawItemRaw(int _slotIndex, ChaFileAccessory.PartsInfo _part)
			{
				if (!_checkboxList.ContainsKey(_slotIndex))
					_checkboxList[_slotIndex] = false;

				if (_part.type != 120)
					_parents.Add(_part.parentKey);

				//if (_part.type == 120 || _part.parentKey == _selectedParent)
				if (_part.type == 120)
				{
					_checkboxList[_slotIndex] = false;
					return;
				}

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
				{
					GameObject _gameObject = _chaCtrl.GetAccessoryObject(_slotIndex);
					ListInfoBase _data = _gameObject?.GetComponent<ListInfoComponent>()?.data;
					if (_part.parentKey == _selectedParent)
					{
						_checkboxList[_slotIndex] = false;
						string _tip = $"The part is already under {_selectedParent}";
						GUILayout.Toggle(_checkboxList[_slotIndex], new GUIContent("", _tip), _gloButtonS);
						GUILayout.Label(new GUIContent($"{_slotIndex + 1:00}:", _tip), _textFieldLabelGrey);
						GUILayout.TextField($"{_data?.Name}", _textFieldLabelGrey, _gloItemName, GUILayout.ExpandWidth(false));
					}
					else
					{
						string _tip = $"The part is under {_part.parentKey}";
						_checkboxList[_slotIndex] = GUILayout.Toggle(_checkboxList[_slotIndex], new GUIContent("", _tip), _gloButtonS);
						GUILayout.Label(new GUIContent($"{_slotIndex + 1:00}:", _tip), _textFieldLabel);
						GUILayout.TextField($"{_data?.Name}", _textFieldLabel, _gloItemName, GUILayout.ExpandWidth(false));
					}
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
			}

			private void CloseWindow()
			{
				//enabled = false;
				_sidebarToggleEnable.SetValue(false);
			}

			// https://answers.unity.com/questions/840756/how-to-scale-unity-gui-to-fit-different-screen-siz.html
			internal void ChangeRes()
			{
				_ScreenRes.x = Screen.width;
				_ScreenRes.y = Screen.height;
				_resScaleFactor.x = _ScreenRes.x / 1600;
				_resScaleFactor.y = _ScreenRes.y / 900;
				_resScaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_resScaleFactor.x, _resScaleFactor.y, 1));
				ResetPos();
			}

			internal void ResetPos()
			{
				_windowPos.x = _cfgMakerWinX.Value;
				_windowPos.y = _cfgMakerWinY.Value;
				_windowRect.x = _windowPos.x;
				_windowRect.y = _windowPos.y;
			}

			internal static class UI
			{
				// https://bensilvis.com/unity3d-auto-scale-gui/
				internal static Rect GetResizedRect(Rect _rect)
				{
					Vector2 _position = GUI.matrix.MultiplyVector(new Vector2(_rect.x, _rect.y));
					Vector2 _size = GUI.matrix.MultiplyVector(new Vector2(_rect.width, _rect.height));

					return new Rect(_position.x, _position.y, _size.x, _size.y);
				}

				internal static Texture2D MakeTex(int _width, int _height, Color _color)
				{
					Color[] pix = new Color[_width * _height];

					for (int i = 0; i < pix.Length; i++)
						pix[i] = _color;

					Texture2D result = new Texture2D(_width, _height);
					result.SetPixels(pix);
					result.Apply();

					return result;
				}
			}
		}
	}
}
