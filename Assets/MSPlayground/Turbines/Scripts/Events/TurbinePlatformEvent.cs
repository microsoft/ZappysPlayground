using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event used to monitor turbine and platform interactions
    /// </summary>
    public class TurbinePlatformEvent : BaseEvent
    {
        public TurbineController Turbine;
        public bool Docked;
    }
}