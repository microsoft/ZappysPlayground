
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Core
{
    public class DebugMenuButton : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] PressableButton _button;

        internal void Init(string buttonText, UnityAction callback)
        {
            _text.text = buttonText;
            _button.OnClicked.AddListener(callback);
        }
    }
}
