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

            File();

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
                ErrorLogger.LogError(new InvalidTokenAtFileLevel(mParserContext));
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

            ConsumeIdentifier();

            if (mToken.Code == eOneCharSyntaxToken.Period.Code())
            {
                ImportStatement();
            }
            else
            {
                ConsumeSemiColon();
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

            ConsumeIdentifier();
            ConsumeExpectedToken(eOneCharSyntaxToken.OpenBrace);

            while (ParserUtils.IsModItem(mToken) && !mIsEof)
            {
                ModItem();
            }

            ConsumeExpectedToken(eOneCharSyntaxToken.CloseBrace, new UnexpectedToken(mParserContext));

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
            else if(ParserUtils.IsFunctionStart(mToken))
            {
                Function();
            }
            else if (ParserUtils.IsVariableStart(mToken))
            {
                VariableCreation();
            }

            LogExitNonTerminal(eNonTerminal.ModItem);
        }
        
        private void Function()
        {
            LogEnterNonTerminal(eNonTerminal.Function);
            AdvanceToken();

            ConsumeIdentifier();

            if (mToken.Code == eOneCharSyntaxToken.Colon.Code())
            {
                OptionalType();
            }

            // Consume function params.
            ConsumeExpectedToken(eOneCharSyntaxToken.OpenPar);
            CalleeParams(false);
            ConsumeExpectedToken(eOneCharSyntaxToken.ClosePar);

            if (mToken.Code == eTwoCharSyntaxToken.Arrow.Code())
            {
                AdvanceToken();
                ConsumeExpression();
                ConsumeSemiColon();
            }
            else if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                CodeBlock();
            }
            else
            {
                ErrorLogger.LogError(new ExpectedFunctionBody(mParserContext));
            }

            LogExitNonTerminal(eNonTerminal.Function);
        }

        private void CodeBlock()
        {
            LogEnterNonTerminal(eNonTerminal.CodeBlock);

            ConsumeExpectedToken(eOneCharSyntaxToken.OpenBrace);
            StatementList();
            ConsumeExpectedToken(eOneCharSyntaxToken.CloseBrace);

            LogExitNonTerminal(eNonTerminal.CodeBlock);
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

        private void ForLoop()
        {
            LogEnterNonTerminal(eNonTerminal.ForLoop);
            AdvanceToken();

            ConsumeExpectedToken(eOneCharSyntaxToken.OpenPar);

            // For loop header body.
            if (ParserUtils.IsCodeStatementToken(mToken))
            {
                CodeStatement();
            }
            else
            {
                ErrorLogger.LogError(new InvalidForLoopStatement(mParserContext));
            }

            ConsumeExpression();
            ConsumeSemiColon();

            if (mToken.Code != eOneCharSyntaxToken.ClosePar.Code())
            {
                ConsumeExpression();
            }

            ConsumeExpectedToken(eOneCharSyntaxToken.ClosePar);

            if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                CodeBlock();
            }
            else
            {
                Statement();
            }

            LogExitNonTerminal(eNonTerminal.ForLoop);
        }

        private void WhileLoop()
        {
            LogEnterNonTerminal(eNonTerminal.WhileLoop);
            AdvanceToken();

            ConsumeExpectedToken(eOneCharSyntaxToken.OpenPar);
            ConsumeExpression();
            ConsumeExpectedToken(eOneCharSyntaxToken.ClosePar);

            if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                CodeBlock();
            }
            else
            {
                Statement();
            }

            LogExitNonTerminal(eNonTerminal.WhileLoop);
        }

        private void CalleeParams(bool required)
        {
            LogEnterNonTerminal(eNonTerminal.CalleeParams);

            if (required || mToken.Type == eTokenType.Identifier)
            {
                ConsumeIdentifier();

                RequiredType();

                if (mToken.Code == eOneCharSyntaxToken.Comma.Code())
                {
                    AdvanceToken();
                    CalleeParams(true);
                }
            }

            LogExitNonTerminal(eNonTerminal.CalleeParams);
        }

        private void CallerParams(bool required)
        {
            LogEnterNonTerminal(eNonTerminal.CallerParams);
            bool expressionFound = false;

            if (ParserUtils.IsExpressionStartToken(mToken))
            {
                ConsumeExpression();
                expressionFound = true;
            }
            else
            {
                if (required)
                {
                    ErrorLogger.LogError(new MissingExpression(mParserContext));
                }
            }

            if (expressionFound) 
            {
                if (mToken.Code == eOneCharSyntaxToken.Comma.Code())
                {
                    AdvanceToken();
                    CallerParams(true);
                }
            }

            LogExitNonTerminal(eNonTerminal.CallerParams);
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
                ConsumeExpression();
            }

            ConsumeSemiColon();

            LogExitNonTerminal(eNonTerminal.ReturnStatement);
        }

        private void IfStatement()
        {
            LogEnterNonTerminal(eNonTerminal.IfStatement);
            AdvanceToken();

            ConsumeExpectedToken(eOneCharSyntaxToken.OpenPar);
            ConsumeExpression();
            ConsumeExpectedToken(eOneCharSyntaxToken.ClosePar);

            if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code()) 
            { 
                CodeBlock();
            }
            else
            {
                Statement();
            }

            // Handle else if and else blocks.
            if (mToken.Code == eReserveWord.Else.Code())
            {
                AdvanceToken();
                if (mToken.Code == eReserveWord.If.Code())
                {
                    // Can handle this as an inline if statement within the codeblock, but its better for the parse
                    // tree to handle it here.
                    IfStatement();
                }
                else if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
                {
                    CodeBlock();
                }
                else
                {
                    Statement();
                }
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

            ConsumeIdentifier();

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

            ConsumeExpectedToken(eOneCharSyntaxToken.Equal);

            // Expect assignment after equals.
            ConsumeExpression();

            ConsumeSemiColon();

            LogExitNonTerminal(eNonTerminal.VariableCreation);
        }

        private void OptionalType()
        {
            AdvanceToken();
            ConsumeIdentifierOrType();
        }

        private void RequiredType()
        {
            ConsumeExpectedToken(eOneCharSyntaxToken.Colon);
            ConsumeIdentifierOrType();
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
                ErrorLogger.LogError(new UnexpectedToken(mParserContext));
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
                ConsumeExpression();
                ConsumeSemiColon();
            }
            else
            {
                // Asserting false here because we should never get here unless something is wrong in the code.
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

        private void ConsumeSemiColon(ParseError error = null)
        {
            if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingSemicolon(mParserContext);
                }

                ErrorLogger.LogError(error);
            }
        }

        private void ConsumeExpectedToken(eOneCharSyntaxToken token, ParseError error = null)
        {
            if (mToken.Code == token.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingSyntaxToken(mParserContext, token.AsLexeme());
                }

                // Add expected token error.
                ErrorLogger.LogError(error);
            }
        }

        private void ConsumeExpectedToken(eTwoCharSyntaxToken token, ParseError error = null)
        {
            if (mToken.Code == token.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingSyntaxToken(mParserContext, token.AsLexeme());
                }

                // Add expected token error.
                ErrorLogger.LogError(error);
            }
        }

        private void ConsumeExpectedToken(eThreeCharSyntaxToken token, ParseError error = null)
        {
            if (mToken.Code == token.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingSyntaxToken(mParserContext, token.AsLexeme());
                }

                // Add expected token error.
                ErrorLogger.LogError(error);
            }
        }

        private void ConsumeIdentifier(ParseError error = null)
        {
            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingIdentifier(mParserContext);
                }

                ErrorLogger.LogError(error);
            }
        }

        private void ConsumeIdentifierOrType(ParseError error = null)
        {
            if (mToken.Type == eTokenType.Identifier ||
                mToken.Type == eTokenType.Type)
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingTypeSpecifier(mParserContext);
                }

                ErrorLogger.LogError(error);
            }
        }

        private void ConsumeExpression(ParseError error = null)
        {
            if (ParserUtils.IsExpressionStartToken(mToken))
            {
                Expression();
            }
            else
            {
                if (error == null)
                {
                    error = new MissingExpression(mParserContext);
                }

                ErrorLogger.LogError(error);
            }
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
