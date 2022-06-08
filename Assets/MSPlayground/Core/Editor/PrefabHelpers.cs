
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Core
{
	public static class PrefabHelpers
	{
		//-----------------------------------------------------------------------------------
		public static List<string> GetAllPrefabs(bool includePackages = true)
		{
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			List<string> prefabPaths = new List<string>();
			foreach (string prefabPath in assetPaths)
			{
				if (!includePackages && prefabPath.StartsWith("Packages/"))
				{
					continue;
				}
				if (prefabPath.Contains(".prefab"))
				{
					prefabPaths.Add(prefabPath);
				}
			}
			return prefabPaths;
		}

		//-----------------------------------------------------------------------------------
		public static void IterateAllPrefabs(UnityAction<string,GameObject> callback, bool includePackages)
		{
			List<string> prefabPaths = GetAllPrefabs(includePackages);

			foreach (string prefabPath in prefabPaths)
			{
				Object rez = AssetDatabase.LoadMainAssetAtPath(prefabPath);
				GameObject go = rez as GameObject;

				callback(prefabPath, go);
			}
		}

		//-----------------------------------------------------------------------------------
		public static bool IsInPrefabEditor()
		{
#if UNITY_EDITOR
			var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			return stage != null;
#else
			return false;
#endif
		}
	}
}
