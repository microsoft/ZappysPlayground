using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MSPlayground.Core
{
	public class SceneIterator
	{
		public static void IterateScenesInBuildSettings(UnityAction<EditorBuildSettingsScene> callback)
		{
			// grab the scenes we need to restore at the end
			List<string> loadedScenePaths = new List<string>();
			for (int i = 0; i < EditorSceneManager.loadedSceneCount; i++)
			{
				Scene loadedScene = EditorSceneManager.GetSceneAt(i);
				loadedScenePaths.Add(loadedScene.path);
			}

			// save if user wants to
			EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

			// load each scene in build settings
			foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
			{
				EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
				callback(buildScene);
			}

			// reload our initial scenes
			bool firstScene = true;
			foreach (string scenePath in loadedScenePaths)
			{
				EditorSceneManager.OpenScene(scenePath, firstScene ? OpenSceneMode.Single : OpenSceneMode.Additive);
				firstScene = false;
			}

		}
	}
}
