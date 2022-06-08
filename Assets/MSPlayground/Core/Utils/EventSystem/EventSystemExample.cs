
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MSPlayground.Core.Utils
{
	public class EventSystemExample : MonoBehaviour
	{
		class Event1 : BaseEvent
		{
			public string EventString;
		}

		private void Update()
		{
			if (Keyboard.current.rKey.wasPressedThisFrame)
			{
				Debug.Log($"REGISTER EVENT {typeof(Event1)}");
				GlobalEventSystem.Register<Event1>(OnEvent1);
			}
			
			if (Keyboard.current.uKey.wasPressedThisFrame)
			{
				Debug.Log($"UNREGISTER EVENT {typeof(Event1)}");
				GlobalEventSystem.Unregister<Event1>(OnEvent1);
			}

			if (Keyboard.current.fKey.wasPressedThisFrame)
			{
				Debug.Log($"FIRE EVENT {typeof(Event1)}");
				GlobalEventSystem.Fire<Event1>(new Event1()
				{
					EventString = $"EventTime = {Time.realtimeSinceStartup}"
				});
			}

		}

		private void OnEvent1(Event1 eventData)
		{
			Debug.Log($"OnEvent1: {eventData.EventString}");
		}
	}
}
