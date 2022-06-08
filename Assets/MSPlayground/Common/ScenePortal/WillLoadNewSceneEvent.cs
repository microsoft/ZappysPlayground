
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Event fired before loading a new scene through the ScenePortal
    /// </summary>
    /// TODO: added this in case I needed it, turned out I didn't.  Should delete if we end up not using it
    public class WillLoadNewSceneEvent : BaseEvent
    {
        public string SceneToLoad;
    }
}
