using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    internal class ScreenLogEntry : MonoBehaviour
    {
        [SerializeField] string _key;
        [SerializeField] TextMesh _text;
        [SerializeField] float _duration;

        internal void Init(string logString, string key, float duration)
        {
            _text.text = logString;
            _key = key;
            _duration = duration;
        }

        void Update()
        {
            if (_duration > 0)
            {
                _duration -= Time.deltaTime;
                if (_duration < 0)
                {
                    if (_key != null)
                    {
                        ScreenLog.Clear(_key);
                    }

                    gameObject.SetActive(false);
                }
            }
        }
    }
}