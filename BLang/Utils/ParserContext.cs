namespace BLang
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
}
