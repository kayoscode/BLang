using BLang.Error;
using BLang.Utils;
using System.Diagnostics;
using System.Numerics;

namespace BLang
{
    /// <summary>
    /// File structure:
    /// 
    /// Each file must start with import statements pointing to each of the modules they want to import.
    /// Then it must have the keyword "mod" then open and closing braces to show whats actually inside the module
    /// 
    /// By default, everything inside of a module is private to that module, but functions and variables inside
    /// can include the keyword (or block) "public" which makes them accessible from anywhere.
    /// 
    /// Identifier: [_A-z]+[_A-z0-9]*
    /// Number: Any number token
    /// StringLiteral: "...text..."
    /// Value: Identifier | Number | StringLiteral
    /// OptionalType: /**empty*/ | : Type
    /// Type: i8 | i16 | i32 | i64 | f32 | f64 | bool | char | Identifier
    /// 
    /// VariableCreation: VariableAssignment | VariableDeclaration
    /// VariableAssignment: let Identifier OptionalType = Expression;
    /// VariableDeclaration: let Identifier : Type;
    /// 
    /// ModBlock: mod { List<ModBlockItem> } 
    /// ModBlockItem: ModBlock | Function | DataStructure | VariableCreation
    /// 
    /// Function: fn Identifier OptionalReturn ( CalleeParamList ) Block
    /// Block: -> Statement | { List<Statement> }
    /// CalleeParamList: CommaSeparatedList<CalleeParam>
    /// CalleeParam: Identifier: Type
    /// 
    /// Statement: VariableCreation | LogicalStatement | LoopStatement | FunctionCall | ReturnStatement
    /// 
    /// LogicalStatement: if Expression Block OptionalElseBlock
    /// OptionalElseBlock else LogicalStatement | else Block
    /// 
    /// FunctionCall: Identifier ( CallerParamList ) ;
    /// ReturnStatement: return Expression ;
    /// CallerParamList: CommaSeparatedList<CallerParam>
    /// CallerParam: Expression
    /// 
    /// </summary>
    public partial class Parser
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public Parser()
        {
            mTokenizer = new(mParserContext);
            mToken = mParserContext.Token;
        }

        /// <summary>
        /// Parses the text in the given input stream into the language AST.
        /// </summary>
        /// <param name="inputStream"></param>
        public void ParseFile(StreamReader inputStream)
        {
            mTokenizer.SetStream(inputStream);
            AdvanceToken();

            if (mToken.Code == eReserveWord.Import.Code() || mToken.Code == eReserveWord.Module.Code())
            {
                File();
            }
            else
            {
                // AddError() fatal
                Debugger.Break();
            }

            if (mAtEOF && 
                ErrorLogger.ErrorCount == 0)
            {
                Console.WriteLine("File parsed to completion");
            }
        }

        /// <summary>
        /// File: List<ImportStatement> Module
        /// </summary>
        private void File()
        {
            LogEnterNonTerminal(eNonTerminal.File);

            // Load the import statements. They can only appear at the top.
            while (mToken.Code == eReserveWord.Import.Code() && !mAtEOF)
            {
                ImportStatement();
            }

            if (mToken.Code == eReserveWord.Module.Code())
            {
                Module();
            }
            else
            {
                // AddError() fatal
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.File);
        }

        /// <summary>
        /// Identifier SubModule;
        /// SubModule ; | Identifier SubModule
        /// </summary>
        private void ImportStatement()
        {
            LogEnterNonTerminal(eNonTerminal.ImportStatement);
            AdvanceToken();

            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();

                if (mToken.Code == eOneCharSyntaxToken.Period.Code())
                {
                    ImportStatement();
                }
                else if(mToken.Code == eOneCharSyntaxToken.Semi.Code())
                {
                    AdvanceToken();
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

            LogExitNonTerminal(eNonTerminal.ImportStatement);
        }

        /// <summary>
        /// Module: { List<ModBlockItem> }
        /// </summary>
        private void Module()
        {
            LogEnterNonTerminal(eNonTerminal.Module);
            AdvanceToken();

            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();
            }
            else
            {
                // AddError (modules must have names)
                Debugger.Break();
            }

            if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                AdvanceToken();

                while (IsModItem() && !mAtEOF)
                {
                    ModItem();
                }

                if (mToken.Code == eOneCharSyntaxToken.CloseBrace.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError()
                    Debugger.Break();
                }
            }
            else
            {
                // AddError() 
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.Module);
        }

        /// <summary>
        /// ModBlockItem: ModBlock | Function | DataStructure | VariableCreation
        /// </summary>
        private void ModItem()
        {
            LogEnterNonTerminal(eNonTerminal.ModItem);

            if (mToken.Code == eReserveWord.Module.Code())
            {
                Module();
            }
            else if(mToken.Code == eReserveWord.Function.Code())
            {
                Function();
            }
            else if (mToken.Code == eReserveWord.Let.Code())
            {
                VariableCreation();
            }

            LogExitNonTerminal(eNonTerminal.ModItem);
        }
        
        private void Function()
        {
            LogEnterNonTerminal(eNonTerminal.Function);
            AdvanceToken();

            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();
                if (mToken.Code == eOneCharSyntaxToken.Colon.Code())
                {
                    OptionalType();
                }

                if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
                {
                    AdvanceToken();
                    CalleeParams(false);

                    if (mToken.Code == eOneCharSyntaxToken.ClosePar.Code())
                    {
                        AdvanceToken();

                        if (mToken.Code == eTwoCharSyntaxToken.Arrow.Code())
                        {
                            ExpressionCodeBlock();
                            
                            if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
                            {
                                AdvanceToken();
                            }
                            else
                            {
                                // AddError();
                                Debugger.Break();
                            }
                        }
                        else if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
                        {
                            CodeBlock();
                        }
                        else
                        {
                            // AddError.
                            Debugger.Break();
                        }
                    }
                    else {
                        // AddError();
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

            LogExitNonTerminal(eNonTerminal.Function);
        }

        private void CodeBlock()
        {
            LogEnterNonTerminal(eNonTerminal.CodeBlock);

            if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                // Multiline block ending with a close brace.
                AdvanceToken();
                StatementList();

                if (mToken.Code == eOneCharSyntaxToken.CloseBrace.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }
            }
            else
            {
                // AddError()
                Debugger.Break();
            }

            LogEnterNonTerminal(eNonTerminal.CodeBlock);
        }

        /// <summary>
        /// A codeblock that starts with an arror and expects one expression which
        /// acts as the return value. A semi colon is not forced at this juncture.
        /// </summary>
        private void ExpressionCodeBlock()
        {
            LogEnterNonTerminal(eNonTerminal.ExpressionCodeBlock);

            AdvanceToken();
            // Single line code block.
            Expression();

            LogExitNonTerminal(eNonTerminal.ExpressionCodeBlock);
        }

        /// <summary>
        /// Code that is converted to instructions in the text segment.
        /// List<Statement>
        /// </summary>
        private void StatementList()
        {
            LogEnterNonTerminal(eNonTerminal.StatementList);

            while (IsStatementToken() && !mAtEOF)
            {
                Statement();
            }

            LogExitNonTerminal(eNonTerminal.StatementList);
        }

        private void FunctionCall()
        {
            LogEnterNonTerminal(eNonTerminal.FunctionCall);
            AdvanceToken();

            // Consume the parameters if they exist.
            if (mToken.Code != eOneCharSyntaxToken.ClosePar.Code())
            {
                CallerParams();
            }

            // Now we definitely have to be a close paren.
            if (mToken.Code == eOneCharSyntaxToken.ClosePar.Code())
            {
                AdvanceToken();
            }
            else
            {
                Debugger.Break();
                // AddError();
            }

            LogExitNonTerminal(eNonTerminal.FunctionCall);
        }

        private void VariableAssignment()
        {
            LogEnterNonTerminal(eNonTerminal.VariableAssignment);
            AdvanceToken();

            Expression();

            // Now it must end with a semicolon.
            if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
            {
                AdvanceToken();
            }
            else
            {
                // AddError
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.VariableAssignment);
        }

        private void ForLoop()
        {
            LogEnterNonTerminal(eNonTerminal.ForLoop);
            AdvanceToken();

            if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                AdvanceToken();

                // For loop header body.
                if (IsCodeStatementToken())
                {
                    CodeStatement();
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }

                Expression();

                if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }

                if (mToken.Code != eOneCharSyntaxToken.ClosePar.Code())
                {
                    Expression();
                }

                // Expect closed parn.
                if (mToken.Code == eOneCharSyntaxToken.ClosePar.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError
                    Debugger.Break();
                }
            }
            else
            {
                // AddError()
                Debugger.Break();
            }

            if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                CodeBlock();
            }
            else
            {
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.ForLoop);
        }

        private void WhileLoop()
        {
            LogEnterNonTerminal(eNonTerminal.WhileLoop);
            AdvanceToken();

            if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                AdvanceToken();

                // Semantics needs to force this to be a bool.
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

                if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
                {
                    CodeBlock();
                }
                else
                {
                    // AddError
                    Debugger.Break();
                }
            }
            else
            {
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.WhileLoop);
        }

        private void CalleeParams(bool required)
        {
            LogEnterNonTerminal(required? eNonTerminal.RequiredCalleeParams : eNonTerminal.OptionalCalleeParams);

            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();
                RequiredType();

                if (mToken.Code == eOneCharSyntaxToken.Comma.Code())
                {
                    AdvanceToken();
                    CalleeParams(true);
                }
            }
            else if (required)
            {
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(required? eNonTerminal.RequiredCalleeParams : eNonTerminal.OptionalCalleeParams);
        }

        private void CallerParams()
        {
            LogEnterNonTerminal(eNonTerminal.RequiredCallerParams);
            Expression();

            if (mToken.Code == eOneCharSyntaxToken.Comma.Code())
            {
                AdvanceToken();
                CallerParams();
            }

            LogEnterNonTerminal(eNonTerminal.RequiredCallerParams);
        }

        /// <summary>
        /// Simply return Expression ;
        /// </summary>
        private void ReturnStatement()
        {
            LogEnterNonTerminal(eNonTerminal.ReturnStatement);
            AdvanceToken();

            if (mToken.Code != eOneCharSyntaxToken.Semi.Code())
            {
                Expression();
            }

            if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
            {
                AdvanceToken();
            }
            else
            {
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.ReturnStatement);
        }

        private void IfStatement()
        {
            LogEnterNonTerminal(eNonTerminal.IfStatement);
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

                if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code()) 
                { 
                    CodeBlock();

                    // Handle else if and else blocks.
                    if (mToken.Code == eReserveWord.Else.Code())
                    {
                        AdvanceToken();
                        if (mToken.Code == eReserveWord.If.Code())
                        {
                            IfStatement();
                        }
                        else if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
                        {
                            CodeBlock();
                        }
                        else
                        {
                            // AddError();
                            Debugger.Break();
                        }
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

            LogExitNonTerminal(eNonTerminal.IfStatement);
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

        /// <summary>
        /// VariableCreation: VariableAssignment | VariableDeclaration
        /// VariableAssignment: let Identifier : OptionalType = Expression;
        /// VariableDeclaration: let Identifier : Type;
        /// </summary>
        private void VariableCreation()
        {
            LogEnterNonTerminal(eNonTerminal.VariableCreation);
            AdvanceToken();

            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();

                if (mToken.Code == eOneCharSyntaxToken.Colon.Code())
                {
                    // the token before or after calling the method.
                    OptionalType();

                    // If they specified a type, then it can end here, other wise they have to use an assignment.
                    if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
                    {
                        AdvanceToken();

                        LogExitNonTerminal(eNonTerminal.VariableCreation);
                        return;
                    }
                }

                // Since they didn't specify a type, or didn't end with a semicolon, they will need an assignment.
                if (mToken.Code == eOneCharSyntaxToken.Equal.Code())
                {
                    AdvanceToken();
                    Expression();
                }
                else
                {
                    // AddError -> theyre gonna need to specify an assignment.
                    Debugger.Break();
                }

                // Now it must end with a semicolon.
                if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError
                    Debugger.Break();
                }
            }
            else
            {
                // LogError -> expected identifier.
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.VariableCreation);
        }

        private void OptionalType()
        {
            LogEnterNonTerminal(eNonTerminal.OptionalType);
            AdvanceToken();

            if (mToken.Type == eTokenType.Type)
            {
                AdvanceToken();
            }
            else
            {
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.OptionalType);
        }

        private void RequiredType()
        {
            LogEnterNonTerminal(eNonTerminal.RequiredType);

            if (mToken.Code == eOneCharSyntaxToken.Colon.Code())
            {
                AdvanceToken();
                if (mToken.Type == eTokenType.Type)
                {
                    AdvanceToken();
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

            LogExitNonTerminal(eNonTerminal.RequiredType);
        }

        /// <summary>
        /// Just an identifier, number, float, char, or string for now.
        /// </summary>
        private void Expression()
        {
            LogEnterNonTerminal(eNonTerminal.Expression);

            // Pre inc. ++Expression. Semantcs will enforce that the expression resolves to a modifiable var.
            if (IsExpressionPrefix())
            {
                AdvanceToken();
                Expression();
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
                    // AddError
                    Debugger.Break();
                }
            }
            // Here we have an if statement that returns something.
            // Semantics  will enforce that there is an else clause.
            else if (mToken.Code == eReserveWord.If.Code())
            {
                IfExpression();
            }
            else if (IsConstant())
            {
                AdvanceToken();
            }
            else if (mToken.Type == eTokenType.Identifier)
            {
                // Can be a variable or a function call.
                AdvanceToken();

                // Handle function call if its there.
                if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
                {
                    FunctionCall();
                }
            }
            else
            {
                // AddError();
                Debugger.Break();
            }

            // Handle post increment.
            if (IsIncDec())
            {
                AdvanceToken();
            }

            LogExitNonTerminal(eNonTerminal.Expression);
        }

        /// <summary>
        /// A single statement. Each one ends with a semicolon.
        /// Statement: VariableCreation | LogicalStatement | LoopStatement | FunctionCall | ReturnStatement
        /// </summary>
        private void Statement()
        {
            LogEnterNonTerminal(eNonTerminal.Statement);

            if (mToken.Code == eReserveWord.If.Code())
            {
                IfStatement();
            }
            else if(mToken.Code == eReserveWord.While.Code())
            {
                WhileLoop();
            }
            else if (mToken.Code == eReserveWord.For.Code())
            {
                ForLoop();
            }
            else if (mToken.Code == eReserveWord.Return.Code())
            {
                ReturnStatement();
            }
            else if (IsCodeStatementToken())
            {
                CodeStatement();
            }
            else
            {
                Trace.Assert(false);
            }

            LogExitNonTerminal(eNonTerminal.Statement);
        }

        /// <summary>
        /// Only assign, call, increement, decrement statements allowed.
        /// </summary>
        private void CodeStatement()
        {
            LogEnterNonTerminal(eNonTerminal.CodeStatement);

            if (mToken.Code == eReserveWord.Let.Code())
            {
                VariableCreation();
            }
            else if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
            {
                // Empty line.
                AdvanceToken();
            }
            else if (IsExpressionStartToken())
            {
                Expression();
            }
            else
            {
                Trace.Assert(false);
            }

            LogExitNonTerminal(eNonTerminal.CodeStatement);
        }

        private bool IsLogical()
        {
            return mToken.Code == eTwoCharSyntaxToken.AndAnd.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.OrOr.Code();
        }

        private bool IsCompare()
        {
            return mToken.Code == eOneCharSyntaxToken.CloseAngleBrace.Code() ||
                   mToken.Code == eOneCharSyntaxToken.CloseAngleBrace.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.Gte.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.Lte.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.AreEqual.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.NotEqual.Code();
        }

        private bool IsAddOp()
        {
            return mToken.Code == eOneCharSyntaxToken.Plus.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Minus.Code() ||
                   mToken.Code == eOneCharSyntaxToken.And.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Or.Code();
        }

        private bool IsMul()
        {
            return mToken.Code == eOneCharSyntaxToken.Star.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Divide.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Mod.Code() ||
                   mToken.Code == eOneCharSyntaxToken.Xor.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.LogicalShiftLeft.Code() ||
                   mToken.Code == eTwoCharSyntaxToken.LogicalShiftRight.Code();
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

        /// <summary>
        /// Whether the token represents a constant.
        /// </summary>
        /// <returns></returns>
        private bool IsConstant()
        {
            return mToken.Type == eTokenType.Integer ||
                   mToken.Type == eTokenType.FloatingPoint ||
                   mToken.Type == eTokenType.Char ||
                   mToken.Type == eTokenType.String ||
                   mToken.Code == eReserveWord.True.Code() ||
                   mToken.Code == eReserveWord.False.Code();
            }


        /// <summary>
        /// The set of all tokens that can be used in a statement as part of a function body.
        /// </summary>
        /// <returns></returns>
        private bool IsStatementToken()
        {
            return 
                   // Logic statement
                   mToken.Code == eReserveWord.If.Code() ||
                   // Loop statement
                   mToken.Code == eReserveWord.While.Code() ||
                   mToken.Code == eReserveWord.For.Code() ||
                   // Function call
                   mToken.Code == eReserveWord.Return.Code() ||
                   IsCodeStatementToken();
        }

        /// <summary>
        /// The subset of statements that can be used as an individual command.
        /// </summary>
        /// <returns></returns>
        private bool IsCodeStatementToken()
        {
            return mToken.Code == eReserveWord.Let.Code() ||        // Varaible creation.
                   // Possible expression.
                   IsExpressionStartToken() ||

                   // Empty statement.
                   mToken.Code == eOneCharSyntaxToken.Semi.Code();
        }

        /// <summary>
        /// Is the token an item that belongs inside of a module definition.
        /// </summary>
        /// <returns></returns>
        private bool IsModItem()
        {
            return (mToken.Code == eReserveWord.Module.Code() ||
                    mToken.Code == eReserveWord.Function.Code() ||
                    //mToken.Code == eReserveWord.) DATA STRUCTURES
                    mToken.Code == eReserveWord.Let.Code());
        }

        private void AdvanceToken()
        {
            mAtEOF = !mTokenizer.NextToken();

#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                mToken.PrintTokenShort();
            }
#endif
        }

        private void LogEnterNonTerminal(eNonTerminal nt)
        {
#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                Console.WriteLine($"Entering {nt}");
            }
#endif
        }

        private void LogExitNonTerminal(eNonTerminal nt)
        {
#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                Console.WriteLine($"Exiting {nt}");
            }
#endif
        }

        private ParserContext mParserContext = new();
        private Tokenizer mTokenizer;
        private Tokenizer.Token mToken;
        private bool mAtEOF = false;

        public ErrorLogger ErrorLogger => ParserContext.ErrorLogger;
    }
}
