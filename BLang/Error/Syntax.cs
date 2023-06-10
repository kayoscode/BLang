using BLang.Utils;
using System.Diagnostics;

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
                             context == Parser.eNonTerminal.ImportStatement);

                var errorTypeText = context switch
                {
                    Parser.eNonTerminal.Function => "function definition",
                    Parser.eNonTerminal.VariableCreation => "variable declaration",
                    Parser.eNonTerminal.ImportStatement => "import statement",
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
                             context == Parser.eNonTerminal.VariableCreation);

                var errorTypeText = context switch
                {
                    Parser.eNonTerminal.Function => "function definition",
                    Parser.eNonTerminal.VariableCreation => "variable definition",
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
                return $"';' expected";
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
                return $"Expected {mExpectedToken}, but found {Context.Token.Lexeme}";
            }
        }

        protected override bool ChildRecoverFromError()
        {
            throw new NotImplementedException();
        }
    }
}
