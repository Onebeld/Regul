namespace Regul.Base.Generators
{
    public class RegulSeparator : IRegulObject
    {
        public RegulSeparator(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}