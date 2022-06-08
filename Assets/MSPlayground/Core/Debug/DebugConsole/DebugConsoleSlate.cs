
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MSPlayground.Core
{
    public class DebugConsoleSlate : MonoBehaviour
    {
        [SerializeField] TextSlateController _textSlateController;
        [SerializeField] GameObject _slateRoot;
        [SerializeField] bool _startActive = false;
        [SerializeField] Color _logColor = Color.white;
        [SerializeField] Color _warningColor = Color.yellow;
        [SerializeField] Color _errorColor = Color.red;
        [SerializeField] Key _activationKey = Key.Escape;
        [SerializeField] float _startDistanceFromCamera = 1.0f;
#if PHRASE_RECOGNITION_ACTIVE
        [SerializeField] string _activationPhrase = "Toggle Debug Console";
#endif

        private void Awake()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void Start()
        {
            _slateRoot.SetActive(_startActive);

#if PHRASE_RECOGNITION_ACTIVE
            PhraseRecognitionSubsystem phraseRecognizer = SpeechUtils.GetSubsystem();
            Debug.Log($"PhraseRecognizer = {phraseRecognizer}");
            if (phraseRecognizer != null)
            {
                phraseRecognizer.CreateOrGetEventForPhrase(_activationPhrase).AddListener(() => OnKeywordRecognized(_activationPhrase));                        
            }
#endif

            DebugMenu.AddButton("Toggle Log Console", ToggleActive);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void OnKeywordRecognized(string keyword)
        {
            ToggleActive();
        }

        void OnLogMessageReceived(string logString, string stackTrace, LogType logType)
        {
            if (Application.isPlaying == false)
            {
                return;
            }
            
            Color textColor = _logColor;

            if (logType == LogType.Warning)
            {
                textColor = _warningColor;
            }
            else if (logType == LogType.Error || logType == LogType.Exception || logType == LogType.Assert)
            {
                textColor = _errorColor;
            }

            _textSlateController.AddText(logString, textColor);
        }

        public void ToggleActive()
        {
            if (!_textSlateController.gameObject.activeSelf)
            {
                _slateRoot.SetActive(true);

                // Offset the position a bit so it doesnt overlap with debug menu on active
                _slateRoot.transform.position = Camera.main.transform.position + new Vector3(0.5f,0,0) + Camera.main.transform.forward * _startDistanceFromCamera;
                
                Quaternion rotation = new Quaternion();
                rotation.SetLookRotation(_slateRoot.transform.position - Camera.main.transform.position, Vector3.up);
                _slateRoot.transform.rotation = rotation;
            }
            else
            {
                _slateRoot.SetActive(false);
            }
        }

        void Update()
        {
            if (Keyboard.current[_activationKey].wasPressedThisFrame)
            {
                ToggleActive();
            }
        }
    }
}