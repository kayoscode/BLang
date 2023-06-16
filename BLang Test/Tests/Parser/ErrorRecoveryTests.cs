using BLang;
using BLang.Error;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace BLang_Test.Tests
{
    [TestClass]
    public class ErrorRecoveryTests
    {
        [TestMethod]
        public void TestImportErrors()
        {
            Parser parser;
            string file = """
                import test.test.test3;
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                // No expected errors.
                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 0);
            }

            file = """
                import test..test..;
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 3);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.MissingIdentifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingIdentifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[2], eParseError.MissingIdentifier);
            }

            file = """
                import mod.test;
                import mod.idt,asdl;
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 4);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.MissingIdentifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingIdentifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[2], eParseError.MissingSemicolon);
                Assert.AreEqual(parser.ErrorLogger.Errors[3], eParseError.UnexpectedTokenAtFileLevel);
            }
        }

        [TestMethod]
        public void TestFuncModStatement()
        {
            Parser parser;
            string file = """
                mod TestModule 
                {
                    // We expect to be allowed to declare variables in a module.
                    let a: i32 = 12;
                    let b = 12;

                    // We expect to be able to create a function with an entrypt.
                    entrypt Main() {
                    }
                    
                    // We expect to be able to create multiple functions in a module.
                    fn Main2(a: i32, b: bool) 
                    {
                    }

                    fn Main3: i32 () 
                    {
                    }
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 0);
            }

            // In this test case, we created a module, then incorrectly defined a couple functions.
            // The code is not able to sync with anything until it finds the last item. The function.
            // Perhaps this functionality will change. Maybe the tokenizer should try to figure out what 
            // the incorrect syntax item was supposed to be. In this case, there was a codeblock, so maybe
            // it could figure out its supposed to be a function.
            file = """
                mod TestModule 
                {
                	int Main()
                	{
                	}

                	int Main2() 
                	{
                	}

                	fn Main3() 
                	{
                		let a;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 2);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.UnexpectedToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingInitializer);
            }

            // Test with a file just straight riddled with errors.
            // Make sure that we can recover from previous ones, and still end up in a state to detect the ones down the road.
            file = """
                mod TestMod 
                {
                    int main() {
                    }

                    entrypt main: (a, b) 
                    {
                        let a 12 = 12;
                        let a
                    }

                    fn main: i32(a: i32) 
                    {
                        :
                    }
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 10);
                // Unexpected token "int"
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.UnexpectedToken);
                // Recovered and found the missing type specifier on the main func.
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingTypeSpecifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[2], eParseError.MissingSyntaxToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[3], eParseError.MissingTypeSpecifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[4], eParseError.MissingSyntaxToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[5], eParseError.MissingTypeSpecifier);
                Assert.AreEqual(parser.ErrorLogger.Errors[6], eParseError.MissingInitializer);
                Assert.AreEqual(parser.ErrorLogger.Errors[7], eParseError.MissingInitializer);
                Assert.AreEqual(parser.ErrorLogger.Errors[8], eParseError.MissingSemicolon);
                Assert.AreEqual(parser.ErrorLogger.Errors[9], eParseError.UnexpectedToken);
            }

            file = """
                mod TestModule 
                {
                	fn main: i32 (a: i32, b: i32) 
                	{
                		let a 12;
                		let b 12;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 2);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.MissingInitializer);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingInitializer);
            }
        }

        [TestMethod]
        public void TestExpressions()
        {
            Parser parser;

            // Clean file.
            var file = """
                mod TestModule 
                {
                	fn main: i32 (a: i32, b: i32) 
                	{
                		let a: i32 = 12;
                		let b: i32 = 12;

                		a = 12;
                		b = 1112;
                		a = b;

                		a *= 2;
                		b *= (a * 2) + 232 - 11;

                		let a = if (false) => 0 else => 1;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 0);
            }

            // No else clause on if expression, missing initializer on variable a.
            // missing two arrow syntax tokens.
            // Missing semicolon at the end.
            file = """
                mod TestModule 
                {
                	fn main: i32 (a: i32, b: i32) 
                	{
                		let b: i32 = if (true) => false;
                		let a;

                		a *= if (b) 1 else 0
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 6);
            }

            file = """
                mod TestModule 
                {
                	fn main: i32 (a: i32, b: i32) 
                	{
                		a *+ 12 +* 12;
                		a *<< 12;

                		a += ;

                        // This is NOT an error because really it is a * -12, but it looks like the others.
                        // Should not produce an error.
                        a *- 12;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 4);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.UnexpectedToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.UnexpectedToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[2], eParseError.UnexpectedToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[3], eParseError.UnexpectedToken);
            }

            file = """
                mod TestModule 
                {
                	fn main() 
                	{
                		let a: i32;
                		let b: i32;

                		// Function call, it looks weird though.
                		a = a122 ( b );

                		a = (b) 8 a
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 2);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.MissingSemicolon);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingSemicolon);
            }

            file = """
                mod TestModule 
                {
                	fn main() 
                	{
                		// Completely legal syntax, invalid semantics.
                		let b == (a = a += 1++);

                		// Missing paired left par.
                		a * 2);

                		// missing right par
                		let a: i32 = (a == 2;

                		let a;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 4);
                Assert.AreEqual(parser.ErrorLogger.Errors[0], eParseError.MissingInitializer);
                Assert.AreEqual(parser.ErrorLogger.Errors[1], eParseError.MissingSemicolon);
                Assert.AreEqual(parser.ErrorLogger.Errors[2], eParseError.MissingSyntaxToken);
                Assert.AreEqual(parser.ErrorLogger.Errors[3], eParseError.MissingInitializer);
            }

            // TODO: Array index, prefix/postfix
        }

        [TestMethod]
        public void TestIfAndWhileLoop()
        {
            Parser parser;
            // Weird code, but no syntax errors.
            string file = """
                mod TestModule 
                {
                	fn main() 
                	{
                		let a = 12;
                		while (a >= 0)	 
                		{
                			a --;
                		}

                		// Weird way to structure an if statement, but sure.
                		if (if (a == 0) => true else => false) 
                		{
                			if (true);
                			while (true) 
                			{
                			;;;;;
                			}
                		}

                		if (a);

                		while (b > a || a);

                		if (a++ >= 0 && (a-- || b++ > if (true) => 0 
                									  else => false)) 
                		{
                		}
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                // No expected errors.
                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 0);
            }

            // More than one or zero expressions.
            // This file has more than a few errors, just use the error count to verify.
            file = """
                mod TestModule 
                {
                	fn main() 
                	{
                		// Empty conditions.
                		if ();
                		while ();

                		// Using like a function.
                		if (a: i32, b: i32) {
                		} 

                		while (a: i32, b: i32) {
                		}

                		if (a == true b = false) ;
                		while ;

                		while (;
                		while ;);
                		let a;
                		let a;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 18);
            }
        }

        [TestMethod]
        public void TestForLoop()
        {
            Parser parser;
            string file = """
                mod TestModule 
                {
                	fn main() 
                	{
                		for (let i: i32 = 0; i < 12; i++);
                		for (let i: i32 = 0, i < 12, i++);
                		for (let i: i32 = 0 i < 12 i++);

                		for let i: i32 = 0; i < 12; i++))));

                		let a;
                	}
                }
                """;

            using (StreamReader sr = StringToStream(file))
            {
                parser = new();
                parser.ParseFile(sr);

                Assert.AreEqual(parser.ErrorLogger.ErrorCount, 8);
            }
        }

        public StreamReader StringToStream(string input)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new(byteArray);

            return new StreamReader(stream);
        }
    }
}
