using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using UnityEngine;
using Studio;

using BepInEx.Configuration;

namespace MapNavi
{
	public partial class MapNavi
	{
		internal class MapNaviUI : MonoBehaviour
		{
			private int _windowRectID;
			private Vector2 _windowSize = new Vector2(190, 375);
			internal Vector2 _windowPos = new Vector2(95, 440);
			private Rect _windowRect, _dragWindowRect;

			private Texture2D _windowBGtex = null;

			private Vector2 _ScreenRes = Vector2.zero;
			private Vector2 _resScaleFactor = Vector2.one;
			private Matrix4x4 _resScaleMatrix;
			//private bool _hasFocus = false;
			private bool _initStyle = true;

			private readonly GUILayoutOption _gloButtonS = GUILayout.Width(20);
			private readonly GUILayoutOption _gloButtonM = GUILayout.Width(50);
			private readonly GUILayoutOption _gloLabelM = GUILayout.Width(60);

			private GUIStyle _windowSolid;
			private GUIStyle _labelAlignCenter;
			private GUIStyle _textFieldAlignRight;

			private float _intervalSliderValue = 1;

			private float _posX, _posY, _posZ;
			private float _posSliderValue = 2;
			private List<float> _posIncValue = new List<float>();// { 0.001f, 0.01f, 0.1f, 1f };

			private float _rotX, _rotY, _rotZ;
			private float _rotSliderValue = 1;
			private List<float> _rotIncValue = new List<float>();// { 0.1f, 1f, 5f, 10f };

			private void Awake()
			{
				DontDestroyOnLoad(this);
				enabled = false;

				_windowPos.x = _cfgMakerWinX.Value;
				_windowPos.y = _cfgMakerWinY.Value;
				_windowRect = new Rect(_windowPos.x, _windowPos.y, _windowSize.x, _windowSize.y);
				_windowRectID = GUIUtility.GetControlID(FocusType.Passive);

				_posIncValue = (_cfgPosIncValue.Description.AcceptableValues as AcceptableValueList<float>).AcceptableValues.ToList();
				_rotIncValue = (_cfgRotIncValue.Description.AcceptableValues as AcceptableValueList<float>).AcceptableValues.ToList();
				_posSliderValue = _posIncValue.IndexOf(_cfgPosIncValue.Value);
				_rotSliderValue = _rotIncValue.IndexOf(_cfgRotIncValue.Value);
#if KK
				_windowBGtex = UI.MakeTex((int) _windowSize.x, (int) _windowSize.y + 10, new Color(0.5f, 0.5f, 0.5f, 1f));
#else
				_windowBGtex = UI.MakeTex((int) _windowSize.x, (int) _windowSize.y + 10, new Color(0.2f, 0.2f, 0.2f, 1f));
#endif
			}

			private void OnGUI()
			{
				if (_ScreenRes.x != Screen.width || _ScreenRes.y != Screen.height)
					ChangeRes();

				if (_initStyle)
				{
					//ChangeRes();

					_windowSolid = new GUIStyle(GUI.skin.window);
					Texture2D _onNormalBG = _windowSolid.onNormal.background;
					_windowSolid.normal.background = _onNormalBG;

					_labelAlignCenter = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
					_textFieldAlignRight = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleRight };

					_initStyle = false;
				}

				GUI.matrix = _resScaleMatrix;

				_dragWindowRect = GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, "", _windowSolid);
				_windowRect.x = _dragWindowRect.x;
				_windowRect.y = _dragWindowRect.y;
				/*
				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = false;
				*/
				if (/*_hasFocus && */UI.GetResizedRect(_windowRect).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					Input.ResetInputAxes();
			}

			private void OnEnable()
			{
				//_hasFocus = true;
			}

			private void OnDisable()
			{
				_initStyle = true;
				//_hasFocus = false;
			}

			private void DrawWindowContents(int _windowID)
			{
#if !KK
				GUI.backgroundColor = Color.grey;
#endif
				/*
				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = true;
				*/
				GUI.Box(new Rect(0, 0, _windowSize.x, _windowSize.y), _windowBGtex);
				GUI.Box(new Rect(0, 0, _windowSize.x, 30), "MapNavi", _labelAlignCenter);

				if (GUI.Button(new Rect(_windowSize.x - 27, 4, 23, 23), new GUIContent("X", "Close this window")))
				{
					CloseWindow();
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

					DrawPosGroup();
					DrawRotGroup();

					GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
					{
						GUILayout.Label("Repeat:", GUILayout.Width(50));
						GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
						{
							GUILayout.Space(5);
							_intervalSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_intervalSliderValue, 1, 3, GUILayout.Width(60)));
						}
						GUILayout.EndVertical();
						GUILayout.Label((_intervalSliderValue / 10).ToString(), _labelAlignCenter, GUILayout.Width(40));
						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				GUI.DragWindow();
			}
			private void DrawPosGroup()
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
#if KK
					ChangeAmount _ca = Studio.Studio.Instance.sceneInfo.caMap;
#else
					ChangeAmount _ca = Studio.Studio.Instance.sceneInfo.mapInfo.ca;
#endif
					Vector3 _pos = _ca.pos;
					float _inc = _posIncValue[(int) _posSliderValue];

					GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));
					{
						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						GUILayout.Label("Position", _labelAlignCenter);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" X:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.x -= _inc;
									_ca.pos = _pos;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_posX = _pos.x;
							if (float.TryParse(GUILayout.TextField(_posX.ToString("F3", CultureInfo.InvariantCulture), _textFieldAlignRight, GUILayout.Width(50)), out _posX))
							{
								if (_pos.x != _posX)
								{
									_pos.x = _posX;
									_ca.pos = _pos;
								}
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.x += _inc;
									_ca.pos = _pos;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_pos.x = 0;
								_ca.pos = _pos;
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" Y:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.y -= _inc;
									_ca.pos = _pos;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_posY = _pos.y;
							if (float.TryParse(GUILayout.TextField(_posY.ToString("F3", CultureInfo.InvariantCulture), _textFieldAlignRight, GUILayout.Width(50)), out _posY))
							{
								if (_pos.y != _posY)
								{
									_pos.y = _posY;
									_ca.pos = _pos;
								}
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.y += _inc;
									_ca.pos = _pos;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_pos.y = 0;
								_ca.pos = _pos;
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" Z:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.z -= _inc;
									_ca.pos = _pos;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_posZ = _pos.z;
							if (float.TryParse(GUILayout.TextField(_posZ.ToString("F3", CultureInfo.InvariantCulture), _textFieldAlignRight, GUILayout.Width(50)), out _posZ))
							{
								if (_pos.z != _posZ)
								{
									_pos.z = _posZ;
									_ca.pos = _pos;
								}
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.z += _inc;
									_ca.pos = _pos;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_pos.z = 0;
								_ca.pos = _pos;
							}
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						{
							GUILayout.Space(5);
							GUILayout.Label("Inc:", GUILayout.Width(40));
							GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
							{
								GUILayout.Space(5);
								_posSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_posSliderValue, 0, _posIncValue.Count - 1, GUILayout.Width(60)));
							}
							GUILayout.EndVertical();
							GUILayout.Label(_inc.ToString(), _labelAlignCenter, GUILayout.Width(40));
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			private void DrawRotGroup()
			{
				GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
#if KK
					ChangeAmount _ca = Studio.Studio.Instance.sceneInfo.caMap;
#else
					ChangeAmount _ca = Studio.Studio.Instance.sceneInfo.mapInfo.ca;
#endif
					Vector3 _rot = _ca.rot;
					float _inc = _rotIncValue[(int) _rotSliderValue];

					GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
					{
						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						GUILayout.Label("Rotation", _labelAlignCenter);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" X:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.x = (_rot.x - _inc + 360f) % 360f;
									_ca.rot = _rot;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_rotX = _rot.x;
							if (float.TryParse(GUILayout.TextField(_rotX.ToString("F1", CultureInfo.InvariantCulture), _textFieldAlignRight, GUILayout.Width(50)), out _rotX))
							{
								if (_rot.x != _rotX)
								{
									_rot.x = (_rotX + 360f) % 360f;
									_ca.rot = _rot;
								}
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.x = (_rot.x + _inc) % 360f;
									_ca.rot = _rot;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_rot.x = 0;
								_ca.rot = _rot;
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" Y:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.y = (_rot.y - _inc + 360f) % 360f;
									_ca.rot = _rot;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_rotY = _rot.y;
							if (float.TryParse(GUILayout.TextField(_rotY.ToString("F1", CultureInfo.InvariantCulture), _textFieldAlignRight, GUILayout.Width(50)), out _rotY))
							{
								if (_rot.y != _rotY)
								{
									_rot.y = (_rotY + 360f) % 360f;
									_ca.rot = _rot;
								}
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.y = (_rot.y + _inc) % 360f;
									_ca.rot = _rot;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_rot.y = 0;
								_ca.rot = _rot;
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" Z:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.z = (_rot.z - _inc + 360f) % 360f;
									_ca.rot = _rot;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_rotZ = _rot.z;
							if (float.TryParse(GUILayout.TextField(_rotZ.ToString("F1", CultureInfo.InvariantCulture), _textFieldAlignRight, GUILayout.Width(50)), out _rotZ))
							{
								if (_rot.z != _rotZ)
								{
									_rot.z = (_rotZ + 360f) % 360f;
									_ca.rot = _rot;
								}
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.z = (_rot.z + _inc) % 360f;
									_ca.rot = _rot;
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_rot.z = 0;
								_ca.rot = _rot;
							}
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						{
							GUILayout.Space(5);
							GUILayout.Label("Inc:", GUILayout.Width(40));
							GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
							{
								GUILayout.Space(5);
								_rotSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_rotSliderValue, 0, _rotIncValue.Count - 1, GUILayout.Width(60)));
							}
							GUILayout.EndVertical();
							GUILayout.Label(_inc.ToString(), _labelAlignCenter, GUILayout.Width(40));
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			private void CloseWindow()
			{
				//enabled = false;
				_ttConfigWindow.SetValue(false);
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

			// https://forum.unity.com/threads/repeat-button-speed.132477/
			private bool _controlbool = false;
			private IEnumerator WaitCor()
			{
				yield return new WaitForSeconds(_intervalSliderValue / 10);
				_controlbool = false;
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