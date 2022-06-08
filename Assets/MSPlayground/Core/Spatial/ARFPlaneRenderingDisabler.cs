
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Spatial
{
    /// <summary>
    /// This is a workaround for a problem with ARPlane renderers and colliders.
    /// 
    /// Disabling an ARPlane to stop rendering it causes ARPlaneMeshVisualizer to unsubscribe from 
    /// the boundaryChanged event, meaning the plane will no longer update its collider when rendering 
    /// is disabled.
    /// 
    /// It also re-enabld the MeshRenderer and LineRenderer in Update, so you can't just disable the
    /// renderers and forget about them.
    /// 
    /// This disables the renderers in LateUpdate to reset their state.  Add this component to your 
    /// ARPlane and enable it to disable plane rendering but allow physics to continue updating.
    /// </summary>
    public class ARFPlaneRenderingDisabler : MonoBehaviour
    {
        private void LateUpdate()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer!=null)
            {
                meshRenderer.enabled = false;
            }

            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer!=null)
            {
                lineRenderer.enabled = false;
            }
        }
    }
}
