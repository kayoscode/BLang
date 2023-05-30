using BLang.Error;
using BLang.Syntax;
using BLang.Utils;
using System.Text;

namespace BLang
{
    public partial class Tokenizer
    {
        /// <summary>
        /// Standard constructor. Initialize the input stream later.
        /// </summary>
        public Tokenizer(ParserContext context)
        {
            mParserContext = context;
            mCurrentToken = mParserContext.Token;

            mCurrentBuffer = new char[BUFFER_SIZE];
            mChar = EOF;
        }

        /// <summary>
        /// Finds the next syntax token and sets data accordingly.
        /// </summary>
        /// <param name="nextToken"></param>
        /// <returns></returns>
        public bool NextToken()
        {
            TrimWhiteSpace();

            // Set the token starting position.
            mCurrentToken.SetTokenData(string.Empty, eTokenType.InvalidToken);
            mCurrentToken.Line = mParserContext.CurrentLine;
            mCurrentToken.Char = mParserContext.CurrentChar;

            // Read the keyword, identifier, constant, or valid symbol
            if (char.IsLetter(mChar) || mChar == '_')
            {
                ReadIdentifierOrKeyword();
            }
            else if (char.IsNumber(mChar) || (mChar == '-' && char.IsNumber(PeekCharacter())))
            {
                ReadNumber();
            }
            else if (mChar == '"')
            {
                ReadString();
            }
            else if (mChar == '\'')
            {
                ReadCharacterLiteral();
            }
            else if (ReadCharKeyword())
            {
            }
            else
            {
                if (mChar != EOF)
                {
                    ErrorLogger.LogError(new UnexpectedCharacter(mParserContext));
                    NextCharacter();
                    return true;
                }

                return false;
            }

            return true;
        }

        #region Parsing utilities

        private void ReadCharacterLiteral()
        {
            NextCharacter();
            if (ReadStringHelper('\'', eTokenType.Char))
            {
                // Must have only one character to be a valid char.
                if (mCurrentToken.Lexeme.Length > 1)
                {
                    ErrorLogger.LogError(new TooManyCharactersInCharLiteral(mParserContext));
                    mCurrentToken.Type = eTokenType.InvalidToken;
                }
                else if(mCurrentToken.Lexeme.Length == 0)
                {
                    ErrorLogger.LogError(new EmptyCharLiteral(mParserContext));
                    mCurrentToken.Type = eTokenType.InvalidToken;
                }
            }
        }

        /// <summary>
        /// Reads from the file either an identifier or keyword based on the reserve table.
        /// </summary>
        private void ReadIdentifierOrKeyword()
        {
            mCurrentToken.Lexeme = string.Empty + mChar;
            NextCharacter();

            // Load the rest of the lexeme from the file.
            while (mChar == '_' ||
                   char.IsLetter(mChar) ||
                   char.IsDigit(mChar))
            {
                mCurrentToken.Lexeme += mChar;

                if (!NextCharacter())
                {
                    break;
                }
            }

            int reserveTableCode = mParserContext.ReserveTable.GetReserveCode(mCurrentToken.Lexeme);

            if (reserveTableCode >= 0)
            {
                mCurrentToken.Type = eTokenType.ReserveWord;
                mCurrentToken.Code = reserveTableCode;
            }
            else
            {
                int typeTableCode = mParserContext.PrimitiveTypeTable.GetTypeCode(mCurrentToken.Lexeme);

                if (typeTableCode >= 0)
                {
                    mCurrentToken.Type = eTokenType.Type;
                    mCurrentToken.Code = typeTableCode;
                }
                else
                {
                    mCurrentToken.Type = eTokenType.Identifier;
                }
            }
        }

        /// <summary>
        /// Reads an escape character and returns the result.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        private bool ReadEscapedChar(char next, out char character)
        {
            if (EscapeCharacterUtils.TryGetEscapeCharacter(next, out character))
            {
                return true;
            }

            character = EOF;
            return false;
        }

        /// <summary>
        /// Reads a string and stores it in the token.
        /// </summary>
        /// <returns></returns>
        private void ReadString()
        {
            NextCharacter();
            ReadStringHelper('"', eTokenType.String);
        }

        /// <summary>
        /// Returns false if there was an error found.
        /// </summary>
        /// <param name="endChar"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool ReadStringHelper(char endChar, eTokenType type)
        {
            StringBuilder loadedString = new StringBuilder();

            bool error = false;

            while (mChar != endChar) 
            {
                // If there's a new line, we have to catch it. A string cannot go into the next line.
                if (mChar == EOF || mChar == '\n')
                {
                    ErrorLogger.LogError(new NewLineInLiteral(mParserContext, type));
                    error = true;
                    break;
                }
                else if (mChar == '\\')
                {
                    NextCharacter();
                    if (!ReadEscapedChar(mChar, out var nextChar))
                    {
                        ErrorLogger.LogError(new UnrecognizedEscapeSequence(mParserContext));
                        error = true;
                    }

                    loadedString.Append(nextChar);
                    NextCharacter();
                }
                else
                {
                    loadedString.Append(mChar);
                    NextCharacter();
                }
            }

            if (!error)
            {
                mCurrentToken.SetTokenData(loadedString.ToString(), type);
            }

            NextCharacter();
            return !error;
        }

        /// <summary>
        /// Reads a number from the stream.
        /// </summary>
        private void ReadNumber()
        {
            StringBuilder lexeme = new StringBuilder(mChar);

            if (mChar == '-')
            {
                lexeme.Append(mChar);
                NextCharacter();
            }

            eTokenType tokenType = eTokenType.Integer;
            int digitCount;

            // Load a hex number if that's what this is
            if (mChar == '0')
            {
                lexeme.Append(mChar);
                NextCharacter();

                if (mChar == 'b')
                {
                    lexeme.Append(mChar);
                    NextCharacter();

                    // Load binary.
                    digitCount = 0;
                    while (mChar == '0' || mChar == '1')
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                        digitCount++;
                    }

                    if (digitCount == 0)
                    {
                        ErrorLogger.LogError(new InvalidNumberLiteral(mParserContext));
                        return;
                    }

                    mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
                    return;
                }
                else if (mChar == 'x')
                {
                    lexeme.Append(mChar);
                    NextCharacter();

                    digitCount = 0;
                    while ((mChar >= '0' && mChar <= '9') ||
                           (mChar >= 'A' && mChar <= 'F') ||
                            mChar >= 'a' && mChar <= 'f')
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                        digitCount++;
                    }

                    if (digitCount == 0)
                    {
                        ErrorLogger.LogError(new InvalidNumberLiteral(mParserContext));
                        return;
                    }

                    mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
                    return;
                }
                else if (mChar == 'o')
                {
                    lexeme.Append(mChar);
                    NextCharacter();

                    // Load hex.
                    digitCount = 0;
                    while ((mChar >= '0' && mChar <= '7'))
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                        digitCount++;
                    }

                    if (digitCount == 0)
                    {
                        ErrorLogger.LogError(new InvalidNumberLiteral(mParserContext));
                        return;
                    }

                    mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
                    return;
                }
                else
                {
                    digitCount = 1;
                }
            }
            else
            {
                digitCount = 0;
            }

            while (char.IsNumber(mChar))
            {
                lexeme.Append(mChar);
                NextCharacter();
                digitCount++;
            }

            if (digitCount == 0)
            {
                ErrorLogger.LogError(new InvalidNumberLiteral(mParserContext));
                return;
            }

            // Handle a decimal.
            if (mChar == '.')
            {
                lexeme.Append(mChar);
                NextCharacter();

                digitCount = 0;
                while (char.IsNumber(mChar))
                {
                    lexeme.Append(mChar);
                    NextCharacter();
                    digitCount++;
                }

                if (digitCount == 0)
                {
                    ErrorLogger.LogError(new InvalidRealLiteral(mParserContext));
                    return;
                }

                tokenType = eTokenType.FloatingPoint;
            }

            // When its not a number anymore, we must see what it is!
            if (mChar == 'e' || mChar == 'E')
            {
                lexeme.Append(mChar);
                NextCharacter();

                if (mChar == '-')
                {
                    lexeme.Append(mChar);
                    NextCharacter();
                }

                digitCount = 0;
                while (char.IsNumber(mChar))
                {
                    lexeme.Append(mChar);
                    NextCharacter();
                    digitCount++;
                }

                if (digitCount == 0)
                {
                    ErrorLogger.LogError(new InvalidRealLiteral(mParserContext));
                }

                tokenType = eTokenType.FloatingPoint;
            }

            mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
        }

        /// <summary>
        /// Reads a character keyword and returns the 
        /// </summary>
        /// <returns></returns>
        public bool ReadCharKeyword()
        {
            // Greedily consume one or two characters to determine if we have a syntax token.
            var c1 = mChar;
            var c2 = PeekCharacter();

            if (c2 != EOF && !char.IsWhiteSpace(c2))
            {
                foreach (var twoCharSyntaxToken in mAllTwoCharTokens)
                {
                    if (c1 == twoCharSyntaxToken.Char1() &&
                        c2 == twoCharSyntaxToken.Char2())
                    {
                        // Move to the peeked character.
                        NextCharacter();
                        NextCharacter();

                        mCurrentToken.SetTokenData(c1 + string.Empty + c2,
                            eTokenType.SyntaxToken, twoCharSyntaxToken.Code());

                        return true;
                    }
                }
            }

            // See if we match a single char token.
            if (c1 != EOF && !char.IsWhiteSpace(c1))
            {
                foreach (var oneCharSyntaxToken in mAllOneCharTokens)
                {
                    if (c1 == oneCharSyntaxToken.Char())
                    {
                        NextCharacter();

                        // We found a one char match.
                        mCurrentToken.SetTokenData(string.Empty + c1,
                            eTokenType.SyntaxToken, oneCharSyntaxToken.Code());

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the whitespace from the file.
        /// </summary>
        private bool TrimWhiteSpace()
        {
            while (char.IsWhiteSpace(mChar))
            {
                NextCharacter();
            }

            // Check if we have a comment, and if we do, take it out.
            if (mChar == '/')
            {
                // We have the start of a comment.
                char nextCharacter = PeekCharacter();

                if (nextCharacter == '/')
                {
                    // Load till the end of the line since we are in a comment.
                    while (true)
                    {
                        if (mChar == '\n' || mChar == EOF)
                        {
                            break;
                        }

                        NextCharacter();
                    }

                    return TrimWhiteSpace();
                }
            }

            return true;
        }

        #endregion

        #region File loading utils

        /// <summary>
        /// Loads the file and prepares to tokenize.
        /// </summary>
        /// <param name="fileName"></param>
        public void SetStream(StreamReader stream)
        {
            this.mStreamReader = stream;

            LoadNextBuffer();
            NextCharacter();
        }

        /// <summary>
        /// Returns the next character in the file.
        /// </summary>
        /// <returns></returns>
        private bool NextCharacter()
        {
            if (mIndexInBuffer < mCurrentBufferLength)
            {
                mChar = mCurrentBuffer[mIndexInBuffer++];
                mParserContext.AdvanceLineAndChar(mChar);
                return true;
            }

            if (!LoadNextBuffer())
            {
                mChar = EOF;
                return false;
            }

            mIndexInBuffer = 0;
            mChar = mCurrentBuffer[mIndexInBuffer++];
            mParserContext.AdvanceLineAndChar(mChar);

            return true;
        }

        /// <summary>
        /// Finds the next chacter without actually loading it.
        /// Returns false if there is no next character in the file.
        /// </summary>
        /// <returns></returns>
        private char PeekCharacter()
        {
            if (mIndexInBuffer < mCurrentBufferLength)
            {
                return mCurrentBuffer[mIndexInBuffer];
            }

            if (!mStreamReader.EndOfStream)
            {
                return (char)mStreamReader.Peek();
            }

            return EOF;
        }

        /// <summary>
        /// Loads the next buffer, if we are the end of the file, break out and return false.
        /// </summary>
        /// <returns></returns>
        private bool LoadNextBuffer()
        {
            if (mStreamReader.EndOfStream)
            {
                return false;
            }

            try
            {
                mCurrentBufferLength = mStreamReader.ReadBlock(mCurrentBuffer, 0, BUFFER_SIZE);
            }
            catch
            {
                Console.WriteLine("Could not read the next buffer from the file!");
            }

            return true;
        }

        #endregion

        const char EOF = '\0';
        private const int BUFFER_SIZE = AppSettings.TokenizerBufferSize;

        private StreamReader mStreamReader;

        private char[] mCurrentBuffer;

        /// <summary>
        ///  The total number of valid bytes in the buffer. We should not read past this boundary.
        /// </summary>
        private int mCurrentBufferLength = 0;
        private int mIndexInBuffer;
        private char mChar;

        private Token mCurrentToken;

        public ParserContext ParserContext => mParserContext;
        private ParserContext mParserContext;

        private IReadOnlyList<eOneCharSyntaxToken> mAllOneCharTokens =
            new List<eOneCharSyntaxToken>(Enum.GetValues<eOneCharSyntaxToken>());

        private IReadOnlyList<eTwoCharSyntaxToken> mAllTwoCharTokens =
            new List<eTwoCharSyntaxToken>(Enum.GetValues<eTwoCharSyntaxToken>());

        public ErrorLogger ErrorLogger { get; private set; } = new();
    }
}
