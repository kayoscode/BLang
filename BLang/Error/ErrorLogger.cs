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
            error.PrintErrorMessage();
            ErrorCount++;
        }

        public int ErrorCount { get; private set; }
    }
}
