using System;

namespace MSPlayground.Turbines
{
    [Flags]
    public enum TurbineModuleType
    {
        None = 0,
        Rotor = 1,
        Nacelle = 2,
        Tower = 3,
    }
}