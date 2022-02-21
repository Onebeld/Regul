namespace Regul.ModuleSystem.Models
{
	public class Language
	{
		public string Name { get; }
		public string Key { get; }
		public string ExoticKey { get; }

		public Language(string name, string key, string exoticKey = null)
		{
			Name = name;
			Key = key;
			ExoticKey = exoticKey;
		}
	}
}
