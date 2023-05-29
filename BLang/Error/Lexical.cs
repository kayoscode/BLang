namespace BLang.Error
{
    /// <summary>
    /// Error created when a token is not recognized by the system.
    /// This error terminates the entire compilation process.
    /// </summary>
    public class UnexpectedCharacter : ParseError
    {
        public UnexpectedCharacter(ParserContext context) 
            : base(context)
        {
        }

        protected override string Message
        {
            get
            {
                return $"Invalid token";
            }
        }

        public override eParseError ErrorCode => eParseError.UnexpectedCharacter;

        public override eErrorLevel Level => eErrorLevel.Error;

        protected override bool ChildRecoverFromError()
        {
            return true;
        }
    }

    public class InvalidRealLiteral : ParseError
    {
        public InvalidRealLiteral(ParserContext context)
            : base(context)
        {
        }

        protected override string Message
        {
            get
            {
                return $"Incorrect floating point format";
            }
        }

        public override eParseError ErrorCode => eParseError.InvalidRealLiteral;

        public override eErrorLevel Level => eErrorLevel.Error;

        protected override bool ChildRecoverFromError()
        {
            // Continue and just skip this token.
            return true;
        }
    }

    public class InvalidCharLiteral : ParseError
    {
        public InvalidCharLiteral(ParserContext context)
            : base(context)
        {
        }

        protected override string Message
        {
            get
            {
                return $"Too many characters in specified in character literal";
            }
        }

        public override eParseError ErrorCode => eParseError.InvalidCharacterLiteral;

        public override eErrorLevel Level => eErrorLevel.Error;

        protected override bool ChildRecoverFromError()
        {
            // Get tokens until we reach the end of the line, or another single quote.
            return true;
        }
    }

    public class InvalidNumberLiteral : ParseError
    {
        public InvalidNumberLiteral(ParserContext context)
            : base(context)
        {
        }

        protected override string Message
        {
            get
            {
                return $"Invalid integer number";
            }
        }

        public override eParseError ErrorCode => eParseError.InvalidNumberLiteral;

        public override eErrorLevel Level => eErrorLevel.Error;

        protected override bool ChildRecoverFromError()
        {
            // Continue and just skip this token.
            return true;
        }
    }

    public class UnrecognizedEscapeSequence : ParseError
    {
        public UnrecognizedEscapeSequence(ParserContext context)
            : base(context)
        {
        }

        protected override string Message
        {
            get
            {
                return $"Unrecognized escape sequence";
            }
        }

        public override eParseError ErrorCode => eParseError.UnrecognizedEscapeSequence;

        public override eErrorLevel Level => eErrorLevel.Error;

        protected override bool ChildRecoverFromError()
        {
            // Continue and just skip this token.
            return true;
        }
    }
}
