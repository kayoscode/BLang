namespace BLang
{
    public enum eReserveWord
    {
        #region Access modifiers

        [ReserveWord("public")]
        Export,
        [ReserveWord("import")]
        Import,

        #endregion

        [ReserveWord("let")]
        Let,
        [ReserveWord("true")]
        True,
        [ReserveWord("false")]
        False,
        [ReserveWord("const")]
        Const,
        [ReserveWord("return")]
        Return,
        [ReserveWord("sizeof")]
        Sizeof,
        [ReserveWord("entrypt")]
        EntryPoint,
        [ReserveWord("fn")]
        Function,
        [ReserveWord("mod")]
        Module,

        #region Control flow

        [ReserveWord("break")]
        Break,
        [ReserveWord("continue")]
        Continue,
        [ReserveWord("if")]
        If,
        [ReserveWord("else")]
        Else,
        [ReserveWord("for")]
        For,
        [ReserveWord("while")]
        While,

        #endregion
    }
}
