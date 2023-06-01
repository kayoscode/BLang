namespace BLang
{
    /// <summary>
    /// Syntax tokens with two characters.
    /// </summary>
    public enum eTwoCharSyntaxToken
    {
        [TwoCharSyntaxToken('>', '=')]
        Gte,
        [TwoCharSyntaxToken('<', '=')]
        Lte,
        [TwoCharSyntaxToken('=', '=')]
        AreEqual,
        [TwoCharSyntaxToken('!', '=')]
        NotEqual,
        [TwoCharSyntaxToken('&', '&')]
        AndAnd,
        [TwoCharSyntaxToken('|', '|')]
        OrOr,
        [TwoCharSyntaxToken('<', '<')]
        LogicalShiftLeft,
        [TwoCharSyntaxToken('>', '>')]
        LogicalShiftRight,
        [TwoCharSyntaxToken('=', '>')]
        Arrow,
        [TwoCharSyntaxToken('+', '+')]
        Increment,
        [TwoCharSyntaxToken('-', '-')]
        Decrement,
    }

    /// <summary>
    /// List of possible syntax tokens.
    /// </summary>
    public enum eOneCharSyntaxToken
    {
        [OneCharSyntaxToken(',')]
        Comma,
        [OneCharSyntaxToken('.')]
        Period,
        [OneCharSyntaxToken('~')]
        Compliment,
        [OneCharSyntaxToken('*')]
        Star,
        [OneCharSyntaxToken('(')]
        OpenPar,
        [OneCharSyntaxToken(')')]
        ClosePar,
        [OneCharSyntaxToken('{')]
        OpenBrace,
        [OneCharSyntaxToken('}')]
        CloseBrace,
        [OneCharSyntaxToken('[')]
        OpenBrack,
        [OneCharSyntaxToken(']')]
        CloseBrack,
        [OneCharSyntaxToken('&')]
        And,
        [OneCharSyntaxToken('|')]
        Or,
        [OneCharSyntaxToken('^')]
        Xor,
        [OneCharSyntaxToken('!')]
        Not,
        [OneCharSyntaxToken('>')]
        CloseAngleBrace,
        [OneCharSyntaxToken('<')]
        OpenAngleBrace,
        [OneCharSyntaxToken(':')]
        Colon,
        [OneCharSyntaxToken(';')]
        Semi,
        [OneCharSyntaxToken('=')]
        Equal,
        [OneCharSyntaxToken('-')]
        Minus,
        [OneCharSyntaxToken('+')]
        Plus,
        [OneCharSyntaxToken('/')]
        Divide,
        [OneCharSyntaxToken('%')]
        Mod,
    }
}
