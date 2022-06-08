// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Hacky fix for TMP_InputField behaviour on hololens.
    /// </summary>
    public class MRInputFieldFix : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField = null;

        private void Reset()
        {
            if (_inputField == null)
            {
                _inputField = GetComponent<TMP_InputField>();
            }
        }

        private void Start()
        {
            _inputField.onTouchScreenKeyboardStatusChanged.AddListener(OnTouchScreenKeyboardStatusChanged);
        }

        private void OnDestroy()
        {
            _inputField.onTouchScreenKeyboardStatusChanged.RemoveListener(OnTouchScreenKeyboardStatusChanged);
        }

        private void OnTouchScreenKeyboardStatusChanged(TouchScreenKeyboard.Status status)
        {
            // AG: Need to manually set selected object to null since it will not automatically deselect in Hololens.
            // If it can't deselect that means that when the touchscreen keyboard is closed, the user will not be able to
            // tap the input field to spawn the keyboard again since it thinks it's already selected.
            // Hacky fix found from:
            // https://stackoverflow.com/questions/56145437/how-to-make-textmesh-pro-input-field-deselect-on-enter-key
            switch (status)
            {
                case TouchScreenKeyboard.Status.Done: // Triggered when touchscreen keyboard is closed
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    break;
            }
        }
    }
}