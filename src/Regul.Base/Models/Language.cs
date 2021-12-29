namespace Regul.Base.Models
{
	public class Language
	{
		public string Name { get; private set; }
		public string Key { get; private set; }
		public string Resource { get; private set; }

		public Language(string name, string key, string resource)
		{
			Name = name;
			Key = key;
			Resource = resource;
		}
	}
}
