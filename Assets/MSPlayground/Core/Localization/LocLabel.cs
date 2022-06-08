// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace MSPlayground.Core
{
    /// <summary>
    /// Used for static labels that need to be localized. If the goal is to have dynamic values entered into the text then see DataConsumerLocalizedText.cs
    /// </summary>
    public class LocLabel : MonoBehaviour
    {
        [SerializeField] private LocalizedString _stringReference = new LocalizedString();
        [SerializeField] private TMP_Text _textMeshProComponent;

        private void OnValidate()
        {
            if (_textMeshProComponent == null)
            {
                _textMeshProComponent = GetComponent<TMP_Text>();
            }
        }

        private void OnEnable()
        {
            _stringReference.StringChanged += OnLocalizationUpdate;
            
            UpdateText();
        }

        private void OnDisable()
        {
            _stringReference.StringChanged -= OnLocalizationUpdate;
        }

        private void OnLocalizationUpdate(string localizedString)
        {
            _textMeshProComponent.text = localizedString;
        }

        [ContextMenu(nameof(UpdateText))]
        private void UpdateText()
        {
            if (_stringReference.IsEmpty)
            {
                return;
            }
            _stringReference.RefreshString();
            _textMeshProComponent.text = _stringReference.GetLocalizedString();
        }
    }
}