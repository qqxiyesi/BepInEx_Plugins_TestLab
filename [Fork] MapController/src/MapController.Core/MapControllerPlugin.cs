using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using ExtensibleSaveFormat;
using Illusion.Extensions;
using Studio;
using UnityEngine;

namespace MapController
{
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInProcess(Constants.StudioProcessName)]
	public class MapControllerPlugin : BaseUnityPlugin
	{
		public const string GUID = "mikke.MapController";
		public const string PluginName = "Map Controller plugin";
		public const string Version = "0.3.0.1";

		private InfoNode rootNode;
		private Rect mapControllerWindowRect = new Rect(Screen.width - 400, (Screen.height < 1440) ? 20 : (Screen.height / 2), 375f, 715f);
		private float slideStep = 0f;
		private float step = 1f;
		private readonly HashSet<GameObject> openedObjects = new HashSet<GameObject>();
		private static string Search = "";
		private static bool ShowModified = false;
		public static readonly Dictionary<GameObject, InfoNode> dirtyNodes = new Dictionary<GameObject, InfoNode>();
		private readonly HashSet<GameObject> childObjects = new HashSet<GameObject>();
		private Vector2 scrollVector;
		public static GameObject map;
		private GameObject selected;
		private GameObject flashing;
		private int flashCount;
		private float lastflash;

		public void Start()
		{
			ExtendedSave.SceneBeingLoaded += ExtendedSaveOnSceneBeingLoaded;
			ExtendedSave.SceneBeingSaved += ExtendedSaveOnSceneBeingSaved;
		}

		private void ExtendedSaveOnSceneBeingLoaded(string path)
		{
			PluginData sceneExtendedDataById = ExtendedSave.GetSceneExtendedDataById(GUID);
			string text = default(string);
			int num;
			if (sceneExtendedDataById != null && sceneExtendedDataById.data.TryGetValue("MAP", out var value))
			{
				text = value as string;
				num = ((text != null) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			if (num != 0)
			{
				IEnumerable<string> enumerable = ParseNodeInfo(text);
				IEnumerator<string> enumerator = enumerable.GetEnumerator();
				enumerator.MoveNext();
				enumerator.MoveNext();
				rootNode = new InfoNode(enumerator);
			}
		}

		private void LateUpdate()
		{
			if (rootNode != null)
			{
				map = Singleton<Map>.Instance.mapRoot;
				if (!(map == null))
				{
					rootNode.TraverseOnLoad(map);
					rootNode = null;
				}
			}
		}

		private IEnumerable<string> ParseNodeInfo(string data)
		{
			string[] str = data.Split('§');
			string[] array = str;
			for (int i = 0; i < array.Length; i++)
			{
				yield return array[i];
			}
		}

		private void ExtendedSaveOnSceneBeingSaved(string path)
		{
			if (!(map == null))
			{
				PluginData pluginData = new PluginData();
				pluginData.data.Add("VERSION", "0.3");
				pluginData.data.Add("MAP", SaveChanges());
				ExtendedSave.SetSceneExtendedDataById(GUID, pluginData);
			}
		}

		private void OnGUI()
		{
			if (Singleton<MapCtrl>.Instance == null || !Singleton<MapCtrl>.Instance.gameObject.activeSelf)
			{
				return;
			}
			map = Singleton<Map>.Instance.mapRoot;
			if (!(map == null))
			{
				mapControllerWindowRect = GUILayout.Window(GetHashCode(), mapControllerWindowRect, MakeWin, "Map Controller Plugin");
				if (mapControllerWindowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
				{
					Input.ResetInputAxes();
				}
			}
		}

		private void MakeWin(int num)
		{
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(GUILayout.ExpandWidth(expand: true));
			GUILayout.BeginHorizontal();
			string search = Search;
			GUILayout.Label("Search", GUILayout.ExpandWidth(expand: false));
			Search = GUILayout.TextField(Search);
			if (GUILayout.Button("X", GUILayout.ExpandWidth(expand: false)))
			{
				Search = "";
			}
			if (search.Length != 0 && selected != null && (Search.Length == 0 || (Search.Length < search.Length && search.StartsWith(Search))))
			{
				string name = selected.name;
				if (selected.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1 || name.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1)
				{
					OpenParents(selected.gameObject);
				}
			}
			Color color = GUI.color;
			if (ShowModified)
			{
				GUI.color = Color.magenta;
			}
			if (GUILayout.Button("Modified only", GUILayout.Width(100f)))
			{
				ShowModified = !ShowModified;
			}
			GUI.color = color;
			GUILayout.EndHorizontal();
			scrollVector = GUILayout.BeginScrollView(scrollVector, GUI.skin.box, GUILayout.Width(375f), GUILayout.Height(470f));
			DisplayObjectTree(map, 0);
			GUILayout.EndScrollView();
			if (selected != null)
			{
				if (GUILayout.Button("Set " + (selected.activeSelf ? "inactive" : "active")))
				{
					ToggleActive();
				}
				GUILayout.BeginHorizontal("box");
				MakeBox("Move", SetTranslate, ResetTranslate);
				MakeBox("Rotate", SetRotate, ResetRotate);
				MakeBox("Scale", SetScale, ResetScale);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Step: " + step, GUILayout.MinWidth(200f), GUILayout.MaxWidth(200f));
				if (GUILayout.Button("-"))
				{
					slideStep -= 1f;
				}
				if (GUILayout.Button("+"))
				{
					slideStep += 1f;
				}
				step = (float)Math.Pow(10.0, slideStep);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUI.DragWindow();
		}

		private void SetScale(float x, float y, float z)
		{
			InfoNode infoNode = CheckDirtyNode();
			Vector3 vector = new Vector3(x / 10f, y / 10f, z / 10f);
			infoNode.NewScl += vector;
			selected.transform.localScale += vector;
		}

		private void ResetScale()
		{
			if (dirtyNodes.TryGetValue(selected, out var value))
			{
				selected.transform.localScale -= value.NewScl;
				value.NewScl = Vector3.zero;
				UpdateDirtyNode(value);
			}
		}

		private void SetRotate(float x, float y, float z)
		{
			InfoNode infoNode = CheckDirtyNode();
			selected.transform.Rotate(x, y, z, Space.Self);
			infoNode.NewRot = selected.transform.eulerAngles;
		}

		private void ResetRotate()
		{
			if (dirtyNodes.TryGetValue(selected, out var value))
			{
				selected.transform.rotation = Quaternion.Euler(value.OrgRot);
				value.NewRot = Vector3.zero;
				UpdateDirtyNode(value);
			}
		}

		private void SetTranslate(float x, float y, float z)
		{
			InfoNode infoNode = CheckDirtyNode();
			infoNode.NewPos += new Vector3(x, y, z);
			selected.transform.Translate(x, y, z, Space.World);
		}

		private void ResetTranslate()
		{
			if (dirtyNodes.TryGetValue(selected, out var value))
			{
				selected.transform.Translate(-value.NewPos, Space.World);
				value.NewPos = Vector3.zero;
				UpdateDirtyNode(value);
			}
		}

		private void MakeBox(string label, Action<float, float, float> transformAction, Action resetAction)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(label);
			GUILayout.BeginHorizontal();
			GUILayout.Label("X");
			if (GUILayout.Button("-1"))
			{
				transformAction(0f - step, 0f, 0f);
			}
			if (GUILayout.Button("+1"))
			{
				transformAction(step, 0f, 0f);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Y");
			if (GUILayout.Button("-1"))
			{
				transformAction(0f, 0f - step, 0f);
			}
			if (GUILayout.Button("+1"))
			{
				transformAction(0f, step, 0f);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Z");
			if (GUILayout.Button("-1"))
			{
				transformAction(0f, 0f, 0f - step);
			}
			if (GUILayout.Button("+1"))
			{
				transformAction(0f, 0f, step);
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Reset"))
			{
				resetAction();
			}
			GUILayout.EndVertical();
		}

		private void ToggleActive()
		{
			InfoNode infoNode = CheckDirtyNode();
			selected.SetActive(!selected.activeSelf);
			UpdateDirtyNode(infoNode);
		}

		private void UpdateDirtyNode(InfoNode infoNode)
		{
			if (infoNode.DefaultActive == selected.activeSelf && infoNode.NewPos == Vector3.zero && infoNode.NewRot == Vector3.zero && infoNode.NewScl == Vector3.zero)
			{
				dirtyNodes.Remove(selected);
			}
		}

		private InfoNode CheckDirtyNode()
		{
			if (dirtyNodes.TryGetValue(selected, out var value))
			{
				return value;
			}
			value = new InfoNode();
			value.Dirty = true;
			value.DefaultActive = selected.activeSelf;
			value.OrgRot = selected.transform.rotation.eulerAngles;
			dirtyNodes.Add(selected, value);
			return value;
		}

		private string SaveChanges()
		{
			InfoNode infoNode = MapInfoNode(map);
			StringBuilder stringBuilder = new StringBuilder();
			infoNode.WriteToString(stringBuilder);
			return stringBuilder.ToString();
		}

		private InfoNode MapInfoNode(GameObject o)
		{
			InfoNode infoNode = new InfoNode();
			if (dirtyNodes.ContainsKey(o))
			{
				InfoNode infoNode2 = dirtyNodes[o];
				infoNode.Dirty = true;
				infoNode.NewPos = infoNode2.NewPos;
				infoNode.NewRot = infoNode2.NewRot;
				infoNode.NewScl = infoNode2.NewScl;
			}
			infoNode.Name = o.name;
			infoNode.Inactive = !o.activeSelf;
			infoNode.Children = new List<InfoNode>();
			foreach (GameObject item in o.Children())
			{
				infoNode.Children.Add(MapInfoNode(item));
			}
			return infoNode;
		}

		private void OpenParents(GameObject child)
		{
			child = child.transform.parent.gameObject;
			while (child.transform != map.transform)
			{
				openedObjects.Add(child);
				child = child.transform.parent.gameObject;
			}
			openedObjects.Add(child);
		}

		private void DisplayObjectTree(GameObject go, int indent)
		{
			if (go == null)
			{
				return;
			}
			string name = go.name;
			if ((!ShowModified && (Search.Length == 0 || go.name.IndexOf(Search, StringComparison.OrdinalIgnoreCase) != -1 || name.IndexOf(Search, StringComparison.OrdinalIgnoreCase) != -1)) || (ShowModified && dirtyNodes.ContainsKey(go)))
			{
				Color color = GUI.color;
				if (!go.activeSelf)
				{
					GUI.color = Color.red;
				}
				if (dirtyNodes.ContainsKey(go))
				{
					GUI.color = Color.magenta;
				}
				if (selected == go)
				{
					GUI.color = Color.cyan;
				}
				GUILayout.BeginHorizontal();
				if (Search.Length == 0 && !ShowModified)
				{
					GUILayout.Space(indent * 20f);
					int num = 0;
					for (int i = 0; i < go.transform.childCount; i++)
					{
						if (!childObjects.Contains(go.transform.GetChild(i).gameObject))
						{
							num++;
						}
					}
					if (num != 0)
					{
						if (GUILayout.Toggle((openedObjects.Contains(go) ? 1 : 0) != 0, "", GUILayout.ExpandWidth(expand: false)))
						{
							if (!openedObjects.Contains(go))
							{
								openedObjects.Add(go);
							}
						}
						else if (openedObjects.Contains(go))
						{
							openedObjects.Remove(go);
						}
					}
					else
					{
						GUILayout.Space(20f);
					}
				}
				if (GUILayout.Button(name + (dirtyNodes.ContainsKey(go) ? "*" : ""), GUILayout.ExpandWidth(expand: false)))
				{
					Event current = Event.current;
					if ((current.type == EventType.MouseUp || EventType.Used == current.type) && current.button == 1)
					{
						StartFlash(go);
					}
					else
					{
						selected = go;
					}
				}
				GUI.color = color;
				GUILayout.EndHorizontal();
			}
			if (Search.Length != 0 || openedObjects.Contains(go) || ShowModified)
			{
				for (int j = 0; j < go.transform.childCount; j++)
				{
					DisplayObjectTree(go.transform.GetChild(j).gameObject, indent + 1);
				}
			}
		}

		private void StartFlash(GameObject go)
		{
			if (go.activeSelf)
			{
				if (flashing != null)
				{
					flashing.SetActive(value: true);
				}
				flashing = go;
				flashCount = 6;
				lastflash = Time.time;
			}
		}

		private void EndFlash()
		{
			flashing.SetActive(value: true);
			flashing = null;
		}

		private void Flash()
		{
			flashing.SetActive(!flashing.activeSelf);
			lastflash = Time.time;
		}

		private void Update()
		{
			if (flashCount > 0 && Time.time - lastflash > 0.1f)
			{
				if (--flashCount <= 0)
				{
					EndFlash();
				}
				else
				{
					Flash();
				}
			}
		}
	}
}
