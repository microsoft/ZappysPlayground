// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core.Utils;
using UnityEngine;
using MSPlayground.Turbines;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event used to trigger robot animation states
    /// </summary>
    internal class RobotAnimationEvent : BaseEvent
    {
        public RobotController.PowerState PowerState;
    }
}