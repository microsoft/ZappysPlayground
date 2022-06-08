
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.ASA;
using MSPlayground.Core.Gamestate;
using System.Collections;
using MSPlayground.Core;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    public class HubScenario_SaveAnchors : HubScenario_Base
    {
        bool _saveAnchorSuccess = false;
        int _numSaveAttempts = 0;

        [SerializeField] int _maxSaveAttempts = 3;

        [Header("Mock ASA Settings")]
        [SerializeField] float _mockASADelay = 3.0f;
        [SerializeField] bool _mockASAFailure = false;

        public override void EnterState()
        {
            base.EnterState();
            StartCoroutine(SequenceRoutine());
        }

        IEnumerator SequenceRoutine()
        {
            // if already anchored then nothing to do
            if (_scenarioResources.GamestateManager.ActiveGameState!=null)
            {
                GoToNextState();
                yield break;
            }

            // create placeholder anchor
            GameObject savedAnchorObject = new GameObject("PlaceholderAnchor");
            savedAnchorObject.transform.position = Vector3.zero;

            // bypass scanning and anchoring?  save game with no virtual room and continue
            if (SceneNavigator.BypassScanningAndAnchoring)
            {
                _scenarioResources.GamestateManager.CreateGameState(_saveAnchorSuccess, "SaveGameId", savedAnchorObject);
                _scenarioResources.GamestateManager.SaveGameState();
                GoToNextState();
                yield break;
            }

            // allow mockASA for anchor testing
            bool mockASA = Application.isEditor || SceneNavigator.BypassSetupFlow;

            GameObject dialog = _scenarioResources.ShowDialog("hub_saving_anchors", rotateToCamera: true);

            _numSaveAttempts = 0;

            // keep trying until success?  after a failure we will probably never actually succeed
            string savedAnchorId = null;
            while (savedAnchorId == null)
            {
                if (!mockASA)
                {
#if ASA_ENABLED
                    // start ASA session
                    bool sessionStarted = false;
                    ASAManager.Instance.StartSession(() => { sessionStarted = true; });
                    yield return new WaitUntil(() => sessionStarted);

                    // create the anchor
                    ASAManager.Instance.CreateAnchorForGameObject(savedAnchorObject);

                    // wait for creation
                    yield return WaitForGlobalEvent<AnchorCreatedEvent>((AnchorCreatedEvent eventData) =>
                    {
                        if (eventData.AnchorObject != null)
                        {
                            _scenarioResources.PlayASASfx();
                            _saveAnchorSuccess = true;
                            savedAnchorId = eventData.AnchorId;
                            savedAnchorObject = eventData.AnchorObject;
                        }
                    });

                    // stop the anchoring session
                    ASAManager.Instance.StopSession();
#else
                    bool initialized = false;
                    LocalAnchorManager.Instance.Initialize(() => { initialized = true; });
                    yield return new WaitUntil(() => initialized);

                    AnchorCreatedEvent eventData = LocalAnchorManager.Instance.CreateAnchorForGameObject(savedAnchorObject);
                    if (eventData.AnchorObject != null)
                    {
                        _scenarioResources.PlayASASfx();
                        _saveAnchorSuccess = true;
                        savedAnchorId = eventData.AnchorId;
                        savedAnchorObject = eventData.AnchorObject;
                    }
#endif
                }
                else
                {
                    yield return new WaitForSeconds(_mockASADelay);

                    if (!_mockASAFailure)
                    {
                        SaveUnanchored();
                    }
                }

                // break out after max num failures
                if (savedAnchorId == null)
                {
                    _numSaveAttempts++;

                    if (_numSaveAttempts >= _maxSaveAttempts)
                    {
                        _saveAnchorSuccess = false;
                        savedAnchorId = "FailedAnchor";
                        break;
                    }
                }
            }

            UISystem.DespawnPanel(dialog);

            // rename the anchor for debugging
            savedAnchorObject.name = $"Anchor.{savedAnchorId}";

            // mark the anchor as don't destroy on load so it persists across scenes
            GameObject.DontDestroyOnLoad(savedAnchorObject);

            // create gamestate
            _scenarioResources.GamestateManager.CreateGameState(_saveAnchorSuccess, savedAnchorId, savedAnchorObject);

            // save out the virtual walls to gamestate
            _scenarioResources.RoomGenerator.WriteVirtualRoomToGamestate(_scenarioResources.GamestateManager.ActiveGameState);

            // save the gamestate only on anchor success.  this is because we name the gamestate files by their anchorid and we
            //  don't have one.  otherwise we would need to open all the gamestates to test anchor success before running our
            //  anchor search.
            if (_saveAnchorSuccess)
            {
                _scenarioResources.GamestateManager.SaveGameState();
            }

            GoToNextState();

            void SaveUnanchored()
            {
                savedAnchorId = "EditorAnchor";
                _saveAnchorSuccess = true;
            }
        }
    }
}
