using System.Diagnostics;

namespace BLang.Error
{
    /// <summary>
    /// Base class which all error types can inherit from.
    /// can be logged and recovered from using various strategies.
    /// </summary>
    public abstract class ParseError
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public ParseError(ParserContext context) 
        {
            Context = new ParserContext(context);
        }

        /// <summary>
        /// The context the parser was in when this error was produced.
        /// </summary>
        protected ParserContext Context { get; private set; }

        /// <summary>
        /// The error level, indicating how the compiler should proceed.
        /// </summary>
        public abstract eErrorLevel Level { get; }

        /// <summary>
        /// Different error types should override this and output their own message.
        /// </summary>
        protected abstract string Message { get; }

        /// <summary>
        /// The error code enumeration token.
        /// We are allowing this to be overridden since its possible
        /// an error can become a different error with a different context.
        /// </summary>
        public abstract eParseError ErrorType { get; }

        /// <summary>
        /// Base implementation of recover from error. Calls the child delegate if it makes sense to do so.
        /// </summary>
        public bool RecoverFromError()
        {
            // It only make sense to recover if we have something to recover from.
            // Warnings and critical messages do not need to recover.
            if (Level == eErrorLevel.Error)
            {
                return ChildRecoverFromError();
            }

            return true;
        }

        /// <summary>
        /// Attempts to recover from the error and set the code into a state that it can continue to run.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ChildRecoverFromError();

        /// <summary>
        /// Prints the error to the output.
        /// </summary>
        public void PrintErrorMessage()
        {
            // TODO: file name.
            Console.WriteLine($"{Level}[{ErrorType.ErrorCode()}] {ErrorType.Name()} on Ln: {Context.CurrentLine} Ch: {Context.CurrentChar}");
            Console.WriteLine(Message);
        }
    }

    /// <summary>
    /// Attribute storing information about an error.
    /// </summary>
    public class ParseErrorAttribute : Attribute
    {
        public string ErrorName { get; }
        public eParseErrorSeries ErrorSeries { get; }
        public string Description { get; }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        public ParseErrorAttribute(string errName, eParseErrorSeries errorSeries, string description)
        {
            ErrorName = errName;
            ErrorSeries = errorSeries;
            Description = description;
        }
    }

    public class ParseErrorSeriesAttribute : Attribute
    {
        public string SeriesPrefix { get; }
        public string Description { get; }

        public ParseErrorSeriesAttribute(string seriesPrefix, string description)
        {
            SeriesPrefix = seriesPrefix;
            Description = description;
        }
    }

    public static class ParseErrorEnumUtils
    {
        static ParseErrorEnumUtils()
        {
            var currentErrCodes = new Dictionary<string, int>();

            foreach (var errorSeries in Enum.GetValues<eParseErrorSeries>())
            {
                string prefix = errorSeries.Prefix();

                if (currentErrCodes.ContainsKey(prefix))
                {
                    Trace.Assert(false, $"Prefix {prefix} has already been used by another series.");
                }

                currentErrCodes[errorSeries.Prefix()] = 1;
            }

            // Set up error code unique codes and verify that they are indeed unique.
            foreach(var error in Enum.GetValues<eParseError>())
            {
                int nextCode = currentErrCodes[error.Series().Prefix()]++;
                mErrorCodes[error] = $"{error.Series().Prefix()}{nextCode:D3}";
            }
        }

        public static string Name(this eParseError token)
        {
            return mCachedAttributes.GetAttribute(token).ErrorName;
        }

        public static eParseErrorSeries Series(this eParseError token)
        {
            return mCachedAttributes.GetAttribute(token).ErrorSeries;
        }

        public static string Description(this eParseError token)
        {
            return mCachedAttributes.GetAttribute(token).Description;
        }

        public static string ErrorCode(this eParseError token)
        {
            return mErrorCodes[token];
        }

        public static string Prefix(this eParseErrorSeries token)
        {
            return mErrorSeriesCachedAttributes.GetAttribute(token).SeriesPrefix;
        }

        public static string Description(this eParseErrorSeries token)
        {
            return mErrorSeriesCachedAttributes.GetAttribute(token).Description;
        }

        private static AttributeCacheHelper<ParseErrorAttribute, eParseError> mCachedAttributes = new();
        private static AttributeCacheHelper<ParseErrorSeriesAttribute, eParseErrorSeries> 
            mErrorSeriesCachedAttributes = new();

        private static Dictionary<eParseError, string> mErrorCodes = new();
    }
}
