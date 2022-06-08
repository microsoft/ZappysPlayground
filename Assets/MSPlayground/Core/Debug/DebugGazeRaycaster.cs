
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


namespace MSPlayground.Core
{
    public class DebugGazeRaycaster : MonoBehaviour
    {
        [SerializeField] float _gazeDistance = 10f;
        [SerializeField] LayerMask _gazeLayers = ~0;

        Transform _debugObjectsContainer = null;

        void Start()
        {
            DebugMenu.AddButton("Debug/Debug Gaze", () =>
            {
                gameObject.SetActive(!gameObject.activeSelf);
            });

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _debugObjectsContainer = new GameObject("DebugGazeObjects").transform;
            _debugObjectsContainer.SetParent(transform, true);
        }

        private void OnDisable()
        {
            if (_debugObjectsContainer!=null)
            {
                GameObject.Destroy(_debugObjectsContainer.gameObject);
            }
        }

        private void Update()
        {
            _debugObjectsContainer.gameObject.DestroyChildren();

            DebugRaycastGaze();
        }

        void DebugRaycastGaze()
        {
            List<RaycastHit> hits = PhysicsHelpers.RaycastAllSorted(Camera.main.transform.position, Camera.main.transform.forward, _gazeDistance, _gazeLayers);

            if (hits.Count > 0)
            {
                string hitString = "";

                for (int i = 0; i < hits.Count; i++)
                {
                    RaycastHit hitInfo = hits[i];
                    Vector3 hitPos = hitInfo.point;

                    // ignore MRTK stuff
                    if (hitInfo.collider.name=="AttachTransform" ||
                        hitInfo.collider.name=="NearInteractionModeDetector")
                    {
                        continue;
                    }

                    DebugSphere.Create(_debugObjectsContainer, hitPos, 0.05f);
                    DebugLine.Create(_debugObjectsContainer, hitPos, hitInfo.normal, 0.1f);

                    ARPlane arPlane = hitInfo.collider.GetComponent<ARPlane>();
                    if (arPlane!=null)
                    {
                        hitString += $"arPlane {arPlane.classification} / {arPlane.size.x*arPlane.size.y:0.0} / {arPlane.alignment}\n";
                    }
                    else
                    {
                        hitString += $"{hitInfo.collider.name} / {hitInfo.collider.GetComponent<NearInteractionModeDetector>()} / {hitInfo.collider.gameObject.layer} / {hitPos}\n";
                    }
                }

                ScreenLog.Log(hitString, 0.5f, "Raycast");
            }
        }
    }
}
