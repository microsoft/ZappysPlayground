
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    /// <summary>
    /// Base event object, derive your events from this.
    /// </summary>
    public class BaseEvent
	{
	}

    /// <summary>
    /// Generic event system
    /// 
    /// Usage:
    /// 
    /// - class MyEventArgs : EventArgs {}
    /// - void OnEvent(MyEventArgs eventArgs) {} 
    /// 
    /// - Register<MyEventArgs>(OnEvent);
    /// - Unregister<MyEventArgs>(OnEvent);
    /// - Fire<MyEventArgs>(new EventType());
    /// 
    /// </summary>
    public class EventSystem
    {
        Dictionary<Type, Delegate> _eventDict = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Register an event handler for event T
        /// </summary>
        /// <typeparam name="T">event args</typeparam>
        /// <param name="handler">event callback</param>
        public void Register<T>(Action<T> handler) where T : BaseEvent
        {
            Type eventType = typeof(T);

            if (!_eventDict.TryGetValue(eventType, out Delegate eventDelegate))
            {
                _eventDict.Add(eventType, handler);
            }
            else
            {
                _eventDict[eventType] = Delegate.Combine(eventDelegate, handler);
            }

        }

        /// <summary>
        /// Unregister an event handler for event T
        /// </summary>
        /// <typeparam name="T">event args</typeparam>
        /// <param name="handler">event callback</param>
        public void Unregister<T>(Action<T> handler) where T : BaseEvent
        {
            Type eventType = typeof(T);

            if (_eventDict.TryGetValue(eventType, out Delegate eventDelegate))
            {
                _eventDict[eventType] = Delegate.Remove(eventDelegate, handler);
            }
            else
            {
                Debug.LogError($"Event [{eventType}] does not exist");
            }
        }

        /// <summary>
        /// Fire an event
        /// </summary>
        /// <typeparam name="T">event args</typeparam>
        /// <param name="eventArgs">event data</param>
        public void Fire<T>(T eventArgs) where T : BaseEvent
        {
            Type eventType = typeof(T);

            if (_eventDict.TryGetValue(eventType, out Delegate eventDelegate))
            {
                eventDelegate?.DynamicInvoke(eventArgs);
            }
        }
    }
}