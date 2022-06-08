using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event for when a power dial of a turbine is being adjused
    /// </summary>
    public class TurbinePowerModuleAdjustedEvent : BaseEvent
    {
        public PowerPanelModuleTurbine Module;
    }
}