// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core;
using MSPlayground.Core.Spatial;
using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    public class HubScenario_GenerateRoom : HubScenario_Base
    {
        public override void EnterState()
        {
            base.EnterState();

            if (SceneNavigator.BypassSetupFlow || SceneNavigator.BypassScanningAndAnchoring)
            {
                BypassSetupFlow();
            }
            else
            {
                StartCoroutine(SequenceRoutine());
            }
        }

        /// <summary>
        /// Bypass the room scan process and: generate fake walls in AR, load virtual room in VR
        /// Go to the next state upon completion.
        /// </summary>
        void BypassSetupFlow()
        {
            if (SceneNavigator.BypassScanningAndAnchoring)
            {
                GoToNextState();
            }
            else
            {
                GlobalEventSystem.Register<RoomScanCompleteEvent>(OnRoomScanComplete);
                _scenarioResources.RoomGenerator.StartRoomScan(true);

                void OnRoomScanComplete(RoomScanCompleteEvent onComplete)
                {
                    GlobalEventSystem.Unregister<RoomScanCompleteEvent>(OnRoomScanComplete);
                    GoToNextState();
                }
            }
        }

        IEnumerator SequenceRoutine()
        {
            // already anchored
            if (_scenarioResources.GamestateManager.ActiveGameState != null)
            {
                LoadRoomFromGameState();

                // If it is already anchored, we still offer the option for user to rescan space or continue
                UISystem.SpawnDialogPrompt("hub_anchors_found", ()=>GoToNextState(), () => StartCoroutine(ScanRoomRoutine()));
            }
            // not anchored, need to scan and generate the room
            else
            {
                if (_scenarioResources.FailedToFindAnchors)
                {
                    yield return WaitForDialogClosed(_scenarioResources.ShowDialog("hub_lets_rescan_your_space"));
                }
                else
                {
                    yield return WaitForDialogClosed(_scenarioResources.ShowDialog("hub_lets_scan_your_space"));
                }

                yield return StartCoroutine(ScanRoomRoutine());
            }
        }
        
        public override void ExitState()
        {
            UISystem.DespawnAllActivePanels();
            base.ExitState();
        }
        
        public override void SkipState()
        {
            base.SkipState();
            
            // If skipping scenario, then either load the room from active gamestate
            // or generate virtual walls if there is none.
            if (_scenarioResources.GamestateManager.ActiveGameState != null)
            {
                LoadRoomFromGameState();
                GoToNextState();
            }
            else
            {
                _scenarioResources.RoomGenerator.StartRoomScan(true); // bypass flow logic
            }
        }

        void LoadRoomFromGameState()
        {
            _scenarioResources.RoomGenerator.GenerateVirtualRoomFromGamestate(_scenarioResources.GamestateManager
                .ActiveGameState);
            _scenarioResources.RoomGenerator.VirtualRoom.SetRenderMode(VirtualWall.RenderMode.Debug,
                VirtualWall.RenderMode.Debug, VirtualWall.RenderMode.Debug, VirtualWall.RenderMode.Debug);         
        }

        IEnumerator ScanRoomRoutine()
        {
            bool? accepted = null;

            // delete and unload active gamestate
            if (_scenarioResources.GamestateManager.ActiveGameState != null)
            {
                _scenarioResources.GamestateManager.UnloadGamestate();
                _scenarioResources.GamestateManager.DeleteActiveGamestate();
            }

            // rescan until the scan is accepted
            while (!accepted.HasValue || !accepted.Value)
            {
                // start room scan and wait for it to complete
                _scenarioResources.RoomGenerator.Reset();
                _scenarioResources.RoomGenerator.StartRoomScan(Application.isEditor || Application.platform==RuntimePlatform.Android);
                yield return WaitForGlobalEvent<RoomScanCompleteEvent>();

                // validate the scanned room
                accepted = null;
                UISystem.SpawnDialogPrompt("hub_validate_scan", () => accepted = true, () => accepted = false);
                yield return new WaitUntil(() => accepted.HasValue);
            }
            GoToNextState();
        }
    }
}