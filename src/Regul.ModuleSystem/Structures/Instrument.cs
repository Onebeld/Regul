using System;

namespace Regul.ModuleSystem.Structures;

public class Instrument
{
    /// <summary>
    /// Instrument's name. Can also be a key for localization.
    /// </summary>
    public string? Name { get; }
    /// <summary>
    /// The entry point to start the <see cref="Instrument"/>.
    /// </summary>
    public Action Execute { get; }
    
    /// <summary>
    /// Creating an instance of the Instrument class
    /// </summary>
    /// <param name="name">Instrument's name. Can also be a key for localization.</param>
    /// <param name="execute">The entry point to start the <see cref="Instrument"/>.</param>
    public Instrument(string? name, Action execute)
    {
        Name = name;
        Execute = execute;
    }
}