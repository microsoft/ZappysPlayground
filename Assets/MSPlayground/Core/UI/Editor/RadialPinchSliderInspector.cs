// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace MSPlayground.Core.UI.Editor
{
    /// <summary>
    /// Editor inspector support for RadialSlider
    /// Allows editing of the start and end points of the slider using Handles
    /// </summary>
    [CustomEditor(typeof(RadialSlider), true)]
    public class RadialPinchSliderInspector : StatefulInteractableInspector
    {
        private static readonly Color DEBUG_COLOR = Color.cyan;
        private const float DEBUG_HANDLE_SIZE = 0.01f;

        private void OnSceneGUI()
        {
            RadialSlider slider = target as RadialSlider;
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

            DrawLabelWithDottedLine(startPos + (10f * DEBUG_HANDLE_SIZE * Vector3.up), startPos, DEBUG_HANDLE_SIZE, "slider start");
            DrawLabelWithDottedLine(endPos + (10f * DEBUG_HANDLE_SIZE * Vector3.up), endPos, DEBUG_HANDLE_SIZE, "slider end");

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

        private void DrawLabelWithDottedLine(Vector3 labelPos,
            Vector3 dottedLineStart,
            float handleSize,
            string labelText)
        {
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;

            Handles.color = Color.white;
            Handles.Label(labelPos + Vector3.up * handleSize, labelText, labelStyle);
            Handles.DrawDottedLine(dottedLineStart, labelPos, 5f);
        }
    }
}