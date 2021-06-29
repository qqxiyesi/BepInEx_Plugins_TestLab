using UnityEngine;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal class MovUrAccUI : MonoBehaviour
		{
			private int _windowRectID;
			private Rect _windowRect;
			private Vector2 _windowSize = new Vector2(300f, 450f);
			private Texture2D _windowBGtex = null;
			private bool _hasFocus = false;

			private static readonly GUILayoutOption expandLayoutOption = GUILayout.ExpandWidth(true);

			private void Awake()
			{
				DontDestroyOnLoad(this);
				enabled = false;

				_windowRectID = GUIUtility.GetControlID(FocusType.Passive);
				_windowRect = new Rect(340, 210, _windowSize.x, _windowSize.y);
				_windowBGtex = MakeTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.2f, 0.2f, 0.2f, 1f));
			}

			private void OnGUI()
			{
				GUIStyle _windowSolid = new GUIStyle(GUI.skin.window);
				Texture2D _onNormalBG = _windowSolid.onNormal.background;
				_windowSolid.normal.background = _onNormalBG;

				Rect rect = GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, "MovUrAcc", _windowSolid);
				_windowRect.x = rect.x;
				_windowRect.y = rect.y;

				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = false;

				if (_hasFocus && _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					Input.ResetInputAxes();
			}

			private string _movStart = "", _movEnd = "", _movTo = "";
			private int _movMode = 0;
			private string _delStart = "", _delEnd = "";
			private int _delMode = 0;

			private void DrawWindowContents(int id)
			{
				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = true;

				GUI.backgroundColor = Color.grey;
				GUI.Box(new Rect(0, 0, _windowSize.x, _windowSize.y), _windowBGtex);
				GUI.Box(new Rect(0, 0, _windowSize.x, 23), $"MovUrAcc", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

				GUILayout.BeginVertical();
				{
					GUI.changed = false;

					GUILayout.BeginVertical(GUI.skin.box);

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Box("Batch transfer accessory slots", expandLayoutOption);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("Start", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					_movStart = GUILayout.TextField(_movStart, GUILayout.Width(100));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _movStart = "";
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("End", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					_movEnd = GUILayout.TextField(_movEnd, GUILayout.Width(100));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _movEnd = "";
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("Shift first slot to", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					_movTo = GUILayout.TextField(_movTo, GUILayout.Width(100));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _movTo = "";
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("Mode", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					string[] _movOpt = new string[] { "All", "Hair", "Item" };
					_movMode = GUILayout.SelectionGrid(_movMode, _movOpt, _movOpt.Length, GUI.skin.toggle);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					if (GUILayout.Button("Go", GUILayout.ExpandWidth(true)))
					{
						if (!int.TryParse(_movStart, out int start))
						{
							_movStart = "";
							start = 0;
						}
						if (!int.TryParse(_movEnd, out int end))
						{
							_movEnd = "";
							end = 0;
						}
						if (!int.TryParse(_movTo, out int newstart))
						{
							_movTo = "";
							newstart = 0;
						}
						ActBatchTransfer(start - 1, end - 1, newstart - 1, _movMode);
					}
					GUILayout.EndHorizontal();

					GUILayout.EndHorizontal();

					GUILayout.BeginVertical(GUI.skin.box);

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Box("Batch remove accessory slots", expandLayoutOption);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("Start", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					_delStart = GUILayout.TextField(_delStart, GUILayout.Width(100));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _delStart = "";
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("End", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					_delEnd = GUILayout.TextField(_delEnd, GUILayout.Width(100));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _delEnd = "";
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Label("Mode", GUILayout.ExpandWidth(false));
					GUILayout.FlexibleSpace();
					string[] _delOpt = new string[] { "All", "Hair", "Item" };
					_delMode = GUILayout.SelectionGrid(_delMode, _delOpt, _delOpt.Length, GUI.skin.toggle);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(expandLayoutOption);
					if (GUILayout.Button("Go", GUILayout.ExpandWidth(true)))
					{
						if (!int.TryParse(_delStart, out int start))
						{
							_delStart = "";
							start = 0;
						}
						if (!int.TryParse(_delEnd, out int end))
						{
							_delEnd = "";
							end = 0;
						}
						ActBatchRemove(start - 1, end - 1, _delMode);
					}
					GUILayout.EndHorizontal();

					GUILayout.EndHorizontal();

					GUILayout.BeginVertical(GUI.skin.box);

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Box("Pack acc list by removing unused slots", expandLayoutOption);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(expandLayoutOption);
					if (GUILayout.Button("Go", GUILayout.ExpandWidth(true)))
					{
						ActPacking();
					}
					GUILayout.EndHorizontal();

					GUILayout.EndHorizontal();

					GUILayout.BeginVertical(GUI.skin.box);

					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.Box("Trim down unused MoreAccessories slots", expandLayoutOption);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(expandLayoutOption);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(expandLayoutOption);
					if (GUILayout.Button("Go", GUILayout.ExpandWidth(true)))
					{
						MoreAccessories.TrimUnusedSlots();
						RefreshMakerUI();
					}
					GUILayout.EndHorizontal();

					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				GUI.DragWindow();
			}

			private Texture2D MakeTex(int _width, int _height, Color _color)
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
