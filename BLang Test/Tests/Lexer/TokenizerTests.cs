using BLang.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(emptyFile);

            while (tokenizer.NextToken(token))
            {
                // It should never get here since we have an empty file.
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void TestOneCharacterSyntaxTokens()
        {
            string singleCharacters = """
                , ~ * ( ) { } [ ] & | ^ ! > < : ; = - + / %
                """;

            var tokens = Enum.GetValues<eOneCharSyntaxToken>()
                .Select(token => token.Code()).ToList();

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(singleCharacters);

            int i = 0;
            while (tokenizer.NextToken(token))
            {
                Assert.AreEqual(token.Type, eTokenType.SyntaxToken);

                // Each token should be separated by a single space.
                Assert.AreEqual(token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            // Test with a single token in the file, and a new line followed by empty text.
            string fileEndEdgeCase = """>""";
            tokenizer = StringToTokenizer(fileEndEdgeCase);

            while (tokenizer.NextToken(token))
            {
                Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
                Assert.AreEqual(token.Code, eOneCharSyntaxToken.Gt.Code());
            }
        }

        [TestMethod]
        public void TestTwoCharacterSyntaxTokens()
        {
            string file = """
                >= <= == != && || << >> =>
                """;

            var tokens = Enum.GetValues<eTwoCharSyntaxToken>()
                .Select(token => token.Code()).ToList();

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(file);

            int i = 0;
            while (tokenizer.NextToken(token))
            {
                Assert.AreEqual(token.Type, eTokenType.SyntaxToken);

                // Each token should be separated by a single space.
                Assert.AreEqual(token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            // Test with a single token in the file, and a new line followed by empty text.
            string fileEndEdgeCase = """>>""";
            tokenizer = StringToTokenizer(fileEndEdgeCase);

            while (tokenizer.NextToken(token))
            {
                Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
                Assert.AreEqual(token.Code, eTwoCharSyntaxToken.LogicalShiftRight.Code());
            }
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

                // TODO: these floats should fail, but they don't yet.
                10. 10.e2
                """;

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(file);

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "1");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "3");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "4");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "5");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "12345");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "-12345");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(token.Lexeme, "-");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "-0x32");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "-0b10");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "0b10");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "-0o1");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "0o1");

            // Floats
            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "10.2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "-10.00001");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "10e2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "-10e2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "10.32e2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "-10.32e20");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "10e-2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "-10.2e-2");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "10.");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.FloatingPoint);
            Assert.AreEqual(token.Lexeme, "10.e2");

            // Note: test cases that should fail such as .2 and 2.
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

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(file);

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.String);
            Assert.AreEqual(token.Lexeme, "test string");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.String);
            Assert.AreEqual(token.Lexeme, "this is yet another test string // test");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.String);
            Assert.AreEqual(token.Lexeme, "another");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.String);
            Assert.AreEqual(token.Lexeme, "\n \\n \t");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.String);
            Assert.AreEqual(token.Lexeme, "\v \f \r\n");
        }

        [TestMethod]
        public void TestChars()
        {
            string file = """
                'a' 'b' 'n'
                '\0'
                '\n'
                """;

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(file);

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Char);
            Assert.AreEqual(token.Lexeme, "a");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Char);
            Assert.AreEqual(token.Lexeme, "b");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Char);
            Assert.AreEqual(token.Lexeme, "n");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Char);
            Assert.AreEqual(token.Lexeme, "\0");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Char);
            Assert.AreEqual(token.Lexeme, "\n");
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

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(file);

            int i = 0;
            while (tokenizer.NextToken(token))
            {
                Assert.AreEqual(token.Type, eTokenType.ReserveWord);

                // Each token should be separated by a single space.
                Assert.AreEqual(token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);

            file = """
                i8 i16 i32 i64 bool f32 f64
                """;

            tokens = Enum.GetValues<ePrimitiveType>()
                .Select(token => token.Code()).ToList();

            tokenizer = StringToTokenizer(file);

            i = 0;
            while (tokenizer.NextToken(token))
            {
                Assert.AreEqual(token.Type, eTokenType.Type);

                // Each token should be separated by a single space.
                Assert.AreEqual(token.Code, tokens[i++]);
            }

            // Make sure we did everything.
            Assert.AreEqual(i, tokens.Count);
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

            Tokenizer.Token token = new();
            var tokenizer = StringToTokenizer(file);

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Identifier);
            Assert.AreEqual(token.Lexeme, "identifier");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(token.Lexeme, "==");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Identifier);
            Assert.AreEqual(token.Lexeme, "_value_idt");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(token.Lexeme, "=");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.String);
            Assert.AreEqual(token.Lexeme, "string");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(token.Lexeme, ";");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.ReserveWord);
            Assert.AreEqual(token.Lexeme, "const");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Identifier);
            Assert.AreEqual(token.Lexeme, "v");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(token.Lexeme, "=");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.Integer);
            Assert.AreEqual(token.Lexeme, "32");

            tokenizer.NextToken(token);
            Assert.AreEqual(token.Type, eTokenType.SyntaxToken);
            Assert.AreEqual(token.Lexeme, ";");
        }

        #region Utilities 

        private Tokenizer StringToTokenizer(string input)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);

            var reader = new StreamReader(stream);
            var tokenizer = new Tokenizer();
            tokenizer.SetStream(reader);

            return tokenizer;
        }

        #endregion
    }
}
