using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event for when the window object is placed in the world via Surface Magnetism
    /// </summary>
    public class WindowManipulationEvent : BaseEvent
    {
        public bool PickedUp = false;
    }
}