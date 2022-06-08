using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Event raised for when the power output or available changes
    /// </summary>
    public class PowerUpdatedEvent : BaseEvent
    {
        public float OldOutputPower { get; private set; }
        public float OldAvailablePower { get; private set; }
        public float NewOutputPower { get; private set; }
        public float NewAvailablePower { get; private set; }
        public bool IsOutputPowerEnough { get; private set; }
        public bool IsAvailablePowerEnough { get; private set; }

        public PowerUpdatedEvent()
        {
            
        }

        public PowerUpdatedEvent(float oldOutputPower, float oldAvailablePower, float newOutputPower, float newAvailablePower, bool isOutputPowerEnough, bool isAvailablePowerEnough)
        {
            UpdateValues(oldOutputPower, oldAvailablePower, newOutputPower, newAvailablePower, isOutputPowerEnough, isAvailablePowerEnough);
        }

        /// <summary>
        /// Update all fields of this event
        /// This is to enable the reuse of this instance
        /// </summary>
        /// <param name="oldOutputPower"></param>
        /// <param name="oldAvailablePower"></param>
        /// <param name="newOutputPower"></param>
        /// <param name="newAvailablePower"></param>
        /// <param name="isOutputPowerEnough"></param>
        public void UpdateValues(float oldOutputPower, float oldAvailablePower, float newOutputPower, float newAvailablePower, bool isOutputPowerEnough, bool isAvailablePowerEnough)
        {
            this.OldOutputPower = oldOutputPower;
            this.OldAvailablePower = oldAvailablePower;
            this.NewOutputPower = newOutputPower;
            this.NewAvailablePower = newAvailablePower;
            this.IsOutputPowerEnough = isOutputPowerEnough;
            this.IsAvailablePowerEnough = isAvailablePowerEnough;
        }
    }
}