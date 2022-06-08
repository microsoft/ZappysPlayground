
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    /// <summary>
    /// Static interface to an EventSystem
    /// </summary>
    public static class GlobalEventSystem
    {
        static EventSystem s_eventSystem = new EventSystem();

        public static void Register<T>(Action<T> handler) where T : BaseEvent
        {
            s_eventSystem.Register<T>(handler);
        }

        public static void Unregister<T>(Action<T> handler) where T : BaseEvent
        {
            s_eventSystem.Unregister<T>(handler);
        }

        public static void Fire<T>(T eventArgs) where T : BaseEvent
        {
            s_eventSystem.Fire<T>(eventArgs);
        }
    }
}
