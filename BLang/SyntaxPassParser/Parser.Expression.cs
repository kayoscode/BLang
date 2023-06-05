using BLang.Utils;
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
            if (ParserUtils.IsAssignmentOperator(mToken))
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

            if (ParserUtils.IsIfExpression(mToken))
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
            if (ParserUtils.IsLogicalOr(mToken))
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
            if (ParserUtils.IsLogicalAnd(mToken))
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
            if (ParserUtils.IsBitwiseOr(mToken))
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
            if (ParserUtils.IsBitwiseXor(mToken))
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
            if (ParserUtils.IsBitwiseAnd(mToken))
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
            if (ParserUtils.IsEqOrNeq(mToken))
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
            if (ParserUtils.IsRelational(mToken))
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
            if (ParserUtils.IsLogicalShift(mToken))
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
            if (ParserUtils.IsAddOp(mToken))
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
            if (ParserUtils.IsMul(mToken))
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
            if (ParserUtils.IsExpressionPrefix(mToken))
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
            if (ParserUtils.IsExpressionPostFix(mToken))
            {
                PostFixOp();
                ExpressionPostFixTail();
            }
        }

        private void PostFixOp()
        {
            if (ParserUtils.IsIncDec(mToken))
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
            if (ParserUtils.IsExprAtom(mToken))
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
            else if (ParserUtils.IsIfExpression(mToken))
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
            if (ParserUtils.IsConstant(mToken))
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
    }
}
