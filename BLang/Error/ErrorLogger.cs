namespace BLang.Error
{
    /// <summary>
    /// Handles error logging for the system.
    /// </summary>
    public class ErrorLogger
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public ErrorLogger()
        {
        }

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        /// <param name="error"></param>
        public void LogError(ParseError error)
        {
            mErrors.Add(error);

            error.PrintErrorMessage();
            ErrorCount++;

            if (!error.RecoverFromError())
            {
                throw new CriticalErrorException();
            }
        }

        public int ErrorCount { get; private set; }

        public IReadOnlyList<ParseError> Errors => mErrors;
        private List<ParseError> mErrors { get; } = new();
    }
}
