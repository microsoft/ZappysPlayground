// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.SpatialManipulation;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// A manipulator that is similar to ObjectManipulator but allows for the IPokeInteractor
    /// For scrolling UI lists its easier to manipulate with a single finger than a pinch
    /// </summary>
    public class ScrollableListManipulator : ObjectManipulator
    {
        protected override void ApplyRequiredSettings()
        {
            //base.ApplyRequiredSettings();
        }
    }
}
