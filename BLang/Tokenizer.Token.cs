namespace BLang
{
    public enum eTokenType
    {
        ReserveWord,
        Type,
        Identifier,
        String,
        FloatingPoint,
        Integer,
        SyntaxToken,
        Char
    }

    public partial class Tokenizer
    {
        public class Token
        {
            public Token()
            {
            }

            /// <summary>
            /// Sets the data in use for this token.
            /// </summary>
            /// <param name="lexeme"></param>
            /// <param name="type"></param>
            public void SetTokenData(string lexeme, eTokenType type, int code = 0)
            {
                Lexeme = lexeme;
                Type = type;

                Code = code;
            }

            /// <summary>
            /// The text used for this token.
            /// </summary>
            public string Lexeme { get; set; }

            /// <summary>
            /// The token type.
            /// </summary>
            public eTokenType Type { get; set; }

            public int line { get; set; }
            public int Col { get; set; }

            /// <summary>
            /// Stores more specific information about the token.
            /// A number for floats and integers, or a code for reserves.
            /// </summary>
            public long Code { get; set; }

            /// <summary>
            /// Prints the token to the console.
            /// </summary>
            public void PrintToken()
            {
                Console.WriteLine($"\"{Lexeme}\"   [{Code}]: {line}, {Col} ({Type})");
            }
        }
    }
}
