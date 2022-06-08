using System;
using MSPlayground.Core.Utils;

namespace MSPlayground.Turbines.Events
{
    /// <summary>
    /// Global event used to trigger random oneshot audio of the given SFX type in the robot model.
    /// </summary>
    public class RobotSFXEvent : BaseEvent
    {
        public SFXType SfxType;
    }
}