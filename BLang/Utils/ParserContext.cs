using BLang.Error;

namespace BLang.Utils
{
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
            Token = new();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other"></param>
        public ParserContext(ParserContext other)
        {
            Token = new Tokenizer.Token(other.Token);
            CurrentLine = other.CurrentLine;
            CurrentChar = other.CurrentChar;
        }

        // Symbol table
        // Parser stack

        // Scope context { namespace, class-ish thing, function }
        public Tokenizer.Token Token { get; private set; }

        public static ReserveTable ReserveTable { get; } = new();
        public static TypeTable PrimitiveTypeTable { get; } = new();

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

        /// <summary>
        /// The error state.
        /// </summary>
        public ErrorLogger ErrorLogger { get; private set; } = new();

        public void AddToken(Tokenizer.Token token, Parser.eNonTerminal context)
        {
            mContext.Add(context);
            mFirstPassTokens.Add(new Tokenizer.Token(token));
        }

        /// <summary>
        /// This list gets filled in by the first pass. It should ensure this follows correct syntax
        /// if no fatal errors have been found.
        /// </summary>
        private List<Parser.eNonTerminal> mContext = new();
        private List<Tokenizer.Token> mFirstPassTokens = new();
    }
}
