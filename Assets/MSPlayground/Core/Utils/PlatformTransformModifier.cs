
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    public class PlatformTransformModifier : MonoBehaviour
    {
#pragma warning disable 0414
        [SerializeField] Vector3 _vrOffset = Vector3.zero;
        [SerializeField] float _vrScaleModifier = 1f;
#pragma warning restore 0414

        private void Start()
        {
#if VRBUILD
            transform.localPosition += _vrOffset;
            transform.localScale *= _vrScaleModifier;
#endif
        }
    }
}
