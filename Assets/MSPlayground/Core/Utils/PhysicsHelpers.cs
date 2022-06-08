
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    public static class PhysicsHelpers
    {
        public static List<RaycastHit> RaycastAllSorted(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
        {
            List<RaycastHit> hitList = null;

            RaycastHit[] hitsArray = Physics.RaycastAll(origin, direction, maxDistance, layerMask);

            hitList = new List<RaycastHit>(hitsArray);

            if (hitsArray.Length > 0)
            {
                hitList.Sort((x, y) => x.distance < y.distance ? -1 : 1);
            }

            return hitList;
        }
    }
}