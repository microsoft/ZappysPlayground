
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Common;
using MSPlayground.Core.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MSPlayground.Core.Gamestate
{
    /// <summary>
    /// GamestateManager tracks gamestate, can serialize and deserialize it.
    /// Gamestate is serialized to file named by its id which is the ASA id of the anchor defining the game space
    /// </summary>
    public class GamestateManager : MonoBehaviour
    {
        public const string DEFAULT_GAMESTATE_ID = "Default";
        const string GAMESTATE_FOLDER = "Gamestate";
        const string HUB_SCENE_NAME = "Hub";
        string _gamestatePath;

        /// <summary>
        /// The active gamestate
        /// </summary>
        public GameState ActiveGameState { get; private set; }

        /// <summary>
        /// Get the pathname for a given gamestate id
        /// </summary>
        /// <param name="id">the gamestate id</param>
        /// <returns></returns>
        string GamestateIdToPath(string id)
        {
            string pathName = Path.Combine(Application.persistentDataPath, GAMESTATE_FOLDER, id);
            return pathName;
        }

        /// <summary>
        /// Discover gamestates on start
        /// </summary>
        void Start()
        {
            _gamestatePath = Path.Combine(Application.persistentDataPath, GAMESTATE_FOLDER);

            if (!Directory.Exists(_gamestatePath))
            {
                Directory.CreateDirectory(_gamestatePath);
            }

            EnumerateGameStates();

            GlobalEventSystem.Register<WillLoadNewSceneEvent>(OnWillLoadNewScene);

            DebugMenu.AddButton("Gamestate/Clear all gamestates", ClearAllGamestates);
        }

        /// <summary>
        /// Called just before loading a new scene.  If we're loading into the hub unload the gamestate first.
        /// </summary>
        /// <param name="eventData"></param>
        void OnWillLoadNewScene(WillLoadNewSceneEvent eventData)
        {
            if (eventData.SceneToLoad == HUB_SCENE_NAME)
            {
                UnloadGamestate();
            }
        }

        /// <summary>
        /// Delete the active gamestate file
        /// </summary>
        public void DeleteActiveGamestate()
        {
            if (ActiveGameState != null)
            {
                string path = GamestateIdToPath(ActiveGameState.AnchorId);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Unload any active gamestate.  
        /// Destroy any created anchors.  This will also destroy any content that has been parented to the anchor.
        /// </summary>
        public void UnloadGamestate()
        {
            if (ActiveGameState!=null)
            {
                if (ActiveGameState.AnchorObject!=null)
                {
                    GameObject.Destroy(ActiveGameState.AnchorObject);
                }
            }

            ActiveGameState = null;
        }

        /// <summary>
        /// Look up all saved gamestates
        /// </summary>
        /// <returns>array of gamestates</returns>
        public string[] EnumerateGameStates()
        {
            string[] gamestates = null;
            string[] fileNames = Directory.GetFiles(_gamestatePath);

            if (fileNames == null || fileNames.Length==0)
            {
                gamestates = new string[0];
            }
            else
            {
                gamestates = new string[fileNames.Length];

                for (int i = 0; i < fileNames.Length; i++)
                {
                    gamestates[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
                }
            }

            return gamestates;
        }

        /// <summary>
        /// Delete all saved gamestates.  Clear the GamestateIds array
        /// </summary>
        public void ClearAllGamestates()
        {
            string[] fileNames = Directory.GetFiles(_gamestatePath);

            for (int i = 0; i < fileNames.Length; i++)
            {
                File.Delete(fileNames[i]);
            }
        }

        /// <summary>
        /// Load a gamestate and make it the ActiveGameState
        /// </summary>
        /// <param name="id">id of the gamestate</param>
        /// <returns>success status</returns>
        public GameState LoadGameState(string id, GameObject anchorObject)
        {
            string pathName = GamestateIdToPath(id);

            if (File.Exists(pathName))
            {
                string json = File.ReadAllText(pathName);
                if (!string.IsNullOrEmpty(json))
                {
                    ActiveGameState = JsonConvert.DeserializeObject<GameState>(json);
                }
            }

            if (ActiveGameState == null)
            {
                Debug.LogError($"Failed to load gamestate {id}");
                return null;
            }

            ActiveGameState.AnchorObject = anchorObject;

            return ActiveGameState;
        }

        /// <summary>
        /// Create a new gamestate and assign it to the active state
        /// </summary>
        /// <param name="id"></param>
        public void CreateGameState(bool anchorSaved, string id, GameObject anchorObject)
        {
            ActiveGameState = new GameState()
            {
                AnchorSaved = anchorSaved,
                AnchorId = id,
                AnchorObject = anchorObject
            };
        }

        /// <summary>
        /// Save the active gamestate.  Delete gamestate if already exists.
        /// </summary>
        public void SaveGameState()
        {
            Debug.Log($"SaveGameState: {ActiveGameState.AnchorId}");

            if (ActiveGameState == null)
            {
                Debug.LogWarning($"No active gamestate");
                return;
            }

            string json = JsonConvert.SerializeObject(ActiveGameState);

            string pathName = GamestateIdToPath(ActiveGameState.AnchorId);

            if (File.Exists(pathName))
            {
                File.Delete(pathName);
            }

            File.WriteAllText(pathName, json);

            // re-enumerate the gamestates so they stay valid
            EnumerateGameStates();
        }
    }
}

