using BLang.Utils;
using System.Diagnostics;

namespace BLang.Error
{
    public class MissingIdentifier : ParseError
    {
        public MissingIdentifier(ParserContext context) : base(context)
        {
        }

        public override eErrorLevel Level => eErrorLevel.Error;

        public override eParseError ErrorType => eParseError.IdentifierExpected;

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
                    Parser.eNonTerminal.VariableCreation => "variable definition",
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
            return false;
        }
    }
}
