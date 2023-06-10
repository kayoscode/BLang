namespace BLang
{
    public partial class Parser
    {
        public enum eNonTerminal
        {
            File,
            Module,
            ModItem,
            ImportStatement,
            Function,
            VariableCreation,
            Expression,
            CalleeParams,
            CallerParams,
            CodeBlock,
            StatementList,
            Statement,
            IfStatement,
            IfExpression,
            ReturnStatement,
            FunctionCall,
            WhileLoop,
            ForLoop,
            CodeStatement,
            ArrayIndex
        }
    }
}
