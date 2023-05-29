using BLang.Error;
using BLang.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BLang.Tests
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void TestEmptyFile()
        {
            string emptyFile = "";
            var tokenizer = StringToTokenizer(emptyFile);

            while (tokenizer.NextToken())
            {
                // It should never get here since we have an empty file.
                Assert.IsTrue(false);
            }

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod] 
        public void TestInvalidTokens()
        {
            string testFile = """
                "\"test\""          // Valid quotes within quotes

                // Invalid strings
                'adf'               // Too many characters in char literal
                '\7'                // Invalid escape sequence

                // Invalid integers
                0xx67              
                0t32
                006

                // Invalid floats
                .12                 // Invalid float
                12.                 // Invalid float
                10e-.12
                10.e12
                10.12e.12
                .12e12
                """;

            var tokenizer = StringToTokenizer(testFile);
            ParserContext context = tokenizer.ParserContext;

            tokenizer.NextToken();
        }

        [TestMethod]
        public void TestOneCharacterSyntaxTokens()
        {
            string singleCharacters = """
                , ~ * ( ) { } [ ] & | ^ ! > < : ; = - + / %
                """;

            var tokens = Enum.GetValues<eOneCharSyntaxToken>()
                .Select(token => token.Code()).ToList();

            var tokenizer = StringToTokenizer(singleCharacters);
            ParserContext context = tokenizer.ParserContext;

            int i = 0;
            while (tokenizer.NextToken())
            {
                Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);

                // Each token should be separated by a single space.
                Assert.AreEqual(context.Token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            // Test with a single token in the file, and a new line followed by empty text.
            string fileEndEdgeCase = """>""";
            tokenizer = StringToTokenizer(fileEndEdgeCase);
            context = tokenizer.ParserContext;

            while (tokenizer.NextToken())
            {
                Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
                Assert.AreEqual(context.Token.Code, eOneCharSyntaxToken.Gt.Code());
            }

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod]
        public void TestTwoCharacterSyntaxTokens()
        {
            string file = """
                >= <= == != && || << >> =>
                """;

            var tokens = Enum.GetValues<eTwoCharSyntaxToken>()
                .Select(token => token.Code()).ToList();

            var tokenizer = StringToTokenizer(file);
            ParserContext context = tokenizer.ParserContext;

            int i = 0;
            while (tokenizer.NextToken())
            {
                Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);

                // Each token should be separated by a single space.
                Assert.AreEqual(context.Token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            // Test with a single token in the file, and a new line followed by empty text.
            string fileEndEdgeCase = """>>""";
            tokenizer = StringToTokenizer(fileEndEdgeCase);
            context = tokenizer.ParserContext;

            while (tokenizer.NextToken())
            {
                Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
                Assert.AreEqual(context.Token.Code, eTwoCharSyntaxToken.LogicalShiftRight.Code());
            }

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod]
        public void TestNumbers()
        {
            // Test basic integers.
            string file = """
                // Basic integers.
                1 2 3 4 5 12345 -12345 - -0x32

                // Special integers.
                -0b10 0b10
                -0o1 0o1

                // Floats.
                10.2 -10.00001
                10e2 -10e2
                10.32e2 -10.32e20
                10e-2 -10.2e-2
                """;

            var tokenizer = StringToTokenizer(file);
            ParserContext context = tokenizer.ParserContext;

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "1");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "2");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "3");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "4");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "5");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "12345");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "-12345");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(context.Token.Lexeme, "-");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "-0x32");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "-0b10");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "0b10");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "-0o1");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "0o1");

            // Floats
            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "10.2");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "-10.00001");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "10e2");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "-10e2");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "10.32e2");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "-10.32e20");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "10e-2");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(context.Token.Lexeme, "-10.2e-2");

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod]
        public void TestStrings()
        {
            string file = """
                "test string"
                "this is yet another test string // test" "another" //test string
                "\n \\n \t"

                "\v \f \r\n"
                """;

            var tokenizer = StringToTokenizer(file);
            ParserContext context = tokenizer.ParserContext;

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.String);
            Assert.AreEqual(context.Token.Lexeme, "test string");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.String);
            Assert.AreEqual(context.Token.Lexeme, "this is yet another test string // test");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.String);
            Assert.AreEqual(context.Token.Lexeme, "another");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.String);
            Assert.AreEqual(context.Token.Lexeme, "\n \\n \t");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.String);
            Assert.AreEqual(context.Token.Lexeme, "\v \f \r\n");

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod]
        public void TestChars()
        {
            string file = """
                'a' 'b' 'n'
                '\0'
                '\n'
                """;

            var tokenizer = StringToTokenizer(file);
            ParserContext context = tokenizer.ParserContext;

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Char);
            Assert.AreEqual(context.Token.Lexeme, "a");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Char);
            Assert.AreEqual(context.Token.Lexeme, "b");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Char);
            Assert.AreEqual(context.Token.Lexeme, "n");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Char);
            Assert.AreEqual(context.Token.Lexeme, "\0");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Char);
            Assert.AreEqual(context.Token.Lexeme, "\n");

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod]
        public void TestReserveWords()
        {
            string file = """
                export import var true false const return
                sizeof entrypt fn break continue if else for while
                """;

            var tokens = Enum.GetValues<eReserveWord>()
                .Select(token => token.Code()).ToList();

            var tokenizer = StringToTokenizer(file);
            ParserContext context = tokenizer.ParserContext;

            int i = 0;
            while (tokenizer.NextToken())
            {
                Assert.AreEqual(context.Token.Type, eTokenType.ReserveWord);

                // Each token should be separated by a single space.
                Assert.AreEqual(context.Token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            file = """
                i8 i16 i32 i64 bool f32 f64
                """;

            tokens = Enum.GetValues<ePrimitiveType>()
                .Select(token => token.Code()).ToList();

            tokenizer = StringToTokenizer(file);
            context = tokenizer.ParserContext;

            i = 0;
            while (tokenizer.NextToken())
            {
                Assert.AreEqual(context.Token.Type, eTokenType.Type);

                // Each token should be separated by a single space.
                Assert.AreEqual(context.Token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        [TestMethod]
        public void TestIdentifiers()
        {
            string file = """
                identifier == _value_idt = "string";
                const v = 32;
                """;

            var tokens = Enum.GetValues<eReserveWord>()
                .Select(token => token.Code()).ToList();

            var tokenizer = StringToTokenizer(file);
            ParserContext context = tokenizer.ParserContext;

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Identifier);
            Assert.AreEqual(context.Token.Lexeme, "identifier");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(context.Token.Lexeme, "==");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Identifier);
            Assert.AreEqual(context.Token.Lexeme, "_value_idt");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(context.Token.Lexeme, "=");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.String);
            Assert.AreEqual(context.Token.Lexeme, "string");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(context.Token.Lexeme, ";");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.ReserveWord);
            Assert.AreEqual(context.Token.Lexeme, "const");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Identifier);
            Assert.AreEqual(context.Token.Lexeme, "v");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(context.Token.Lexeme, "=");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.Integer);
            Assert.AreEqual(context.Token.Lexeme, "32");

            tokenizer.NextToken();
            Assert.AreEqual(context.Token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(context.Token.Lexeme, ";");

            Assert.AreEqual(tokenizer.NextToken(), false);
            Assert.AreEqual(tokenizer.ErrorLogger.ErrorCount, 0);
        }

        #region Utilities 

        private Tokenizer StringToTokenizer(string input)
        {
            ParserContext context = new ParserContext();
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new(byteArray);

            var reader = new StreamReader(stream);
            var tokenizer = new Tokenizer(context);
            tokenizer.SetStream(reader);

            return tokenizer;
        }

        #endregion
    }
}
