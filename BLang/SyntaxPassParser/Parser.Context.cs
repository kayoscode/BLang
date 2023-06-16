namespace BLang
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class SyncTokenAttribute : Attribute 
    {
        public List<long> SyncTokens { get; set; } = new();

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="syncTokenCodes"></param>
        public SyncTokenAttribute(eOneCharSyntaxToken[] oneCharSyncTokens = null,
                                  eTwoCharSyntaxToken[] twoCharSyncTokens = null,
                                  eThreeCharSyntaxToken[] threeCharSyncTokens = null,
                                  eReserveWord[] reserveWordSyncTokens = null)
        {
            if (oneCharSyncTokens != null)
            {
                foreach (var token in oneCharSyncTokens)
                {
                    SyncTokens.Add(token.Code());
                }
            }

            if (twoCharSyncTokens != null)
            {
                foreach (var token in twoCharSyncTokens)
                {
                    SyncTokens.Add(token.Code());
                }
            }

            if (threeCharSyncTokens != null)
            {
                foreach (var token in threeCharSyncTokens)
                {
                    SyncTokens.Add(token.Code());
                }
            }

            if (reserveWordSyncTokens != null)
            {
                foreach (var token in reserveWordSyncTokens)
                {
                    SyncTokens.Add(token.Code());
                }
            }
        }
    }

    public partial class Parser
    {
        public enum eParserContext
        {
            [SyncToken(reserveWordSyncTokens: new eReserveWord[]
            {
                eReserveWord.Module,
            })]
            File,

            [SyncToken(reserveWordSyncTokens: new eReserveWord[]
            {
                eReserveWord.Function,
                eReserveWord.EntryPoint,
                eReserveWord.Module,
                eReserveWord.Let
            })]
            Module,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Semi, eOneCharSyntaxToken.Period, eOneCharSyntaxToken.CloseBrace
            })]
            ImportStatement,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.ClosePar, eOneCharSyntaxToken.OpenBrace, eOneCharSyntaxToken.CloseBrace
            })]
            FunctionDefinition,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Comma, eOneCharSyntaxToken.ClosePar, eOneCharSyntaxToken.CloseBrace
            })]
            CalleeParams,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Comma, eOneCharSyntaxToken.ClosePar, eOneCharSyntaxToken.CloseBrace
            })]
            CallerParams,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Semi, eOneCharSyntaxToken.CloseBrace
            })]
            VariableCreation,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Semi,

                // Expressions can sync with any valid operator except for postfix/prefix.
                eOneCharSyntaxToken.Star, eOneCharSyntaxToken.Divide, eOneCharSyntaxToken.Mod,
                eOneCharSyntaxToken.Plus, eOneCharSyntaxToken.Minus,

                eOneCharSyntaxToken.Equal,
                eOneCharSyntaxToken.And, eOneCharSyntaxToken.Or, eOneCharSyntaxToken.Xor,

            }, new eTwoCharSyntaxToken[]
            {
                eTwoCharSyntaxToken.AddEquals,
                eTwoCharSyntaxToken.SubEquals,
                eTwoCharSyntaxToken.MulEquals,
                eTwoCharSyntaxToken.DivEquals,
                eTwoCharSyntaxToken.ModEquals,
                eTwoCharSyntaxToken.AndEquals,
                eTwoCharSyntaxToken.XorEquals,
                eTwoCharSyntaxToken.OrEquals,

                eTwoCharSyntaxToken.LogicalOr, eTwoCharSyntaxToken.LogicalAnd,
                eTwoCharSyntaxToken.LogicalShiftLeft, eTwoCharSyntaxToken.LogicalShiftRight,
                eTwoCharSyntaxToken.AreEqual, eTwoCharSyntaxToken.NotEqual,
                eTwoCharSyntaxToken.Gte, eTwoCharSyntaxToken.Lte
            }, new eThreeCharSyntaxToken[]
            {
                eThreeCharSyntaxToken.LslEquals,
                eThreeCharSyntaxToken.LsrEquals,
            })]
            Expression,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.CloseBrace, eOneCharSyntaxToken.Semi
            })]
            CodeBlock,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Semi, eOneCharSyntaxToken.CloseBrace
            })]
            Statement,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.OpenBrace,
                eOneCharSyntaxToken.Semi, eOneCharSyntaxToken.CloseBrace
            })]
            // If and while loop statements.
            BlockStatement,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.Semi,
                eOneCharSyntaxToken.ClosePar,
                eOneCharSyntaxToken.CloseBrace
            })]
            IfExpression,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.ClosePar, eOneCharSyntaxToken.Comma, eOneCharSyntaxToken.Semi,
                eOneCharSyntaxToken.CloseBrace
            })]
            FunctionCall,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.CloseBrace, eOneCharSyntaxToken.OpenBrace,
                eOneCharSyntaxToken.ClosePar, eOneCharSyntaxToken.Semi
            })]
            ForLoop,

            [SyncToken(new eOneCharSyntaxToken[]
            {
                eOneCharSyntaxToken.CloseBrack, eOneCharSyntaxToken.Semi,
                eOneCharSyntaxToken.CloseBrace
            })]
            ArrayIndex
        }
    }

    public static class ParserContextAttributeUtils
    {
        public static IReadOnlyList<long> SyncTokens(this Parser.eParserContext context)
        {
            return mAttribute.GetAttribute(context).SyncTokens;
        }

        private static readonly AttributeCacheHelper<SyncTokenAttribute, Parser.eParserContext> mAttribute = new();
    }
}
