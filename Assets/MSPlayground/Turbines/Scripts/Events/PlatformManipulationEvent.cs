using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event for when the platform is placed in the world
    /// </summary>
    public class PlatformManipulationEvent : BaseEvent
    {
        public bool PickedUp = false;
    }
}