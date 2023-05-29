namespace BLang.Error
{
    /// <summary>
    /// All the different types of errors that can occur in the system.
    /// </summary>
    public enum eParseError
    {
        #region Tokenization errors

        // Tokenization error series = 1000
        [ParseError("Unexpected character", 1000)]
        UnexpectedCharacter,

        /// <summary>
        /// When a floating point number doesn't match the expected format.
        /// </summary>
        [ParseError("Invalid Real Literal", 1001)]
        InvalidRealLiteral,

        /// <summary>
        /// A character literal is formatted incorrectly. 
        /// </summary>
        [ParseError("Invalid character literal", 1002)]
        InvalidCharacterLiteral,

        /// <summary>
        /// When a number is not formatted correctly for the compiler.
        /// </summary>
        [ParseError("Invalid number literal", 1003)]
        InvalidNumberLiteral,

        /// <summary>
        /// If there is an unrecognized escape sequence in a char or string.
        /// </summary>
        [ParseError("Unrecognized escape sequence", 1004)]
        UnrecognizedEscapeSequence

        #endregion
    }

    public enum eErrorLevel
    {
        /// <summary>
        /// Warnings do not affect compilation, but they can indicate a problem with the code.
        /// </summary>
        Warning,

        /// <summary>
        /// An error occurs when the parser can no longer form a binary from the input text, 
        /// At the error level, the parser can continue to run and log more errors and warnings.
        /// 1. Undefined identifiers
        /// 2. Unexpected token
        /// </summary>
        Error,

        /// <summary>
        /// A critical error occurs when an error cannot be recovered from.
        /// Examples of this include:
        /// 1. Invalid token
        /// 2. Some syntax errors
        /// </summary>
        CriticalError
    }

    /// <summary>
    /// Class storing relevant information about the parser at any moment in time.
    /// </summary>
    public class ParserContext
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public ParserContext()
        {
            // Copy the token over to a new token.
            Token = new Tokenizer.Token();
        }

        // Symbol table
        // Parser stack

        // Scope context { namespace, class-ish thing, function }
        public Tokenizer.Token Token { get; private set; }

        public ReserveTable ReserveTable { get; } = new();
        public TypeTable PrimitiveTypeTable { get; } = new();

        public void AdvanceLineAndChar(char mChar)
        {
            // Adjust line and column counters as needed.
            if (mChar == '\n')
            {
                CurrentLine++;
                CurrentChar = 0;
            }
            else
            {
                CurrentChar++;
            }
        }

        public int CurrentLine { get; private set; } = 1;
        public int CurrentChar { get; private set; } = 0;

        // Compiler flags { warn all, warn=error, etc }
    }

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
            Context = context;
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
        public abstract eParseError ErrorCode { get; }

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
            Console.WriteLine($"{Level} {ErrorCode.ErrorName()} on Ln: {Context.Token.Line} Ch: {Context.Token.Char}");
            Console.WriteLine(Message);
        }
    }

    /// <summary>
    /// Attribute storing information about an error.
    /// </summary>
    public class ParseErrorAttribute : Attribute
    {
        public string ErrorName { get; private set; }
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        public ParseErrorAttribute(string errName, int code)
        {
            ErrorName = errName;
            ErrorCode = code;
        }
    }

    public static class ParseErrorEnumUtils
    {
        public static string ErrorName(this eParseError token)
        {
            return mCachedAttributes.GetAttribute(token).ErrorName;
        }

        public static int ErrorCode(this eParseError token)
        {
            return mCachedAttributes.GetAttribute(token).ErrorCode;
        }

        private static AttributeCacheHelper<ParseErrorAttribute, eParseError> mCachedAttributes = new();
    }
}
