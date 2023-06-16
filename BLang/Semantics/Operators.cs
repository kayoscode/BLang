using System.Diagnostics;

namespace BLang.Semantics
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ConvertsFromAttribute : Attribute
    {
        public long ConvertsFromTokenCode;

        public ConvertsFromAttribute(eOneCharSyntaxToken op)
        {
            ConvertsFromTokenCode = op.Code();
        }

        public ConvertsFromAttribute(eTwoCharSyntaxToken op)
        {
            ConvertsFromTokenCode = op.Code();
        }

        public ConvertsFromAttribute(eThreeCharSyntaxToken op)
        {
            ConvertsFromTokenCode = op.Code();
        }
    }

    public enum eMathOperator
    {
        // One char ops.
        [ConvertsFrom(eOneCharSyntaxToken.Tilde)]
        Complement,
        [ConvertsFrom(eOneCharSyntaxToken.Star)]
        Multiply,
        [ConvertsFrom(eOneCharSyntaxToken.And)]
        BinaryAnd,
        [ConvertsFrom(eOneCharSyntaxToken.Or)]
        BinaryOr,
        [ConvertsFrom(eOneCharSyntaxToken.Xor)]
        BinaryXor,
        [ConvertsFrom(eOneCharSyntaxToken.Not)]
        Not,
        [ConvertsFrom(eOneCharSyntaxToken.Equal)]
        Assign,
        [ConvertsFrom(eOneCharSyntaxToken.Minus)]
        Subtract,
        [ConvertsFrom(eOneCharSyntaxToken.Plus)]
        Add,
        [ConvertsFrom(eOneCharSyntaxToken.Divide)]
        Divide,
        [ConvertsFrom(eOneCharSyntaxToken.Mod)]
        Modulus,

        // Two char ops.
        [ConvertsFrom(eTwoCharSyntaxToken.Gte)]
        Gte,
        [ConvertsFrom(eTwoCharSyntaxToken.Lte)]
        Lte,
        [ConvertsFrom(eTwoCharSyntaxToken.AreEqual)]
        LogicalEqual,
        [ConvertsFrom(eTwoCharSyntaxToken.NotEqual)]
        LogicalNotEqual,
        [ConvertsFrom(eTwoCharSyntaxToken.LogicalAnd)]
        LogicalAnd,
        [ConvertsFrom(eTwoCharSyntaxToken.LogicalOr)]
        LogicalOr,
        [ConvertsFrom(eTwoCharSyntaxToken.LogicalShiftLeft)]
        LogicalShiftLeft,
        [ConvertsFrom(eTwoCharSyntaxToken.LogicalShiftRight)]
        LogicalShiftRight,
        [ConvertsFrom(eTwoCharSyntaxToken.Increment)]
        Increment,
        [ConvertsFrom(eTwoCharSyntaxToken.Decrement)]
        Decrement,
        [ConvertsFrom(eTwoCharSyntaxToken.AddEquals)]
        AddAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.SubEquals)]
        SubAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.MulEquals)]
        MulAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.DivEquals)]
        DivAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.ModEquals)]
        ModAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.AndEquals)]
        BinaryAndAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.XorEquals)]
        BinaryXorAssign,
        [ConvertsFrom(eTwoCharSyntaxToken.OrEquals)]
        BinaryOrAssign,

        // Three char ops.
        [ConvertsFrom(eThreeCharSyntaxToken.LsrEquals)]
        LogicalShiftRightAssign,
        [ConvertsFrom(eThreeCharSyntaxToken.LslEquals)]
        LogicalShiftLeftAssign,
    }

    public static class OperatorAttributeUtils
    {
        static OperatorAttributeUtils()
        {
        }

        public static eMathOperator ToOperator(long tokenCode)
        {
            foreach (var op in mAllOperators)
            {
                long conversion = mAttribute.GetAttribute(op).ConvertsFromTokenCode;

                if (conversion == tokenCode)
                {
                    return op;
                }
            }

            Trace.Assert(false);
            throw new ArgumentException($"Token code {tokenCode} has no valid conversion to an operator.");
        }

        private static readonly Dictionary<long, eMathOperator> mTokenConversions = new();
        private static readonly AttributeCacheHelper<ConvertsFromAttribute, eMathOperator> mAttribute = new();

        private static readonly IReadOnlyList<eMathOperator> mAllOperators = Enum.GetValues<eMathOperator>();
    }
}
