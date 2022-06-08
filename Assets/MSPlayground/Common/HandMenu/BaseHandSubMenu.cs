// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Sub hand menu found within the hand menu.
    /// </summary>
    public class BaseHandSubMenu : MonoBehaviour
    {
        [SerializeField] protected RectTransform _rectTransform;
        [SerializeField] protected HandMenu _handMenu;
        [SerializeField] protected GameObject _toggledOnGUI;

        private void Reset()
        {
            if (_rectTransform == null)
            {
                _rectTransform = this.GetComponent<RectTransform>();
            }
            if (_handMenu == null)
            {
                _handMenu = this.GetComponentInParent<HandMenu>();
            }
        }

        private void OnEnable()
        {
            UpdateMenuPosition(_handMenu.CurrentHandednessState);
            _handMenu.OnHandednessChangedEvent += UpdateMenuPosition;
            _toggledOnGUI.SetActive(true);
        }

        private void OnDisable()
        {
            _handMenu.OnHandednessChangedEvent -= UpdateMenuPosition;
            _toggledOnGUI.SetActive(false);
        }

        protected void UpdateMenuPosition(Handedness handedness)
        {
            if (handedness != Handedness.None)
            {
                _handMenu.PositionHorizontalMenu(_rectTransform);   
            }   
        }

        /// <summary>
        /// Toggles the state of this gameobject active or not
        /// </summary>
        public virtual void ToggleActive()
        {
            bool active = !gameObject.activeSelf;
            _handMenu.CloseSubMenus();
            SetActive(active);
        }

        /// <summary>
        /// Sets the sub menu active if allowed
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetActive(bool active)
        {
            if (active)
            {
                if (_handMenu.CanOpenSubMenu)
                {
                    UpdateMenuPosition(_handMenu.CurrentHandednessState);
                    gameObject.SetActive(true);
                }   
            }
            else
            {
                gameObject.SetActive(false);    
            }
        }
    }
}