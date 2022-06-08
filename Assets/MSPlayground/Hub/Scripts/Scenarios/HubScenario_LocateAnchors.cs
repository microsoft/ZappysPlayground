
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.ASA;
using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    public class HubScenario_LocateAnchors : HubScenario_Base
    {
        [SerializeField] float _searchDuration = 5.0f;
        [SerializeField] HubScenario_Base _nextScenarioFoundAnchors;
        [SerializeField] HubScenario_Base _nextScenarioFailedToFindAnchors;

        [Header("Mock ASA Settings")]
        [SerializeField] float _mockASADelay = 3.0f;
        [SerializeField] bool _mockASAFailure = false;

        public override void EnterState()
        {
            base.EnterState();

            // initialize room generator
            _scenarioResources.RoomGenerator.Reset();

            // autoload scene
            if (SceneNavigator.BypassSetupFlow || SceneNavigator.BypassScanningAndAnchoring)
            {
                GoToNextState();
            }
            else
            {
                StartCoroutine(SequenceRoutine());
            }
        }

        IEnumerator SequenceRoutine()
        {
            string locatedAnchorId = null;
            GameObject locatedAnchorObject = null;

            bool mockAnchors = Application.isEditor;

            GameObject dialog = _scenarioResources.ShowDialog("hub_searching_for_anchors");

            if (!mockAnchors)
            {
#if ASA_ENABLED
                // start ASA session
                bool sessionStarted = false;
                ASAManager.Instance.StartSession(() => { sessionStarted = true; });
                yield return new WaitUntil(() => sessionStarted);

                // search for our anchor
                ASAManager.Instance.LocateAnchors(_scenarioResources.GamestateIds);

                // wait for response
                AnchorLocatedEvent anchorLocatedEvent = null;
                yield return WaitForGlobalEvent<AnchorLocatedEvent>((AnchorLocatedEvent eventData) => { anchorLocatedEvent = eventData; }, _searchDuration);

                // add a delay here, the anchor needs time to settle
                yield return new WaitForSeconds(0.5f);

                // stop locating
                ASAManager.Instance.StopLocatingAnchors();
                ASAManager.Instance.StopSession();

                // success?
                if (anchorLocatedEvent != null && anchorLocatedEvent.AnchorObject != null)
                {
                    locatedAnchorId = anchorLocatedEvent.AnchorId;
                    locatedAnchorObject = anchorLocatedEvent.AnchorObject;
                }
#else
                bool initialized = false;
                LocalAnchorManager.Instance.Initialize(() => { initialized = true; });
                yield return new WaitUntil(() => initialized);

                AnchorLocatedEvent anchorLocatedEvent = null;
                LocalAnchorManager.Instance.LocateAnchors(_scenarioResources.GamestateIds);
                yield return WaitForGlobalEvent<AnchorLocatedEvent>((AnchorLocatedEvent eventData) => { anchorLocatedEvent = eventData; }, _searchDuration);
                LocalAnchorManager.Instance.StopLocatingAnchors();

                if (anchorLocatedEvent!=null && anchorLocatedEvent.AnchorObject!=null)
                {
                    locatedAnchorId = anchorLocatedEvent.AnchorId;
                    locatedAnchorObject = anchorLocatedEvent.AnchorObject;
                }
#endif
            }
            else
            {
                yield return new WaitForSeconds(_mockASADelay);

                if (!_mockASAFailure)
                {
                    locatedAnchorId = _scenarioResources.GamestateIds[0];
                    locatedAnchorObject = new GameObject(locatedAnchorId);
                }
            }

            // close dialog
            UISystem.DespawnPanel(dialog);

            // fail state?
            if (locatedAnchorId==null)
            {
                _scenarioResources.FailedToFindAnchors = true;
                GoToCustomState(_nextScenarioFailedToFindAnchors);
            }
            // found anchor, load its gamestate
            else
            {
                _scenarioResources.PlayASASfx();

                _scenarioResources.GamestateManager.LoadGameState(locatedAnchorId, locatedAnchorObject);

                // mark the anchor as don't destroy on load so it persists across scenes
                GameObject.DontDestroyOnLoad(locatedAnchorObject);

                GoToCustomState(_nextScenarioFoundAnchors);
            }
        }
    }
}
