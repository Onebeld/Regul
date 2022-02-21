namespace Regul.Base.Generators
{
    public class RegulSeparator : IRegulObject
    {
        public string Id { get; }

        public RegulSeparator(string id)
        {
            Id = id;
        }
    }
}