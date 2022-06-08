// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using MSPlayground.Core.Utils;
using MSPlayground.Scenarios.Turbines;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Core.Scenario
{   
    /// <summary>
    /// Base scenario class as a state managed by ScenarioManager 
    /// </summary>
    public class Scenario : MonoBehaviour
    {
        [SerializeField] Scenario _nextScenarioWhenComplete;
        [SerializeField] ScenarioManager _scenarioManager;
        [SerializeField] GameObjectList _activateOnEnter;

        protected virtual void Reset()
        {
            if (_scenarioManager == null)
            {
                _scenarioManager = GetComponentInParent<ScenarioManager>();
            }
        }

        /// <summary>
        /// Used to reset the state of the instance
        /// This should include any members that help managing the state of this object
        /// </summary>
        [ContextMenu(nameof(ResetState))]
        public virtual void ResetState()
        {
        }

        /// <summary>
        /// The start of a scenario state
        /// </summary>
        public virtual void EnterState()
        {
            _activateOnEnter.SetActive(true);
        }

        /// <summary>
        /// The end of a scenario state
        /// </summary>
        public virtual void ExitState()
        {
        }

        protected void GoToNextState()
        {
            _scenarioManager.ChangeScenario(_nextScenarioWhenComplete);
        }

        /// <summary>
        /// Goto a state other than the next scenario.  Used for branching scenarios.
        /// </summary>
        /// <param name="scenario">the scenario to go to</param>
        protected void GoToCustomState(Scenario scenario)
        {
            _scenarioManager.ChangeScenario(scenario);
        }

        /// <summary>
        /// Skip this state and start the next state
        /// This method is meant to be overriden so that inherited classes can implement their
        /// own Skip logic and set the state of the environment ready for the next state
        /// </summary>
        public virtual void SkipState()
        {
            GoToNextState();
        }

        protected IEnumerator WaitForDialogClosed(GameObject instance)
        {
            while (instance != null && instance.activeInHierarchy)
            {
                yield return null;
            }
        }

        protected IEnumerator WaitForDialogClosed(Panel instance)
        {
            while (instance != null && instance.isActiveAndEnabled)
            {
                yield return null;
            }
        }

        protected IEnumerator WaitForInteraction(StatefulInteractable interactable)
        {
            bool interactionExecuted = false;
            UnityAction listener = () => interactionExecuted = true;

            interactable.OnClicked.AddListener(listener);
            while (interactionExecuted == false)
            {
                yield return null;
            }
            interactable.OnClicked.RemoveListener(listener);
        }

        protected IEnumerator WaitForGlobalEvent<T>(System.Action<T> callback = null, float maxDuration = -1) where T : BaseEvent
        {
            bool eventReceived = false;
            T result = null;
            System.Action<T> eventHandler = (r) =>
            {
                eventReceived = true;
                result = r;
            };

            float totalTime = 0;

            GlobalEventSystem.Register<T>(eventHandler);
            while (eventReceived == false && (maxDuration < 0 || totalTime < maxDuration))
            {
                totalTime += Time.deltaTime;
                yield return null;
            }
            GlobalEventSystem.Unregister<T>(eventHandler);

            callback?.Invoke(result);
        }
    }
}
