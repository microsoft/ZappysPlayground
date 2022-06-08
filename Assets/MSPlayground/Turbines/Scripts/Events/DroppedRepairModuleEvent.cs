using MSPlayground.Core.Utils;
using UnityEngine;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event for when a repair module is dropped into the world
    /// </summary>
    public class DroppedRepairModuleEvent : BaseEvent
    {
        public GameObject ModuleObject;
        public TurbineModuleType ModuleType;
    }
}