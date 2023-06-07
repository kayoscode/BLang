using BLang.Error;
using BLang.Utils;
using System.Diagnostics;

namespace BLang
{
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

            if (mIsEof && 
                ErrorLogger.ErrorCount == 0)
            {
                Console.WriteLine("File parsed to completion.");
            }
        }

        /// <summary>
        /// File: List<ImportStatement> Module
        /// </summary>
        private void File()
        {
            LogEnterNonTerminal(eNonTerminal.File);

            // Load the import statements. They can only appear at the top.
            while (mToken.Code == eReserveWord.Import.Code() && !mIsEof)
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

                while (ParserUtils.IsModItem(mToken) && !mIsEof)
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

            LogExitNonTerminal(eNonTerminal.CodeBlock);
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

            while (ParserUtils.IsStatementToken(mToken) && !mIsEof)
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
                // AddError();
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.FunctionCall);
        }

        private void ArrayIndex()
        {
            LogEnterNonTerminal(eNonTerminal.ArrayIndex);
            AdvanceToken();

            if (ParserUtils.IsExpressionStartToken(mToken))
            {
                Expression();

                if (mToken.Code == eOneCharSyntaxToken.CloseBrack.Code())
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
                // AddError
                Debugger.Break();
            }

            LogExitNonTerminal(eNonTerminal.ArrayIndex);
        }

        private void ForLoop()
        {
            LogEnterNonTerminal(eNonTerminal.ForLoop);
            AdvanceToken();

            if (mToken.Code == eOneCharSyntaxToken.OpenPar.Code())
            {
                AdvanceToken();

                // For loop header body.
                if (ParserUtils.IsCodeStatementToken(mToken))
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

            LogExitNonTerminal(eNonTerminal.RequiredCallerParams);
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
            else if (ParserUtils.IsCodeStatementToken(mToken))
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
            else if (ParserUtils.IsExpressionStartToken(mToken))
            {
                Expression();

                if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
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
                Trace.Assert(false);
            }

            LogExitNonTerminal(eNonTerminal.CodeStatement);
        }

        private void AdvanceToken()
        {
            mIsEof = !mTokenizer.NextToken();
            mParserContext.AddToken(mToken, mCurrentContext);

#if (DEBUG)
            if (!mIsEof)
            {
                if (AppSettings.LogParserDetails)
                {
                    mToken.PrintTokenShort();
                }
            }
#endif
        }

        private void LogEnterNonTerminal(eNonTerminal nt)
        {
            mContext.Push(nt);
            mCurrentContext = nt;

#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                Console.WriteLine($"Entering {nt}");
            }
#endif
        }

        private void LogExitNonTerminal(eNonTerminal nt)
        {
            Trace.Assert(mContext.Count > 0);
            Trace.Assert(nt == mContext.Peek());
            mCurrentContext = mContext.Pop();

#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                Console.WriteLine($"Exiting {nt}");
            }
#endif
        }

        public ParserContext ParserContext => mParserContext;
        private ParserContext mParserContext = new();
        private Tokenizer mTokenizer;
        private Tokenizer.Token mToken;
        private bool mIsEof = false;

        private Stack<eNonTerminal> mContext = new();
        private eNonTerminal mCurrentContext = eNonTerminal.File;

        public ErrorLogger ErrorLogger => ParserContext.ErrorLogger;
    }
}
