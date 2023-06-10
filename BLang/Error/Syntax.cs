using BLang.Utils;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace BLang.Error
{
    public class InvalidTokenAtFileLevel : ParseError
    {
        public InvalidTokenAtFileLevel(ParserContext context) : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.CriticalError;

        public override eParseError ErrorType => eParseError.UnexpectedTokenAtFileLevel;

        protected override string Message => "A file must include only a list of imports followed by a module block";

        protected override bool ChildRecoverFromError()
        {
            // Cannot recover from this error.
            // Maybe we can eventually insert the right token into the stream to put it into context
            // but maybe that's not worth it.

            // Or a simpler way to do it would be to pull tokens off until we find one that matches something that's 
            // expected at the file level.
            return false;
        }
    }

    public class UnexpectedToken : ParseError
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

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }

    public class MissingIdentifier : ParseError
    {
        public MissingIdentifier(ParserContext context) : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingIdentifier;

        protected override string Message 
        {
            get
            {
                var context = Context.CurrentContext;

                Trace.Assert(context == Parser.eNonTerminal.Function ||
                             context == Parser.eNonTerminal.VariableCreation ||
                             context == Parser.eNonTerminal.ImportStatement ||
                             context == Parser.eNonTerminal.CalleeParams ||
                             context == Parser.eNonTerminal.Module);

                var errorTypeText = context switch
                {
                    Parser.eNonTerminal.Module => "module definition",
                    Parser.eNonTerminal.Function => "function definition",
                    Parser.eNonTerminal.VariableCreation => "variable declaration",
                    Parser.eNonTerminal.ImportStatement => "import statement",
                    Parser.eNonTerminal.CalleeParams => "callee param",
                    _ => null
                };

                return $"Identifer was expected in {errorTypeText}";
            }
        }

        protected override bool ChildRecoverFromError()
        {
            // To recover from this error, insert a nameless identifier into the token stream.
            return true;
        }
    }

    public class MissingTypeSpecifier : ParseError
    {

        public MissingTypeSpecifier(ParserContext context) : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingTypeSpecifier;

        protected override string Message 
        {
            get
            {
                var context = Context.CurrentContext;

                Trace.Assert(context == Parser.eNonTerminal.Function ||
                             context == Parser.eNonTerminal.VariableCreation ||
                             context == Parser.eNonTerminal.CalleeParams);

                var errorTypeText = context switch
                {
                    Parser.eNonTerminal.Function => "function definition",
                    Parser.eNonTerminal.VariableCreation => "variable definition",
                    Parser.eNonTerminal.CalleeParams => "callee param",
                    _ => null
                };

                return $"Type was expected in {errorTypeText}";
            }
        }

        protected override bool ChildRecoverFromError()
        {
            // To recover from this error, insert a nameless identifier into the token stream.
            return true;
        }
    }

    public class MissingInitializer : ParseError
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

        protected override bool ChildRecoverFromError()
        {
            // To recover from this error, insert a nameless identifier into the token stream.
            return true;
        }
    }

    public class MissingSemicolon : ParseError
    {
        public MissingSemicolon(ParserContext context) : base(context)
        {
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

        protected override bool ChildRecoverFromError()
        {
            // To recover from this error, insert a nameless identifier into the token stream.
            return true;
        }
    }

    public class MissingSyntaxToken : ParseError 
    {
        private string mExpectedToken;

        public MissingSyntaxToken(ParserContext context, string expectedToken) 
            : base(context)
        {
            mExpectedToken = expectedToken;
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.MissingSyntaxToken;

        protected override string Message
        {
            get
            {
                return $"Expected '{mExpectedToken}' but found '{Context.Token.Lexeme}'";
            }
        }

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }

    public class MissingExpression : ParseError
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

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }

    public class ExpectedFunctionBody : ParseError
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

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }

    public class InvalidForLoopStatement : ParseError
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

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }

    public class NoElseOnIfExpression : ParseError
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

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }
}
