using Avalonia.Media;
using Avalonia.Styling;
using Regul.ModuleSystem.Models;

namespace Regul.ModuleSystem;

public interface IModule
{
	/// <summary>
	///     The image that will be shown in the program settings
	/// </summary>
	IImage Icon { get; set; }

	/// <summary>
	///     Module name
	/// </summary>
	string Name { get; }

	/// <summary>
	///     Name of the creator who created this module
	/// </summary>
	string Creator { get; }

	/// <summary>
	///     Module Description
	/// </summary>
	string Description { get; }

	/// <summary>
	///     Module version
	/// </summary>
	string Version { get; }

	/// <summary>
	///     If the module is incorrectly initialized, it can be reinitialized.
	/// </summary>
	bool CorrectlyInitialized { get; set; }

	/// <summary>
	///     Indicates if the module needs to be updated
	/// </summary>
	bool ThereIsAnUpdate { get; set; }

	Language[] Languages { get; }

	IStyle LanguageStyle { get; set; }
	string PathToLocalization { get; }
	string? LinkForCheckUpdates { get; }

	/// <summary>
	///     Starting the module
	/// </summary>
	void Execute();
}