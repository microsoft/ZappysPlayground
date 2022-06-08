using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// When the user pushed the overall power button
    /// </summary>
    public class PowerEngagedEvent : BaseEvent
    {
        public bool Success;
    }
}