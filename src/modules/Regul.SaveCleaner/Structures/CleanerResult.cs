namespace Regul.SaveCleaner.Structures
{
    public class CleanerResult
    {
        public float OldSize { get; set; }

        public float NewSize { get; set; }
        
        public string Save { get; set; }

        public CleanerResult(float oldsize, float newsize, double totalSecond, string save)
        {
            OldSize = oldsize;
            NewSize = newsize;
            TotalSecond = totalSecond;
            Save = save;
        }

        public double TotalSecond { get; set; }
    }
}