namespace BLang.Utils
{
    public static class ParserUtils
    {
        public static bool IsAssignmentOperator(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.Equal.Code() ||
                   token.Code == eTwoCharSyntaxToken.AddEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.SubEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.MulEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.DivEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.ModEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.AndEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.XorEquals.Code() ||
                   token.Code == eTwoCharSyntaxToken.OrEquals.Code() ||
                   token.Code == eThreeCharSyntaxToken.LslEquals.Code() ||
                   token.Code == eThreeCharSyntaxToken.LsrEquals.Code();
        }

        public static bool IsIfExpression(Tokenizer.Token token)
        {
            return token.Code == eReserveWord.If.Code();
        }

        public static bool IsLogicalOr(Tokenizer.Token token)
        {
            return token.Code == eTwoCharSyntaxToken.LogicalOr.Code();
        }

        public static bool IsLogicalAnd(Tokenizer.Token token)
        {
            return token.Code == eTwoCharSyntaxToken.LogicalAnd.Code();
        }

        public static bool IsBitwiseAnd(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.And.Code();
        }

        public static bool IsBitwiseXor(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.Xor.Code();
        }

        public static bool IsBitwiseOr(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.Or.Code();
        }

        public static bool IsEqOrNeq(Tokenizer.Token token)
        {
            return token.Code == eTwoCharSyntaxToken.AreEqual.Code() ||
                   token.Code == eTwoCharSyntaxToken.NotEqual.Code();
        }

        public static bool IsRelational(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.CloseAngleBrace.Code() ||
                   token.Code == eOneCharSyntaxToken.OpenAngleBrace.Code() ||
                   token.Code == eTwoCharSyntaxToken.Gte.Code() ||
                   token.Code == eTwoCharSyntaxToken.Lte.Code();
        }

        public static bool IsAddOp(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.Plus.Code() ||
                   token.Code == eOneCharSyntaxToken.Minus.Code();
        }

        public static bool IsMul(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.Star.Code() ||
                   token.Code == eOneCharSyntaxToken.Divide.Code() ||
                   token.Code == eOneCharSyntaxToken.Mod.Code();
        }

        public static bool IsLogicalShift(Tokenizer.Token token)
        {
            return token.Code == eTwoCharSyntaxToken.LogicalShiftRight.Code() ||
                   token.Code == eTwoCharSyntaxToken.LogicalShiftLeft.Code();
        }

        public static bool IsExprAtom(Tokenizer.Token token)
        {
            return IsConstant(token) ||
                   token.Type == eTokenType.Identifier;
        }

        /// <summary>
        /// Determines whether the token could represent a valid expression.
        /// </summary>
        public static bool IsExpressionStartToken(Tokenizer.Token token)
        {
            return token.Type == eTokenType.Identifier ||
                   token.Code == eOneCharSyntaxToken.OpenPar.Code() ||
                   token.Code == eReserveWord.If.Code() ||
                   IsExpressionPrefix(token) ||
                   IsConstant(token);
        }

        /// <summary>
        /// Whether the token is an increment or decrement token.
        /// </summary>
        /// <returns></returns>
        public static bool IsIncDec(Tokenizer.Token token)
        {
            return token.Code == eTwoCharSyntaxToken.Increment.Code() ||
                   token.Code == eTwoCharSyntaxToken.Decrement.Code();
        }

        public static bool IsExpressionPrefix(Tokenizer.Token token)
        {
            return token.Code == eOneCharSyntaxToken.Not.Code() ||
                   token.Code == eOneCharSyntaxToken.Minus.Code() ||
                   token.Code == eOneCharSyntaxToken.Compliment.Code() ||
                   IsIncDec(token);
        }

        public static bool IsExpressionPostFix(Tokenizer.Token token)
        {
            return IsIncDec(token) ||
                   // function call postfix.
                   token.Code == eOneCharSyntaxToken.OpenPar.Code() ||
                   // Array subscript.
                   token.Code == eOneCharSyntaxToken.OpenBrack.Code();
        }
        /// <summary>
        /// Whether the token represents a constant.
        /// </summary>
        /// <returns></returns>
        public static bool IsConstant(Tokenizer.Token token)
        {
            return token.Type == eTokenType.Integer ||
                   token.Type == eTokenType.FloatingPoint ||
                   token.Type == eTokenType.Char ||
                   token.Type == eTokenType.String ||
                   token.Code == eReserveWord.True.Code() ||
                   token.Code == eReserveWord.False.Code();
        }

        /// <summary>
        /// The set of all tokens that can be used in a statement as part of a function body.
        /// </summary>
        /// <returns></returns>
        public static bool IsStatementToken(Tokenizer.Token token)
        {
            return 
                   // Logic statement
                   token.Code == eReserveWord.If.Code() ||
                   // Loop statement
                   token.Code == eReserveWord.While.Code() ||
                   token.Code == eReserveWord.For.Code() ||
                   // Function call
                   token.Code == eReserveWord.Return.Code() ||
                   IsCodeStatementToken(token);
        }

        /// <summary>
        /// The subset of statements that can be used as an individual command.
        /// </summary>
        /// <returns></returns>
        public static bool IsCodeStatementToken(Tokenizer.Token token)
        {
            return IsVariableStart(token) ||
                   IsExpressionStartToken(token) ||
                   token.Code == eOneCharSyntaxToken.Semi.Code();
        }

        /// <summary>
        /// Is the token an item that belongs inside of a module definition.
        /// </summary>
        /// <returns></returns>
        public static bool IsModItem(Tokenizer.Token token)
        {
            return token.Code == eReserveWord.Module.Code() ||
                    IsFunctionStart(token) ||
                    IsVariableStart(token);
                    //token.Code == eReserveWord.) DATA STRUCTURES
        }

        public static bool IsFunctionStart(Tokenizer.Token token)
        {
            return token.Code == eReserveWord.Function.Code() ||
                   token.Code == eReserveWord.EntryPoint.Code();
        }

        public static bool IsVariableStart(Tokenizer.Token token)
        {
            return token.Code == eReserveWord.Let.Code();
        }
    }
}
