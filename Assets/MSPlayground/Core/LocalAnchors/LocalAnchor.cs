
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Microsoft.MixedReality.OpenXR;

namespace MSPlayground.Core
{
    public class LocalAnchor : MonoBehaviour
    {
        ARAnchor _arAnchor;
        
        public string Guid { get; private set; }

        public void Initialize(ARAnchor arAnchor, string guid)
        {
            _arAnchor = arAnchor;
            Guid = guid;
        }
    }
}
