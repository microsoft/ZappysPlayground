// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Common.Tweens;
using MSPlayground.Core;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Scenarios.Turbines;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace MSPlayground.Common
{
    public class HandMenu : MonoBehaviour
    {
        [SerializeField] private HandConstraint _handConstraint;
        [SerializeField] private TweenerGroup _onShowTweens;
        [SerializeField] private ToggleGroup _toggleGroup;
        
        [Header("Sub Menus")]
        [SerializeField] private BaseHandSubMenu[] _subMenus = new BaseHandSubMenu[0];
        [SerializeField] private DynamicHandSubMenu _debugMenu;
        [SerializeField] private DynamicHandSubMenu _doorMenu;
        [SerializeField] private SettingsPanel _settingsPanel;

        [Header("External Panels")]
        [SerializeField] private FeatureSummary _featureSummaryMenu;
        [SerializeField] private DebugMenu _developerMenuPanel;

        [Header("VR Settings")]
#pragma warning disable 0414
        [SerializeField] private Vector3 _vrOffset = new Vector3(0.1f, 0, 0);
        [SerializeField] private float _vrScale = 1.5f;
#pragma warning restore 0414

        private Handedness _currentHandednessState;
        private SolverHandler _solverHandler;
        public Action<Handedness> OnHandednessChangedEvent;

        public Handedness CurrentHandednessState => _currentHandednessState;
        public DynamicHandSubMenu DebugMenu => _debugMenu;
        public DynamicHandSubMenu DoorMenu => _doorMenu;
        public bool CanOpenSubMenu => _onShowTweens.IsPlaying == false;
        public SettingsPanel SettingsPanel => _settingsPanel;
        
        public static HandMenu Instance { get; private set; }

        private void Reset()
        {
            if (_handConstraint == null)
            {
                _handConstraint = GetComponent<HandConstraint>();
            }
        }

        private void Start()
        {
            Instance = this;
            
            _handConstraint.OnFirstHandDetected.AddListener(ShowHandMenu);
            _handConstraint.OnLastHandLost.AddListener(HideHandMenu);
            
            _onShowTweens.gameObject.SetActive(false);
            CloseSubPanels();
            CloseSubMenus();

            _toggleGroup.ClearAllButtonToggleState();

            _solverHandler = GetComponent<SolverHandler>();

#if VRBUILD
            GetComponent<HandBounds>().enabled = false;
            GetComponent<HandConstraintPalmUp>().enabled = false;
            GetComponent<Follow>().enabled = true;
            _solverHandler.TrackedTargetType = TrackedObjectType.Head;
            transform.localScale *= _vrScale;
#else
            GetComponent<HandBounds>().enabled = true;
            GetComponent<HandConstraintPalmUp>().enabled = true;
            GetComponent<Follow>().enabled = false;
#endif
        }

        private void OnDestroy()
        {
            _handConstraint.OnFirstHandDetected.RemoveListener(ShowHandMenu);
            _handConstraint.OnLastHandLost.RemoveListener(HideHandMenu);
        }

        private void OnEnable()
        {
            GlobalEventSystem.Register<WillLoadNewSceneEvent>(OnWillLoadNewSceneEvent);
        }

        private void OnDisable()
        {
            GlobalEventSystem.Unregister<WillLoadNewSceneEvent>(OnWillLoadNewSceneEvent);
        }

        private void OnWillLoadNewSceneEvent(WillLoadNewSceneEvent obj)
        {
            HideHandMenu();
            CloseSubPanels();
        }

        private void HideHandMenu()
        {
            _onShowTweens.OnGroupCompleteEvent = null;
        
            void OnTweenComplete()
            {
                _onShowTweens.OnGroupCompleteEvent -= OnTweenComplete;
                _onShowTweens.gameObject.SetActive(false);
            }

            _onShowTweens.OnGroupCompleteEvent += OnTweenComplete;
            _onShowTweens.PlayReverse();
            
            CloseSubMenus();
        }

        /// <summary>
        /// Close all known submenus
        /// </summary>
        public void CloseSubMenus()
        {
            foreach (var submenu in _subMenus)
            {
                submenu.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Close all known panels
        /// </summary>
        public void CloseSubPanels()
        {
            if (_featureSummaryMenu != null)
            {
                _featureSummaryMenu.gameObject.SetActive(false);   
            }
            if (_developerMenuPanel != null)
            {
                _developerMenuPanel.Deactivate();   
            }
        }

        private void ShowHandMenu()
        {
            if (_onShowTweens.IsPlaying == false || _onShowTweens.isActiveAndEnabled == false)
            {
                _toggleGroup.ClearAllButtonToggleState();
                
                _onShowTweens.OnGroupCompleteEvent = null;
                _onShowTweens.PlayForward();
                _onShowTweens.ResetToBeginning();
                _onShowTweens.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Expecting a horizontal menu RectTransform so that it may pivot and flip based on which hand is being raised
        /// </summary>
        /// <param name="menu"></param>
        public void PositionHorizontalMenu(RectTransform menu)
        {
            Vector2 pivot = menu.pivot;
            Vector3 pos = menu.localPosition;
            pos.x = Mathf.Abs(pos.x);

            if (_currentHandednessState == Handedness.Right)
            {
                pos.x = -pos.x;
                pivot.x = 1;
            }
            else if (_currentHandednessState == Handedness.Left)
            {
                pivot.x = 0;
            }

            menu.pivot = pivot;
            menu.localPosition = pos;
        }

        private void Update()
        {
            Handedness prevHandedness = _handConstraint.Handedness;
            if (prevHandedness != _currentHandednessState)
            {
                _currentHandednessState = _handConstraint.Handedness;
                OnHandednessChangedEvent?.Invoke(_currentHandednessState);
            }

#if VRBUILD
            VRUpdate();
#endif
        }

#if VRBUILD
        void VRUpdate()
        {
            bool rightButtonPressed = false;
            bool leftButtonPressed = false;

            if (Application.isEditor)
            {
                if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.P].wasPressedThisFrame)
                {
                    if (!_onShowTweens.isActiveAndEnabled)
                    {
                        ShowHandMenu();
                    }
                    else
                    {
                        HideHandMenu();
                    }
                }
            }
            else
            {
                InputDevice leftHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out leftButtonPressed);

                InputDevice rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out rightButtonPressed);

                if (rightButtonPressed || leftButtonPressed)
                {
                    if (rightButtonPressed)
                    {
                        _solverHandler.AdditionalOffset = _vrOffset;
                    }
                    else
                    {
                        _solverHandler.AdditionalOffset = new Vector3(-_vrOffset.x, _vrOffset.y, _vrOffset.z);
                    }

                    if (!_onShowTweens.isActiveAndEnabled)
                    {
                        ShowHandMenu();
                    }
                }
                else
                {
                    if (_onShowTweens.isActiveAndEnabled && !_onShowTweens.IsPlaying)
                    {
                        HideHandMenu();
                    }
                }
            }
        }
#endif

        [ContextMenu(nameof(FindAllSubmenuInChildren))]
        private void FindAllSubmenuInChildren()
        {
            _subMenus = this.GetComponentsInChildren<BaseHandSubMenu>(true);
        }

        /// <summary>
        /// Display feature summary
        /// </summary>
        public void ToggleFeatureSummary()
        {
            bool enable = !_featureSummaryMenu.isActiveAndEnabled;
            if (CanOpenSubMenu)
            {
                if (enable)
                {
                    CloseSubPanels();
                    CloseSubMenus();   
                }
                _featureSummaryMenu.gameObject.SetActive(enable);
            }
        }

        /// <summary>
        /// Display Developer Menu panel
        /// </summary>
        public void ToggleDeveloperMenuPanel()
        {
            bool enable = !_developerMenuPanel.isActiveAndEnabled;
            if (CanOpenSubMenu)
            {
                if (enable)
                {
                    CloseSubPanels();
                    CloseSubMenus();
                }
                
                _developerMenuPanel.ToggleActive();
            }
        }
    }
}