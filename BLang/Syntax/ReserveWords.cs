namespace BLang
{
    public enum eReserveWord
    {
        #region Access modifiers

        [ReserveWord("export")]
        Export,
        [ReserveWord("import")]
        Import,

        #endregion

        [ReserveWord("var")]
        Var,
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
