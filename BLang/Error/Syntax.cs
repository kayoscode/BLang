using BLang.Utils;
using System.Diagnostics;

namespace BLang.Error
{
    internal static class ErrorRecoveryUtils
    {
        public static void ConsumeUntilExpectedToken(ParserContext context, Enum token)
        {
            while (context.Token.Code != SyntaxTokenAttributeData.Code(token))
            {
                if (!context.Tokenizer.NextToken())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Searches for a synchronization token until it finds one, then continues.
        /// None of the tokens discarded in panic mode are inserted into the stream.
        /// </summary>
        public static void RecoverPanicMode(ParserContext context)
        {
            while (!context.CurrentContext.SyncTokens().Contains(context.Token.Code) &&
                   context.Token.Type != eTokenType.EndOfStream)
            {
                context.Tokenizer.NextToken();
            }
        }

        public const string FakeIdt = "$";
    }

    /// <summary>
    /// Triggers panic mode when the error occurs.
    /// </summary>
    public abstract class PanicModeError : ParseError
    {
        protected PanicModeError(ParserContext context)
            : base(context)
        {
        }

        protected override sealed bool ChildRecoverFromError()
        {
            ErrorRecoveryUtils.RecoverPanicMode(Context);
            return true;
        }
    }

    /// <summary>
    /// Inserts the correct token if the error occurs.
    /// </summary>
    public abstract class TokenInsertionError : ParseError
    {
        public TokenInsertionError(ParserContext context) 
            : base(context) 
        {
            TokenToAdd = new();
        }

        protected override sealed bool ChildRecoverFromError()
        {
            Context.AddToken(TokenToAdd, Context.CurrentContext);
            return true;
        }

        protected Tokenizer.Token TokenToAdd { get; private set; }
    }

    public class UnexpectedTokenAtFileLevel : PanicModeError
    {
        public UnexpectedTokenAtFileLevel(ParserContext context) : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.UnexpectedTokenAtFileLevel;

        protected override string Message => "A file must include only a list of imports followed by a module block";
    }

    public class UnexpectedToken : PanicModeError
    {
        public UnexpectedToken(ParserContext context)
            : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.UnexpectedToken;

        protected override string Message
        {
            get
            {
                return $"Unexpected token: {Context.Token.Lexeme}";
            }
        }
    }

    public class MissingIdentifier : TokenInsertionError
    {
        public MissingIdentifier(ParserContext context) : base(context)
        {
            TokenToAdd.Lexeme = ErrorRecoveryUtils.FakeIdt;
            TokenToAdd.Type = eTokenType.Identifier;
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingIdentifier;

        protected override string Message 
        {
            get
            {
                var context = Context.CurrentContext;

                Trace.Assert(context == Parser.eParserContext.FunctionDefinition ||
                             context == Parser.eParserContext.VariableCreation ||
                             context == Parser.eParserContext.ImportStatement ||
                             context == Parser.eParserContext.CalleeParams ||
                             context == Parser.eParserContext.Module);

                var errorTypeText = context switch
                {
                    Parser.eParserContext.Module => "module definition",
                    Parser.eParserContext.FunctionDefinition => "function definition",
                    Parser.eParserContext.VariableCreation => "variable declaration",
                    Parser.eParserContext.ImportStatement => "import statement",
                    Parser.eParserContext.CalleeParams => "callee param",
                    _ => null
                };

                return $"Identifer was expected in {errorTypeText} instead '{Context.Token.Lexeme}' was found";
            }
        }
    }

    public class MissingTypeSpecifier : TokenInsertionError
    {

        public MissingTypeSpecifier(ParserContext context) : base(context)
        {
            TokenToAdd.Lexeme = ErrorRecoveryUtils.FakeIdt;
            TokenToAdd.Type = eTokenType.Type;
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingTypeSpecifier;

        protected override string Message 
        {
            get
            {
                var context = Context.CurrentContext;

                Trace.Assert(context == Parser.eParserContext.FunctionDefinition ||
                             context == Parser.eParserContext.VariableCreation ||
                             context == Parser.eParserContext.CalleeParams);

                var errorTypeText = context switch
                {
                    Parser.eParserContext.FunctionDefinition => "function definition",
                    Parser.eParserContext.VariableCreation => "variable definition",
                    Parser.eParserContext.CalleeParams => "callee param",
                    _ => null
                };

                return $"Type was expected in {errorTypeText}";
            }
        }
    }

    public class MissingInitializer : PanicModeError
    {
        public MissingInitializer(ParserContext context) : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingInitializer;

        protected override string Message 
        {
            get
            {
                return $"Implicitly typed variables must be initialized";
            }
        }
    }

    public class MissingSemicolon : TokenInsertionError
    {
        public MissingSemicolon(ParserContext context) : base(context)
        {
            TokenToAdd.Code = eOneCharSyntaxToken.Semi.Code();
            TokenToAdd.Lexeme = eOneCharSyntaxToken.Semi.AsLexeme();
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingSemicolon;

        protected override string Message 
        {
            get
            {
                return $"Expected ';'";
            }
        }
    }

    public class MissingSyntaxToken : TokenInsertionError 
    {
        private string mExpectedTokenString => SyntaxTokenAttributeData.AsLexeme(mExpectedTokenEnum);
        private Enum mExpectedTokenEnum;

        public MissingSyntaxToken(ParserContext context, Enum expectedToken) 
            : base(context)
        {
            mExpectedTokenEnum = expectedToken; 

            TokenToAdd.Lexeme = mExpectedTokenString;
            TokenToAdd.Type = eTokenType.SyntaxToken;
            TokenToAdd.Code = SyntaxTokenAttributeData.Code(mExpectedTokenEnum);
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingSyntaxToken;

        protected override string Message
        {
            get
            {
                return $"Expected '{mExpectedTokenString}' but found '{Context.Token.Lexeme}'";
            }
        }
    }

    public class MissingExpression : PanicModeError
    {
        public MissingExpression(ParserContext context) 
            : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingExpression;

        protected override string Message
        {
            get
            {
                return $"'{Context.Token.Lexeme}' does not represent a term in an expression";
            }
        }
    }

    public class ExpectedFunctionBody : PanicModeError
    {
        public ExpectedFunctionBody(ParserContext context)
            : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.ExpectedFunctionBody;

        protected override string Message
        {
            get
            {
                return "Expected function body";
            }
        }
    }

    public class InvalidForLoopStatement : PanicModeError
    {
        public InvalidForLoopStatement(ParserContext context)
            : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.InvalidForLoopStatement;

        protected override string Message
        {
            get
            {
                return "Invalid statement in for loop";
            }
        }
    }

    public class NoElseOnIfExpression : PanicModeError
    {
        public NoElseOnIfExpression(ParserContext context) 
            : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.NoElseOnIfExpression;

        protected override string Message
        {
            get
            {
                return "Else clause missing from if expression";
            }
        }
    }
}
