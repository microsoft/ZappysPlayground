
using Microsoft.MixedReality.Toolkit.UX;
using MSPlayground.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MSPlayground.Core
{
    public class DebugMenu : MonoBehaviour
    {
        static DebugMenu _instance;
        [SerializeField] Key _activationKey = Key.Escape;
        [SerializeField] Transform _menuRoot;
        [SerializeField] Transform _menuContentRoot;
        [SerializeField] GameObject _pageButtonPrefab;
        [SerializeField] GameObject _itemButtonPrefab;
        [SerializeField] TextMeshPro _titleText;
        [SerializeField] PressableButton _backButton;
        [SerializeField] float _startDistanceFromCamera = 1.0f;

        PageInfo _rootPage = new PageInfo();
        PageInfo _selectedPage;
        Dictionary<string, ButtonInfo> _buttonInfoDict = new Dictionary<string, ButtonInfo>();

        public delegate void ButtonCallback();

        internal class ButtonInfo
        {
            internal string Key;
            internal string Title;
            internal bool IsPageButton;
            internal ButtonCallback Callback;
        }

        internal class PageInfo
        {
            internal PageInfo PreviousPage;
            internal string Title = "Debug Menu";
            internal List<PageInfo> Pages = new List<PageInfo>();
            internal List<ButtonInfo> Buttons = new List<ButtonInfo>();

            internal PageInfo GetPage(string pageName)
            {
                foreach (PageInfo page in Pages)
                {
                    if (page.Title==pageName)
                    {
                        return page;
                    }
                }

                return null;
            }

            internal void AddPage(PageInfo page)
            {
                Pages.Add(page);
            }

            internal void AddButton(ButtonInfo button)
            {
                Buttons.Add(button);
            }
        }

        void Awake()
        {
            Debug.Assert(_instance==null,"Instance already exists");
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        internal void DoAddButton(string buttonPath, ButtonCallback callback)
        {
            if (!_buttonInfoDict.ContainsKey(buttonPath))
            {
                PageInfo currentPage = _rootPage;

                // add any needed paging buttons
                string[] pathComponents = buttonPath.Split('/', '\\');
                for (int i = 0; i < pathComponents.Length - 1; i++)
                {
                    string pathComponent = pathComponents[i];

                    PageInfo pageInfo = currentPage.GetPage(pathComponent);
                    if (pageInfo == null)
                    {
                        pageInfo = new PageInfo()
                        {
                            Title = pathComponent,
                            PreviousPage = currentPage,
                        };

                        currentPage.AddPage(pageInfo);
                    }

                    currentPage = pageInfo;
                }

                // add the button itself
                ButtonInfo buttonInfo = new ButtonInfo()
                {
                    Key = buttonPath,
                    Title = pathComponents[pathComponents.Length - 1],
                    IsPageButton = false,
                    Callback = callback
                };

                currentPage.AddButton(buttonInfo);

                // add to the button info map so it can be recalled and modified later
                _buttonInfoDict.Add(buttonPath, buttonInfo);
            }
            else
            {
                Debug.LogWarning($"[{this.GetType().ToString()}] There is already a button at path: <{buttonPath}>" +
                                 $"\nCallback was not added.");
            }
        }

        public void BackPage()
        {
            if (_selectedPage.PreviousPage!=null)
            {
                GotoPage(_selectedPage.PreviousPage);
            }
        }

        public void Activate()
        {
            _menuRoot.transform.position = Camera.main.transform.position + Camera.main.transform.forward * _startDistanceFromCamera;
            Quaternion rotation = new Quaternion();
            rotation.SetLookRotation(_menuRoot.transform.position - Camera.main.transform.position);
            _menuRoot.transform.rotation = rotation;

            _menuRoot.gameObject.SetActive(true);
            GotoPage(_selectedPage ?? _rootPage);
        }

        public void Deactivate()
        {
            _menuRoot.gameObject.SetActive(false);
            ResetUI();
        }

        public void ToggleActive()
        {
            if (!_menuRoot.gameObject.activeSelf)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        void Update()
        {
            if (_activationKey != Key.None && Keyboard.current[_activationKey].wasPressedThisFrame)
            {
                ToggleActive();
            }
        }

        void ResetUI()
        {
            _menuContentRoot.gameObject.DestroyChildren();
        }

        void GotoPage(PageInfo page)
        {
            ResetUI();

            _selectedPage = page;

            foreach (PageInfo pageInfo in page.Pages)
            {
                GameObject childPageGo = GameObject.Instantiate(_pageButtonPrefab);
                childPageGo.transform.SetParent(_menuContentRoot,false);
                DebugMenuButton button = childPageGo.GetComponent<DebugMenuButton>();
                button.Init(pageInfo.Title, () =>
                {
                    GotoPage(pageInfo);
                });
            }

            foreach (ButtonInfo buttonInfo in page.Buttons)
            {
                GameObject childButtonGo = GameObject.Instantiate(_itemButtonPrefab);
                childButtonGo.transform.SetParent(_menuContentRoot, false);
                DebugMenuButton button = childButtonGo.GetComponent<DebugMenuButton>();
                button.Init(buttonInfo.Title, () =>
                {
                    buttonInfo.Callback?.Invoke();
                    ToggleActive();
                });
            }

            _titleText.text = page.Title;
            _backButton.enabled = (page.PreviousPage != null);
        }

        #region Static Interface

        /// <summary>
        /// Adds a button to the debug menu
        /// </summary>
        /// <param name="buttonPath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void AddButton(string buttonPath, ButtonCallback callback)
        {
            _instance?.DoAddButton(buttonPath, callback);
        }

        /// <summary>
        /// Removes the button from the menu
        /// </summary>
        /// <param name="buttonPath">The ID given by AddButton()</param>
        public static void RemoveButton(string buttonPath)
        {
            if (_instance != null)
            {
                _instance._buttonInfoDict.Remove(buttonPath);
                foreach (var page in _instance._rootPage.Pages)
                {
                    page.Buttons.RemoveAll((buttonInfo) => buttonInfo.Key.Equals(buttonPath, StringComparison.Ordinal));
                }   
            }
        }

        #endregion
    }
}
