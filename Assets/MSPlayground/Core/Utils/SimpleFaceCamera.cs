
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    public class SimpleFaceCamera : MonoBehaviour
    {
        [SerializeField] bool _flipDirection = false;

        private void Update()
        {
            if (Camera.main!=null)
            {             
                if (_flipDirection)
                {
                    transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
                }
                else
                {
                    transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
                }
            }
        }
    }
}
