using Avalonia.Media;

namespace Regul.ModuleSystem
{
	public interface IModule
	{
		/// <summary>
		/// The image that will be shown in the program settings
		/// </summary>
		IImage Icon { get; set; }
		/// <summary>
		/// Module name
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Name of the creator who created this module
		/// </summary>
		string Creator { get; }
		/// <summary>
		/// Module Description
		/// </summary>
		string Description { get; }
		/// <summary>
		/// Module version
		/// </summary>
		string Version { get; }

		/// <summary>
		/// If the module is incorrectly initialized, it can be reinitialized.
		/// </summary>
		bool CorrectlyInitialized { get; set; }
		/// <summary>
		/// Indicates if the module needs to be updated
		/// </summary>
		bool ThereIsAnUpdate { get; set; }

		/// <summary>
		/// Starting the module
		/// </summary>
		void Execute();
		/// <summary>
		/// Certain actions at the end of the program, such as saving settings
		/// </summary>
		void Release();

		/// <summary>
		/// If your module supports localization, change the module language using this method.
		/// </summary>
		void ChangeLanguage(string language);

		/// <summary>
		/// Certain actions after clicking the "Go to update" button, for example, opening a website
		/// </summary>
		void GoToUpdate();
	}
}