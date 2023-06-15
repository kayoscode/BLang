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
            mContext = new();
            mFirstPassTokens = new();
            Tokenizer = new(this);
        }

        // Symbol table
        // Parser stack

        // Scope context { namespace, class-ish thing, function }
        public Tokenizer.Token Token { get; private set; }
        public Tokenizer Tokenizer { get; set; }

        public static ReserveTable ReserveTable { get; } = new();
        public static TypeTable PrimitiveTypeTable { get; } = new();

        public void AdvanceLineAndChar(char mChar)
        {
            // Adjust line and column counters as needed.
            // Moves to a new line if \n is found.
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

        public void AddToken(Tokenizer.Token token, Parser.eParserContext context)
        {
            mFirstPassTokens.Add(new Tokenizer.Token(token));
        }

        /// <summary>
        /// The current context we are working within.
        /// </summary>
        public Parser.eParserContext CurrentContext => mContext.Peek();

        /// <summary>
        /// This list gets filled in by the first pass. It should ensure this follows correct syntax
        /// if no fatal errors have been found.
        /// </summary>
        private Stack<Parser.eParserContext> mContext;
        private List<Tokenizer.Token> mFirstPassTokens;

        public Stack<Parser.eParserContext> Context => mContext;
        public IReadOnlyList<Tokenizer.Token> FirstPassTokens => mFirstPassTokens;
    }
}
