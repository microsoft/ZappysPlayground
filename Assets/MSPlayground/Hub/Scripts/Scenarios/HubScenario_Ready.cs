
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using MSPlayground.Core;
using MSPlayground.Core.Spatial;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    public class HubScenario_Ready : HubScenario_Base
    {
        public override void EnterState()
        {
            base.EnterState();

            _scenarioResources.InitializeEnvironment();

            if (SceneNavigator.BypassSetupFlow)
            {
                SceneNavigator.AutoLoadSceneAfterSetup();
            }
        }

        private void OnDestroy()
        {
            if (_scenarioResources.RoomGenerator != null)
            {
                _scenarioResources.RoomGenerator.EnableDebugRendering(false);
                _scenarioResources.RoomGenerator.VirtualRoom.SetRenderMode(VirtualWall.RenderMode.Disabled, VirtualWall.RenderMode.Disabled, VirtualWall.RenderMode.Disabled, VirtualWall.RenderMode.Disabled);
            }
        }
    }
}
