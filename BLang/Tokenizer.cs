using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace BLang
{
    public partial class Tokenizer
    {
        /// <summary>
        /// Standard constructor. Initialize the input stream later.
        /// </summary>
        public Tokenizer()
        {
            mCurrentBuffer = new char[BUFFER_SIZE];
            mChar = EOF;

            mCurrentLine = 1;
            mCurrentCol = 0;
        }

        /// <summary>
        /// Finds the next syntax token and sets data accordingly.
        /// </summary>
        /// <param name="nextToken"></param>
        /// <returns></returns>
        public bool NextToken(Token nextToken)
        {
            mCurrentToken = nextToken;
            TrimWhiteSpace();

            // Set the token starting position.
            mCurrentToken.line = mCurrentLine;
            mCurrentToken.Col = mCurrentCol;

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
            else if (ReadCharKeyword())
            {
            }
            else if (mChar == '\'')
            {
                char token;

                NextCharacter();

                if (mChar == '\\')
                {
                    NextCharacter();
                    if (!ReadEscapedChar(mChar, out token))
                    {
                        Debugger.Break();
                        // AddError();
                    }

                    NextCharacter();
                }
                else
                {
                    token = mChar;
                }

                NextCharacter();

                if (mChar != '\'')
                {
                    Debugger.Break();
                    //AddError("Expected '");
                }

                NextCharacter();

                mCurrentToken.SetTokenData(string.Empty + token, eTokenType.Char);
            }
            else
            {
                // An error occurs in this case. 
                // Move till we get to the next word, and log the error.
                // TODO!
                return false;
            }

            return true;
        }

        #region Parsing utilities

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

            int reserveTableCode = mReserveTable.GetReserveCode(mCurrentToken.Lexeme);

            if (reserveTableCode >= 0)
            {
                mCurrentToken.Type = eTokenType.ReserveWord;
                mCurrentToken.Code = reserveTableCode;
            }
            else
            {
                int typeTableCode = mPrimitiveTypeTable.GetTypeCode(mCurrentToken.Lexeme);

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
            if (next == '\'')
            {
                character = '\'';
                return true;
            }
            if (next == '"')
            {
                character = '"';
                return true;
            }
            else if (next == '\\')
            {
                character = '\\';
                return true;
            }
            else if (next == 'n')
            {
                character = '\n';
                return true;
            }
            else if (next == 'r')
            {
                character = '\r';
                return true;
            }
            else if (next == 't')
            {
                character = '\t';
                return true;
            }
            else if (next == 'b')
            {
                character = '\b';
                return true;
            }
            else if (next == 'f')
            {
                character = '\f';
                return true;
            }
            else if (next == 'a')
            {
                character = '\a';
                return true;
            }
            else if (next == 'v')
            {
                character = '\v';
                return true;
            }
            else if (next == '\0')
            {
                character = '\0';
                return true;
            }

            character = EOF;
            return false;
        }

        /// <summary>
        /// Reads a string and stores it in the token.
        /// </summary>
        /// <returns></returns>
        public void ReadString()
        {
            NextCharacter();

            StringBuilder loadedString = new StringBuilder();

            while (mChar != '"')
            {
                if (mChar == '\\')
                {
                    NextCharacter();
                    if (!ReadEscapedChar(mChar, out var nextChar))
                    {
                        Debugger.Break();
                        // TODO: ERROR CASE
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

            mCurrentToken.SetTokenData(loadedString.ToString(), eTokenType.String);

            NextCharacter();
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
                    while (mChar == '0' || mChar == '1')
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                    }

                    mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
                    return;
                }
                else if (mChar == 'x')
                {
                    lexeme.Append(mChar);
                    NextCharacter();

                    while ((mChar >= '0' && mChar <= '9') ||
                           (mChar >= 'A' && mChar <= 'F') ||
                            mChar >= 'a' && mChar <= 'f')
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                    }

                    mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
                    return;
                }
                else if (mChar == 'o')
                {
                    lexeme.Append(mChar);
                    NextCharacter();

                    // Load hex.
                    while ((mChar >= '0' && mChar <= '7'))
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                    }

                    mCurrentToken.SetTokenData(lexeme.ToString(), tokenType);
                    return;
                }
            }

            while (char.IsNumber(mChar))
            {
                lexeme.Append(mChar);
                NextCharacter();
            }

            // Handle a decimal.
            if (mChar == '.')
            {
                lexeme.Append(mChar);
                NextCharacter();

                while (char.IsNumber(mChar))
                {
                    lexeme.Append(mChar);
                    NextCharacter();
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

                if (char.IsNumber(mChar))
                {
                    while (char.IsNumber(mChar))
                    {
                        lexeme.Append(mChar);
                        NextCharacter();
                    }
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
            if (mIndexInBuffer < mCurrentBuffer.Length)
            {
                mChar = mCurrentBuffer[mIndexInBuffer++];
                AdvanceLineAndCol();
                return true;
            }

            if (!LoadNextBuffer())
            {
                return false;
            }

            mIndexInBuffer = 0;
            mChar = mCurrentBuffer[mIndexInBuffer++];
            AdvanceLineAndCol();

            return true;
        }

        private void AdvanceLineAndCol()
        {
            // Adjust line and column counters as needed.
            if (mChar == '\n')
            {
                mCurrentLine++;
                mCurrentCol = 0;
            }
            else
            {
                mCurrentCol++;
            }

        }

        /// <summary>
        /// Finds the next chacter without actually loading it.
        /// Returns false if there is no next character in the file.
        /// </summary>
        /// <returns></returns>
        private char PeekCharacter()
        {
            if (mIndexInBuffer < mCurrentBuffer.Length)
            {
                return mCurrentBuffer[mIndexInBuffer];
            }

            if (!mStreamReader.EndOfStream)
            {
                // Load a single character from the file stream.
                Span<char> peekedCharacter = stackalloc char[1];
                mStreamReader.ReadBlock(peekedCharacter);

                // Reset the stream to make sure we aren't consuming.
                mStreamReader.BaseStream.Position = mStreamReader.BaseStream.Position - 1;

                return peekedCharacter[0];
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
                mStreamReader.ReadBlock(mCurrentBuffer, 0, BUFFER_SIZE);
            }
            catch
            {
                Console.WriteLine("Could not read the next buffer from the file!");
            }

            return true;
        }

        #endregion

        const char EOF = '\0';
        private const int BUFFER_SIZE = 1000;

        private StreamReader mStreamReader;

        private char[] mCurrentBuffer = new char[BUFFER_SIZE];
        private int mIndexInBuffer;
        private char mChar;

        private Token mCurrentToken;

        /// <summary>
        /// Keep track of the current line we are on.
        /// </summary>
        private int mCurrentLine = 0;

        /// <summary>
        /// Keep track of the current character we are on on that line.
        /// </summary>
        private int mCurrentCol = 0;

        private IReadOnlyList<eOneCharSyntaxToken> mAllOneCharTokens =
            new List<eOneCharSyntaxToken>(Enum.GetValues<eOneCharSyntaxToken>());

        private IReadOnlyList<eTwoCharSyntaxToken> mAllTwoCharTokens =
            new List<eTwoCharSyntaxToken>(Enum.GetValues<eTwoCharSyntaxToken>());

        // TODO: these should eventually move.
        private ReserveTable mReserveTable = new();
        private TypeTable mPrimitiveTypeTable = new();
    }
}
