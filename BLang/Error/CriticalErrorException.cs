namespace BLang.Error
{
    /// <summary>
    /// Exception thrown when the parser encounters an error it cannot recover from.
    /// </summary>
    public class CriticalErrorException : Exception
    {
        public CriticalErrorException() : base()
        {
        }
    }
}
