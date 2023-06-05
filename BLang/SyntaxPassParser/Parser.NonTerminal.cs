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
            VariableInit,
            VariableDeclaration,
            VariableAssignment,
            OptionalType,
            RequiredType,
            Expression,
            OptionalCalleeParams,
            RequiredCalleeParams,
            RequiredCallerParams,
            CodeBlock,
            ExpressionCodeBlock,
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
