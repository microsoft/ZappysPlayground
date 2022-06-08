
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Gamestate
{
    /// <summary>
    /// GameState that is managed by the GameStateManager
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// Indicates that the anchor was succesfully saved.
        /// </summary>
        public bool AnchorSaved;

        /// <summary>
        /// AnchorGo is the ASA anchor for the gamespace.
        /// </summary>
        [System.NonSerialized] public GameObject AnchorObject;

        /// <summary>
        /// The ASA id of the anchor
        /// </summary>
        public string AnchorId;

        /// <summary>
        /// Username saved in the hub
        /// </summary>
        public string UserName = null;

        /// <summary>
        /// Hub specific gamestate
        /// </summary>
        public HubGameState HubState = new HubGameState();

        /// <summary>
        /// Turbines specific gamestate
        /// </summary>
        public TurbineGameState TurbineState = new TurbineGameState();
    }

    /// <summary>
    /// Dictionary of reference points keyed by id.
    /// Can save anchor relative reference points and look them up later.  Used to ensure objects can be loaded into the same locations.
    /// </summary>
    public class ReferencePointDict
    {
        /// <summary>
        /// Transform values required to re-transform a point back into anchor space
        /// </summary>
        public class ReferencePoint
        {
            public SerializableV3 Position;
            public SerializableV3 Forward;
            public SerializableV3 Up;
            public SerializableV3 Scale;

            public void ApplyToTransform(Transform targetTransform, Transform anchorTransform)
            {
                Vector3 pos = anchorTransform.TransformPoint(Position.ToV3());
                Vector3 fwd = anchorTransform.TransformDirection(Forward.ToV3());
                Vector3 up = anchorTransform.TransformDirection(Up.ToV3());
                Vector3 scale = Scale.ToV3();

                targetTransform.position = pos;
                targetTransform.rotation = Quaternion.LookRotation(fwd, up);
                targetTransform.localScale = scale;
            }
        }

        public Dictionary<string, ReferencePoint> RefPoints = new Dictionary<string, ReferencePoint>();

        /// <summary>
        /// Add a reference point to track
        /// </summary>
        /// <param name="id">id of the reference point</param>
        /// <param name="refTransform">reference transform to add</param>
        /// <param name="anchorTransform">the anchor transform</param>
        public void AddRefPoint(string id, Transform refTransform, Transform anchorTransform)
        {
            // get the inverse point
            ReferencePoint refPoint = new ReferencePoint()
            {
                Position = new SerializableV3(anchorTransform.InverseTransformPoint(refTransform.position)),
                Forward = new SerializableV3(anchorTransform.InverseTransformDirection(refTransform.forward)),
                Up = new SerializableV3(anchorTransform.InverseTransformDirection(refTransform.up)),
                Scale = new SerializableV3(refTransform.localScale)
            };

            // if key already exists remove it
            RefPoints.Remove(id);

            // add the new key
            RefPoints.Add(id, refPoint);
        }

        /// <summary>
        /// Apply a reference point back to a transform
        /// </summary>
        /// <param name="id">the id of the reference point</param>
        /// <param name="refTransform">the transform to apply the ref point to</param>
        /// <param name="anchorTransform">the anchor transform</param>
        /// <returns></returns>
        public void ApplyRefPoint(string id, Transform refTransform, Transform anchorTransform)
        {
            if (RefPoints.TryGetValue(id, out ReferencePoint refPoint))
            {
                refPoint.ApplyToTransform(refTransform, anchorTransform);
            }
            else
            { 
                Debug.LogError($"Failed to lookup ReferencePoint {id}");
            }
        }

        public ReferencePoint GetRefPoint(string id)
        {
            RefPoints.TryGetValue(id, out ReferencePoint refPoint);
            return refPoint;
        }
    }

    /// <summary>
    /// BaseGameState
    /// </summary>
    public abstract class BaseGameState
    {
        public ReferencePointDict ReferencePoints = new ReferencePointDict();
    }

    public class HubGameState : BaseGameState
    {
    }

    public class TurbineGameState : BaseGameState
    {
    }
}
