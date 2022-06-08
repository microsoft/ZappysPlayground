using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MSPlayground.Core.Spatial
{
    public static class ARFPlaneExtension
    {
        public static void EnableRendering(this ARPlane plane, bool enabled)
        {
            ARFPlaneRenderingDisabler disabler = plane.GetComponent<ARFPlaneRenderingDisabler>();

            if (disabler==null)
            {
                disabler = plane.gameObject.AddComponent<ARFPlaneRenderingDisabler>();
            }

            disabler.enabled = !enabled;
        }
    }
}
