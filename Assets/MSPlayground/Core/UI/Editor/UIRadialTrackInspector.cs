// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;
using UnityEngine;

namespace MSPlayground.Core.UI.Editor
{
    /// <summary>
    /// Editor inspector support for UIRadialTrack
    /// Allows editing of the start and end points of the slider using Handles
    /// </summary>
    [CustomEditor(typeof(UIRadialTrack), true)]
    public class UIRadialTrackInspector : UnityEditor.Editor
    {
        private static readonly Color DEBUG_COLOR = Color.cyan;
        private const float DEBUG_HANDLE_SIZE = 0.01f;

        private void OnSceneGUI()
        {
            UIRadialTrack slider = target as UIRadialTrack;
            if (slider == null)
            {
                return;
            }

            Transform sliderTransform = slider.transform;
            Vector3 sliderPosition = sliderTransform.position;
            Vector3 sliderForward = -sliderTransform.forward;

            Color debugColor = DEBUG_COLOR;
            Handles.color = debugColor;
            Handles.DrawWireDisc(sliderPosition, sliderForward, slider.Radius);

            EditorGUI.BeginChangeCheck();

            Vector3 dir;

            dir = Quaternion.AngleAxis(slider.StartDegree, sliderForward) * sliderTransform.up;
            Vector3 startPos = Handles.FreeMoveHandle(RadialHelpers.ProjectOnCircumference(sliderPosition + dir, sliderTransform, slider.Radius),
                Quaternion.identity,
                DEBUG_HANDLE_SIZE,
                Vector3.zero,
                Handles.SphereHandleCap);
            slider.StartDegree += Vector3.SignedAngle(dir, startPos - sliderPosition, sliderForward);


            dir = Quaternion.AngleAxis(slider.StartDegree + slider.DeltaDegree, sliderForward) * sliderTransform.up;
            Vector3 endPos = Handles.FreeMoveHandle(RadialHelpers.ProjectOnCircumference(sliderPosition + dir, sliderTransform, slider.Radius),
                Quaternion.identity,
                DEBUG_HANDLE_SIZE,
                Vector3.zero,
                Handles.SphereHandleCap);
            slider.DeltaDegree += Vector3.SignedAngle(dir, endPos - sliderPosition, sliderForward);

            EditorGUI.EndChangeCheck();

            debugColor.a = 0.1f;
            Handles.color = debugColor;
            Handles.DrawSolidArc(
                sliderPosition,
                sliderForward,
                startPos - sliderPosition,
                slider.DeltaDegree,
                slider.Radius
            );
        }
    }
}