
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MSPlayground.Core
{
	public class FindMissingReferences
	{
		static string MissingScriptsString = null;

		//-----------------------------------------------------------------------------------
		[MenuItem("Utils/Missing Script Check/Current Scene")]
		public static void FindMissingScriptsCurrentScene()
		{
			DoFindMissingScriptsInCurrentScene();
		}

		//-----------------------------------------------------------------------------------
		[MenuItem("Utils/Missing Script Check/Scenes In Build")]
		public static void FindMissingScriptsScenesInBuild()
		{
			DoFindMissingScriptsScenesInBuild();
		}

		//-----------------------------------------------------------------------------------
		[MenuItem("Utils/Missing Script Check/Prefabs excluding Packages")]
		public static void FindMissingScriptsInPrefabsExcludingPackages()
		{
			FindMissingScriptsInPrefabs(false);
		}

		//-----------------------------------------------------------------------------------
		[MenuItem("Utils/Missing Script Check/Prefabs including Packages")]
		public static void FindMissingScriptsInPrefabsIncludingPackages()
		{
			FindMissingScriptsInPrefabs(true);
		}

		//-----------------------------------------------------------------------------------	
		public static void FindMissingScriptsInPrefabs(bool includePackages)
		{
			MissingScriptsString = "";

			int numPrefabsMissingScripts = 0;

			PrefabHelpers.IterateAllPrefabs((string prefabPath, GameObject go) =>
			{
				numPrefabsMissingScripts += DoesGameObjectHaveAllScripts(go, true, null, prefabPath);
			}, includePackages);

			Debug.Log(string.Format("=== Prefabs: {0} missing scripts{1}\n", numPrefabsMissingScripts, MissingScriptsString));
		}

		//-----------------------------------------------------------------------------------
		static int DoesGameObjectHaveAllScripts(GameObject go, bool recursive, bool? wantActive, string prefabPath)
		{
			int numMissingScripts = 0;
			bool foundMissingScript = false;

			try
			{
				if (!wantActive.HasValue || go.activeInHierarchy == wantActive.Value)
				{
					Component[] components = go.GetComponents<MonoBehaviour>();
					foreach (Component component in components)
					{
						if (component == null)
						{
							if (prefabPath == null)
							{
								MissingScriptsString += string.Format("\nGO: {0}", go.GetFullName());
							}
							else
							{
								if (!foundMissingScript)
								{
									MissingScriptsString += string.Format("\nPrefab: {0} ==> {1}", prefabPath, go.GetFullName());
								}
							}

							foundMissingScript = true;
							numMissingScripts++;
						}
					}
				}
			}
			catch(Exception ex)
			{
				Debug.Log("Exception: " + ex.Message + " go = " + go + " Prefab = " + prefabPath);
			}

			if (recursive)
			{
				for (int i = 0; i < go.transform.childCount; i++)
				{
					GameObject child = go.transform.GetChild(i).gameObject;
					numMissingScripts += DoesGameObjectHaveAllScripts(child, recursive, wantActive, prefabPath);
				}
			}

			return numMissingScripts;
		}

		//-----------------------------------------------------------------------------------
		static void DoFindMissingScriptsInCurrentScene()
		{
			int numMissingScripts = 0;
			MissingScriptsString = "";

			// find all objects in the hierarchy
			foreach (GameObject go in EditorSceneManager.GetActiveScene().GetRootGameObjects())
			{
				numMissingScripts += DoesGameObjectHaveAllScripts(go, true, null, null);
			}

			Debug.Log(string.Format("=== Scene: {0} === {1} missing scripts:{2}\n", EditorSceneManager.GetActiveScene().path, numMissingScripts, MissingScriptsString));
		}

		//-----------------------------------------------------------------------------------
		static void DoFindMissingScriptsScenesInBuild()
		{
			SceneIterator.IterateScenesInBuildSettings((EditorBuildSettingsScene scene) =>
			{
				DoFindMissingScriptsInCurrentScene();
			});
		}
	}
}