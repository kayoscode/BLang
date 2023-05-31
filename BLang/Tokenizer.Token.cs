namespace BLang
{
    /// <summary>
    /// The different types of tokens which can appear in the code.
    /// </summary>
    public enum eTokenType
    {
        ReserveWord,
        Type,
        Identifier,
        String,
        FloatingPoint,
        Integer,
        SyntaxToken,
        Char,
        InvalidToken
    }

    public partial class Tokenizer
    {
        /// <summary>
        /// Class to store information about a token in the system. 
        /// </summary>
        public class Token
        {
            /// <summary>
            /// Standard construtor.
            /// </summary>
            public Token()
            {
            }

            /// <summary>
            /// Copy constructor.
            /// </summary>
            /// <param name="other"></param>
            public Token(Token other)
            {
                Lexeme = other.Lexeme;
                Type = other.Type;
                Line = other.Line;
                Char = other.Char;
                Code = other.Code;
                Data = other.Data;
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

            public int Line { get; set; }
            public int Char { get; set; }

            /// <summary>
            /// Stores the token code.
            /// </summary>
            public long Code { get; set; }

            /// <summary>
            /// Stores the data of the token, aka the parsed lexeme into an integer, or char or whatever
            /// </summary>
            public long Data { get; set; }

            /// <summary>
            /// Prints the token to the console.
            /// </summary>
            public void PrintToken()
            {
                Console.WriteLine($"\"{Lexeme}\" [{Code}]: {Line}, {Char} ({Type})");
            }

            public void PrintTokenShort()
            {
                Console.WriteLine($"[{Code:D4}] \"{Lexeme}\"");
            }
        }
    }
}
