using System.Diagnostics;

namespace BLang
{
    /// <summary>
    /// Code for handling expressions in the parser.
    /// </summary>
    public partial class Parser
    {
        /// <summary>
        /// Expressions need to handle grouping by priority. We cant just group left to right and
        /// expect to get a valid result.
        /// 
        /// Don't even bother trying to read this if you dont immediately understand.
        /// Ask me instead.
        /// </summary>
        private void Expression()
        {
            ExpressionAssignmentOperator();
        }

        // Level 14 -> assignment operators.
        private void ExpressionAssignmentOperator()
        {
            ExpressionIfExpression();
            ExpressionAssignmentOperatorTail();
        }

        private void ExpressionAssignmentOperatorTail()
        {
            if (IsAssignmentOperator())
            {
                AdvanceToken();
                ExpressionIfExpression();
                ExpressionAssignmentOperatorTail();
            }
        }

        // Level 13 -> if expression.
        private void ExpressionIfExpression()
        {
            ExpressionLogicalOr();

            if (IsIfExpression())
            {
                IfExpression();
            }
        }

        // Level 12 -> logical OR
        private void ExpressionLogicalOr()
        {
            ExpressionLogicalAnd();
            ExpressionLogicalOrTail();
        }

        private void ExpressionLogicalOrTail()
        {
            if (IsLogicalOr())
            {
                AdvanceToken();
                ExpressionLogicalAnd();
                ExpressionLogicalOrTail();
            }
        }

        // Level 11 -> logical AND
        private void ExpressionLogicalAnd()
        {
            ExpressionBitwiseOr();
            ExpressionLogicalAndTail();
        }

        private void ExpressionLogicalAndTail()
        {
            if (IsLogicalAnd())
            {
                AdvanceToken();
                ExpressionBitwiseOr();
                ExpressionLogicalAndTail();
            }
        }

        // Level 10 -> bitwise OR
        private void ExpressionBitwiseOr()
        {
            ExpressionBitwiseXor();
            ExpressionBitwiseOrTail();
        }

        private void ExpressionBitwiseOrTail()
        {
            if (IsBitwiseOr())
            {
                AdvanceToken();
                ExpressionBitwiseXor();
                ExpressionBitwiseOrTail();
            }
        }

        // Level 9 -> bitwise XOR
        private void ExpressionBitwiseXor()
        {
            ExpressionBitwiseAnd();
            ExpressionBitwiseXorTail();
        }

        private void ExpressionBitwiseXorTail()
        {
            if (IsBitwiseXor())
            {
                AdvanceToken();
                ExpressionBitwiseAnd();
                ExpressionBitwiseXorTail();
            }
        }

        // Level 8 -> bitwise AND
        private void ExpressionBitwiseAnd()
        {
            ExpressionEqNeq();
            ExpressionBitwiseAndTail();
        }

        private void ExpressionBitwiseAndTail()
        {
            if (IsBitwiseAnd())
            {
                AdvanceToken();
                ExpressionEqNeq();
                ExpressionBitwiseAndTail();
            }
        }

        // Level 7 -> equal and not equal.
        private void ExpressionEqNeq()
        {
            ExpressionRelational();
            ExpressionEqNeqTail();
        }

        private void ExpressionEqNeqTail()
        {
            if (IsEqOrNeq())
            {
                AdvanceToken();
                ExpressionRelational();
                ExpressionEqNeqTail();
            }
        }

        // Level 6 -> relational operators.
        private void ExpressionRelational()
        {
            ExpressionLogicalShift();
            ExpressionRelationalTail();
        }

        private void ExpressionRelationalTail()
        {
            if (IsRelational())
            {
                AdvanceToken();
                ExpressionLogicalShift();
                ExpressionRelationalTail();
            }
        }

        // Level 5 -> logical shifts.
        private void ExpressionLogicalShift()
        {
            ExpressionAdd();
            ExpressionLogicalShiftTail();
        }

        private void ExpressionLogicalShiftTail()
        {
            if (IsLogicalShift())
            {
                AdvanceToken();
                ExpressionAdd();
                ExpressionLogicalShiftTail();
            }
        }

        // Level 4 -> addition and subtraction.
        private void ExpressionAdd()
        {
            ExpressionTerm();
            ExpressionAddTail();
        }

        private void ExpressionAddTail()
        {
            if (IsAddOp())
            {
                AdvanceToken();
                ExpressionTerm();
                ExpressionAddTail();
            }
        }

        // Level 3 -> multiplication and division.
        private void ExpressionTerm()
        {
            ExpressionPrefix();
            ExpressionTermTail();
        }

        private void ExpressionTermTail()
        {
            if (IsMul())
            {
                AdvanceToken();
                ExpressionPrefix();
                ExpressionTermTail();
            }
        }

        // Level 2 -> right associative prefix.
        private void ExpressionPrefix()
        {
            // Right associative.
            ExpressionPrefixTail();
            ExpressionPostFix();
        }

        private void ExpressionPrefixTail()
        {
            if (IsExpressionPrefix())
            {
                AdvanceToken();
                ExpressionPrefixTail();
            }
        }

        // Level 1 -> post fix.
        private void ExpressionPostFix()
        {
            ExpressionFactor();
            ExpressionPostFixTail();
        }

        private void ExpressionPostFixTail()
        {
            if (IsExpressionPostFix())
            {
                PostFixOp();
                ExpressionPostFixTail();
            }
        }

        private void PostFixOp()
        {
            if (IsIncDec())
            {
                AdvanceToken();
            }
            else if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                // Function call.
                FunctionCall();
            }
            else if (mToken.Code == eOneCharSyntaxToken.OpenBrack.Code())
            {
                // Array index.
                ArrayIndex();
            }
            else
            {
                Trace.Assert(false, "Invalid postfix expression");
            }
        }

        // Level 0 -> token level interpretation.
        private void ExpressionFactor()
        {
            // Handle the different types of individual values you can receive.
            if (IsExprAtom())
            {
                ExpressionAtom();
            }
            else if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                AdvanceToken();
                Expression();

                if (mToken.Code == eOneCharSyntaxToken.ClosePar.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }
            }
            else if (IsIfExpression())
            {
                // Don't need to do anything, we will handle the if expression upstream.
            }
            else
            {
                // AddError();
                Debugger.Break();
            }
        }

        private void ExpressionAtom()
        {
            if (IsConstant())
            {
                AdvanceToken();
            }
            else if (mToken.Type == eTokenType.Identifier)
            {
                ExpressionVariable();
            }
        }

        /// <summary>
        /// Handles and consumes any format an expression atom can be in.
        /// </summary>
        private void ExpressionVariable()
        {
            AdvanceToken();

            if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                FunctionCall();
            }
            //else if (mToken.Code == eOneCharSyntaxToken.OpenBrack.Code())
            //{
            // TODO.
            //}
        }

        /// <summary>
        /// An if statement that forces a value to be returned at the end of each block. 
        /// Must be a single expression.
        /// </summary>
        private void IfExpression()
        {
            LogEnterNonTerminal(eNonTerminal.IfExpression);
            // If we get here, we assume that we have an if token currently.
            AdvanceToken();

            if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                AdvanceToken();
                Expression();

                if (mToken.Code == eOneCharSyntaxToken.ClosePar.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }

                if (mToken.Code == eTwoCharSyntaxToken.Arrow.Code())
                {
                    ExpressionCodeBlock();

                    if (mToken.Code == eReserveWord.Else.Code())
                    {
                        AdvanceToken();
                        if (mToken.Code == eReserveWord.If.Code())
                        {
                            IfExpression();
                        }
                        else if (mToken.Code == eTwoCharSyntaxToken.Arrow.Code())
                        {
                            ExpressionCodeBlock();
                        }
                        else
                        {
                            // AddError();
                            Debugger.Break();
                        }
                    }
                    else
                    {
                        // Add error, must have else clause
                        Debugger.Break();
                    }
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }
            }
            else
            {
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.IfExpression);
        }

        #region Utilities

        private bool IsAssignmentOperator()
        {
            return mToken.Code == eOneCharSyntaxToken.Equal.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.AddEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.SubEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.MulEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.DivEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.ModEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.AndEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.XorEquals.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.OrEquals.Code() ||
                   mToken.Code == eThreeCharSyntaxToken.LslEquals.Code() ||
                   mToken.Code == eThreeCharSyntaxToken.LsrEquals.Code();
        }

        private bool IsIfExpression()
        {
            return mToken.Code == eReserveWord.If.Code();
        }

        private bool IsLogicalOr()
        {
            return mToken.Code == eTwoCharSyntaxToken.LogicalOr.Code();
        }

        private bool IsLogicalAnd()
        {
            return mToken.Code == eTwoCharSyntaxToken.LogicalAnd.Code();
        }

        private bool IsBitwiseAnd()
        {
            return mToken.Code == eOneCharSyntaxToken.And.Code();
        }

        private bool IsBitwiseXor()
        {
            return mToken.Code == eOneCharSyntaxToken.Xor.Code();
        }

        private bool IsBitwiseOr()
        {
            return mToken.Code == eOneCharSyntaxToken.Or.Code();
        }

        private bool IsEqOrNeq()
        {
            return mToken.Code == eTwoCharSyntaxToken.AreEqual.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.NotEqual.Code();
        }

        private bool IsRelational()
        {
            return mToken.Code == eOneCharSyntaxToken.CloseAngleBrace.Code() ||
                   mToken.Code == eOneCharSyntaxToken.OpenAngleBrace.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.Gte.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.Lte.Code();
        }

        private bool IsAddOp()
        {
            return mToken.Code == eOneCharSyntaxToken.Plus.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Minus.Code();
        }

        private bool IsMul()
        {
            return mToken.Code == eOneCharSyntaxToken.Star.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Divide.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Mod.Code();
        }

        private bool IsLogicalShift()
        {
            return mToken.Code == eTwoCharSyntaxToken.LogicalShiftRight.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.LogicalShiftLeft.Code();
        }

        private bool IsExprAtom()
        {
            return IsConstant() ||
                   mToken.Type == eTokenType.Identifier;
        }

        /// <summary>
        /// Determines whether the token could represent a valid expression.
        /// </summary>
        private bool IsExpressionStartToken()
        {
            return mToken.Type == eTokenType.Identifier ||
                   mToken.Code == eOneCharSyntaxToken.OpenPar.Code() ||
                   mToken.Code == eReserveWord.If.Code() ||
                   IsExpressionPrefix() ||
                   IsConstant();
        }

        /// <summary>
        /// Whether the token is an increment or decrement token.
        /// </summary>
        /// <returns></returns>
        private bool IsIncDec()
        {
            return mToken.Code == eTwoCharSyntaxToken.Increment.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.Decrement.Code();
        }

        private bool IsExpressionPrefix()
        {
            return mToken.Code == eOneCharSyntaxToken.Not.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Minus.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Compliment.Code() ||
                   IsIncDec();
        }

        private bool IsExpressionPostFix()
        {
            return IsIncDec() ||
                   // function call postfix.
                   mToken.Code == eOneCharSyntaxToken.OpenPar.Code() ||
                   // Array subscript.
                   mToken.Code == eOneCharSyntaxToken.OpenBrack.Code();
        }

        #endregion
    }
}
