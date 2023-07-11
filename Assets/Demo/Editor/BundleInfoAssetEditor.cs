using UnityEngine;
using UnityEditor;
using NCore.JSON;
using System.IO;

namespace MirrorCity
{
	[CustomEditor(typeof(BundleInfos))]
	public class BundleInfoAssetEditor : Editor
	{
		private BundleInfos _target;

		private void OnEnable()
		{
			_target = (BundleInfos) target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_target != null)
			{
				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("TextDataLoad") == true)
					Load(_target.textAsset.text);

				if (GUILayout.Button("TextDataSave") == true)
					Save();

				EditorGUILayout.EndHorizontal();
			}
		}

		private void Load(string data)
		{
			if (string.IsNullOrEmpty(data) == true)
				return;

			_target.bundleInfos.Clear();

			var jsonArray = JSONArray.Parse(data);

			foreach (var json in jsonArray)
			{
				BundleInfo bundleInfo = new BundleInfo(json.Obj);
				_target.bundleInfos.Add(bundleInfo);
			}
		}

		private void Save()
		{
			if (_target.bundleInfos == null || _target.bundleInfos.Count == 0)
				return;

			var assetPath = AssetDatabase.GetAssetPath(_target);

			var jsonArray = new JSONArray();

			foreach (var bundle in _target.bundleInfos)
			{
				var json = bundle.ToJson();
				jsonArray.Add(json);
			}

			var assetName = Path.GetFileName(assetPath);
			assetPath = assetPath.Replace(assetName, "");
			File.WriteAllText($"{assetPath}/bundle.json", jsonArray.ToString());
			AssetDatabase.Refresh();
		}
	}

}
