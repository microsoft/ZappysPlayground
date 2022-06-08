using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event raised when a turbine instance is selected
    /// </summary>
    public class TurbineSelectedEvent : BaseEvent
    {
        public TurbineController Turbine;
    }
}