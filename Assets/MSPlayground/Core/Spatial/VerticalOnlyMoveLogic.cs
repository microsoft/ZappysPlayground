// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;

namespace MSPlayground.Core.Spatial
{
    /// <summary>
    /// Implements a generic movelogic that works for most/all XRI interactors,
    /// assuming a well-defined attachTransform.
    ///
    /// This ManipulationLogic is based off of Microsoft.MixedReality.Toolkit.SpatialManipulation.MoveLogic.
    /// The difference is that this restricts any translations to just the y-axis.
    /// 
    /// Usage:
    /// When a manipulation starts, call Setup.
    /// Call Update any time to update the move logic and get a new rotation for the object.
    /// </summary>
    public class VerticalOnlyMoveLogic : ManipulationLogic<Vector3>
    {
        private Vector3 _objectLocalAttachPoint;

        /// <inheritdoc />
        public override void Setup(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget)
        {
            base.Setup(interactors, interactable, currentTarget);

            Vector3 attachCentroid = GetAttachCentroid(interactors, interactable);

            _objectLocalAttachPoint = Quaternion.Inverse(currentTarget.Rotation) * (attachCentroid - currentTarget.Position);
            _objectLocalAttachPoint = _objectLocalAttachPoint.Div(currentTarget.Scale);
        }

        /// <inheritdoc />
        public override Vector3 Update(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget, bool centeredAnchor)
        {
            base.Update(interactors, interactable, currentTarget, centeredAnchor);

            Vector3 attachCentroid = GetAttachCentroid(interactors, interactable);

            Vector3 scaledLocalAttach = Vector3.Scale(_objectLocalAttachPoint, currentTarget.Scale);
            Vector3 worldAttachPoint = currentTarget.Rotation * scaledLocalAttach + currentTarget.Position;
            Vector3 offset = (attachCentroid - worldAttachPoint);
            return currentTarget.Position + interactable.transform.up * offset.y;
        }

        private Vector3 GetAttachCentroid(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable)
        {
            Vector3 sumPos = Vector3.zero;
            int count = 0;
            foreach (IXRSelectInteractor interactor in interactors)
            {
                sumPos += interactor.GetAttachTransform(interactable).position;
                count++;
            }
        
            return sumPos / Mathf.Max(1, count);
        }
    }
}