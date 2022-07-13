
using MSPlayground.Core.Utils;
using System.Diagnostics;
using MSPlayground.Common.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace MSPlayground.Core.Scenario
{
    /// <summary>
    /// Manager of scenarios. Holds onto the starting scenario and transitions between scenario instances
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        const string DEBUG_SKIP_CURRENT_SCENARIO = "Scenario/Skip Current Scenario";
        
        [SerializeField] Scenario _runOnStartup;
        [SerializeField] GameObjectList _deactivateOnStart;

        Scenario _currentScenario;

        /// <summary>
        /// The initial Scenario to be ran
        /// </summary>
        public Scenario RunOnStartup
        {
            get => _runOnStartup;
        }

        void Start()
        {
            DebugMenu.AddButton(DEBUG_SKIP_CURRENT_SCENARIO, SkipToNextState);
            StartCoroutine(Coroutines.WaitOneFrame(RunBeginningScenario));
        }

        private void OnDestroy()
        {
            DebugMenu.RemoveButton(DEBUG_SKIP_CURRENT_SCENARIO);
            _currentScenario?.ExitState();
        }

        public void RunBeginningScenario()
        {
            if (_runOnStartup == null)
            {
                UnityEngine.Debug.LogError("No starting Scenario was given to ScenarioManager");
                return;
            }

            _deactivateOnStart.SetActive(false);

            ChangeScenario(_runOnStartup);
        }

        /// <summary>
        /// Changes the current scenario to the given scenario
        /// </summary>
        /// <param name="to">Scenario to change to</param>
        public void ChangeScenario(Scenario to)
        {
            if (_currentScenario != null)
            {
                Log($"Exiting {_currentScenario.GetType().Name}");
                _currentScenario.ExitState();
                _currentScenario.gameObject.SetActive(false);
            }

            _currentScenario = to;

            if (_currentScenario != null)
            {
                Log($"Entering {_currentScenario.GetType().Name}");
                _currentScenario.gameObject.SetActive(true);
                _currentScenario.ResetState();
                _currentScenario.EnterState();
            }
        }

        /// <summary>
        /// Debug logging for ScenarioManager
        /// </summary>
        /// <param name="text"></param>
        [Conditional("DEBUG_LOGGING_ENABLED")]
        public static void Log(string text)
        {
            UnityEngine.Debug.Log($"[{nameof(ScenarioManager)}] {text}");
        }

        /// <summary>
        /// Runs the skip logic of the current scenario that is being executed to change to the next state
        /// </summary>
        [ContextMenu(nameof(SkipToNextState))]
        public void SkipToNextState()
        {
            _currentScenario?.SkipState();
        }

        private void Update()
        {
            if (Keyboard.current[Key.Period].wasPressedThisFrame)
            {
                SkipToNextState();
            }
        }
    }
}