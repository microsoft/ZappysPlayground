using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Global event for when a part of a target turbine was repaired
    /// </summary>
    public class TurbineModuleRepairedEvent : BaseEvent
    {
        public TurbineController Turbine;
        public TurbineModuleType ModuleType;
    }
}