using System;
using System.Collections.Generic;
using System.Text;
using Illusion.Extensions;
using UnityEngine;

namespace MapController
{
	public class InfoNode
	{
		public string Name;
		public bool Inactive;
		public List<InfoNode> Children;
		public bool Dirty;
		public Vector3 NewPos;
		public Vector3 NewRot;
		public Vector3 OrgRot;
		public Vector3 NewScl;
		public bool DefaultActive = true;

		public InfoNode(IEnumerator<string> enumerator)
		{
			string current = enumerator.Current;
			enumerator.MoveNext();
			string[] array = current.Split('%');
			int num = Convert.ToInt32(array[0]);
			Name = array[1];
			Dirty = array[2].Equals("1");
			Inactive = array[3].Equals("1");
			if (array.Length > 3)
			{
				NewPos = ReadVector(array[4]);
				NewRot = ReadVector(array[5]);
				NewScl = ReadVector(array[6]);
			}
			Children = new List<InfoNode>();
			for (int i = 0; i < num; i++)
			{
				Children.Add(new InfoNode(enumerator));
			}
		}

		public InfoNode()
		{
		}

		public void WriteToString(StringBuilder stringBuilder)
		{
			stringBuilder.Append("§").Append(Children.Count).Append("%")
				.Append(Name.Replace("§", "").Replace("%", ""))
				.Append("%")
				.Append(Dirty ? "1" : "0")
				.Append("%")
				.Append(Inactive ? "1" : "0")
				.Append("%")
				.Append(WriteVector(NewPos))
				.Append("%")
				.Append(WriteVector(NewRot))
				.Append("%")
				.Append(WriteVector(NewScl));
			foreach (InfoNode child in Children)
			{
				child.WriteToString(stringBuilder);
			}
		}

		private static Vector3 ReadVector(string s)
		{
			string[] array = s.Split(',');
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}

		private static string WriteVector(Vector3 v)
		{
			return v.x + "," + v.y + "," + v.z;
		}

		public void TraverseOnLoad(GameObject mapElement)
		{
			if (Dirty)
			{
				mapElement.SetActive(!Inactive);
				MapControllerPlugin.dirtyNodes[mapElement] = this;
				mapElement.transform.Translate(NewPos, Space.World);
				OrgRot = mapElement.transform.rotation.eulerAngles;
				mapElement.transform.rotation = Quaternion.Euler(NewRot);
				mapElement.transform.localScale += NewScl;
			}
			if (mapElement.name != Name)
			{
				UnityEngine.Debug.LogError("name mismatch: " + mapElement.name + "|" + Name);
			}
			List<GameObject> list = mapElement.Children();
			if (list.Count != Children.Count)
			{
				UnityEngine.Debug.LogError("child count mismatch in " + Name + " : " + list.Count + "|" + Children.Count);
			}
			else
			{
				for (int i = 0; i < Children.Count; i++)
				{
					Children[i].TraverseOnLoad(list[i]);
				}
			}
		}
	}
}
