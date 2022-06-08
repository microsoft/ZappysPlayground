using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    class ScreenLog : MonoBehaviour
    {
        static ScreenLog _instance = null;
        [SerializeField] GameObject _screenLogEntryPrefab;
        [SerializeField] Vector2 _firstEntryPos = Vector2.zero;
        [SerializeField] float _textHeight = 0.027f;

        List<ScreenLogEntry> _logEntries = new List<ScreenLogEntry>();
        Dictionary<string, ScreenLogEntry> _logEntryDict = new Dictionary<string, ScreenLogEntry>();

        private void Awake()
        {
            Debug.Assert(_instance==null,"Instance already exists");
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        void DoLog(string logString, float duration, string key)
        {
            ScreenLogEntry entry = null;

            // lookup the entry in the dictionary and re-use it if it exists
            if (key!=null)
            {
                _logEntryDict.TryGetValue(key, out entry);
            }

            // if no existing entry find a new one
            if (entry == null)
            {
                entry = FindUnusedEntry();

                // if no entry found create a new one
                if (entry == null)
                {
                    GameObject newEntryGo = GameObject.Instantiate(_screenLogEntryPrefab);
                    newEntryGo.transform.SetParent(transform);
                    newEntryGo.transform.localPosition = new Vector3(_firstEntryPos.x, _firstEntryPos.y - _textHeight * _logEntries.Count, 0);
                    newEntryGo.transform.localRotation = Quaternion.identity;
                    entry = newEntryGo.GetComponent<ScreenLogEntry>();

                    _logEntries.Add(entry);
                }
            }

            // init the entry
            entry.Init(logString, key, duration);
            entry.gameObject.SetActive(true);

            if (key != null && !_logEntryDict.ContainsKey(key))
            {
                _logEntryDict.Add(key, entry);
            }
        }

        void DoClear(string key, bool assertOnInvalidKey = false)
        {
            if (!_logEntryDict.TryGetValue(key, out ScreenLogEntry entry))
            {
                Debug.Assert(!assertOnInvalidKey, $"No log entry found for {key}");
                return;
            }

            entry.gameObject.SetActive(false);
            _logEntryDict.Remove(key);
        }

        ScreenLogEntry FindUnusedEntry()
        {
            foreach (ScreenLogEntry entry in _logEntries)
            {
                if (!entry.gameObject.activeSelf)
                {
                    return entry;
                }
            }
            return null;
        }

        #region Static Interface

        public static void Log(string logString, float duration=-1.0f, string key=null)
        {
            Debug.Assert(_instance!=null,"ScreenLog==null");
            _instance?.DoLog(logString, duration, key);
        }

        public static void Clear(string key, bool assertOnInvalidKey = false)
        {
            Debug.Assert(_instance != null, "ScreenLog==null");
            _instance?.DoClear(key, assertOnInvalidKey);
        }

        #endregion
    }
}
