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
            // The tokenizer needs to store the parser context.
            // and the parser context needs to store the tokenizer.
            mParserContext = new();
            mTokenizer = mParserContext.Tokenizer;
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

            if (IsEof && 
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
            LogEnterContext(eParserContext.File);

            // Load the import statements. They can only appear at the top.
            while (mToken.Code == eReserveWord.Import.Code() && !IsEof)
            {
                ImportStatement();
            }

            while (mToken.Type != eTokenType.EndOfStream)
            {
                if (mToken.Code == eReserveWord.Module.Code())
                {
                    Module();
                }
                else
                {
                    ErrorLogger.LogError(new UnexpectedTokenAtFileLevel(mParserContext));
                }
            }

            LogExitContext(eParserContext.File);
        }

        /// <summary>
        /// Identifier SubModule;
        /// SubModule ; | Identifier SubModule
        /// </summary>
        private void ImportStatement()
        {
            LogEnterContext(eParserContext.ImportStatement);
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

            LogExitContext(eParserContext.ImportStatement);
        }

        /// <summary>
        /// Module: { List<ModBlockItem> }
        /// </summary>
        private void Module()
        {
            LogEnterContext(eParserContext.Module);
            AdvanceToken();

            ConsumeIdentifier();
            ConsumeExpectedToken(eOneCharSyntaxToken.OpenBrace);

            while (mToken.Code != eOneCharSyntaxToken.CloseBrace.Code() && !IsEof)
            {
                if (ParserUtils.IsModItem(mToken))
                {
                    ModItem();
                }
                else
                {
                    ErrorLogger.LogError(new UnexpectedToken(mParserContext));
                }
            }

            ConsumeExpectedToken(eOneCharSyntaxToken.CloseBrace, eParseError.UnexpectedToken);

            LogExitContext(eParserContext.Module);
        }

        /// <summary>
        /// ModBlockItem: ModBlock | Function | DataStructure | VariableCreation
        /// </summary>
        private void ModItem()
        {
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
        }
        
        private void Function()
        {
            LogEnterContext(eParserContext.FunctionDefinition);
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

            CodeBlock();

            LogExitContext(eParserContext.FunctionDefinition);
        }

        private void CodeBlock()
        {
            LogEnterContext(eParserContext.CodeBlock);

            ConsumeExpectedToken(eOneCharSyntaxToken.OpenBrace);
            StatementList();
            ConsumeExpectedToken(eOneCharSyntaxToken.CloseBrace);

            LogExitContext(eParserContext.CodeBlock);
        }

        /// <summary>
        /// Code that is converted to instructions in the text segment.
        /// List<Statement>
        /// </summary>
        private void StatementList()
        {
            while (mToken.Code != eOneCharSyntaxToken.CloseBrace.Code() && !IsEof)
            {
                if (ParserUtils.IsStatementToken(mToken))
                {
                    Statement();
                }
                else
                {
                    ErrorLogger.LogError(new UnexpectedToken(mParserContext));
                }
            }

            while (ParserUtils.IsStatementToken(mToken) && !IsEof)
            {
                Statement();
            }
        }

        private void ForLoop()
        {
            LogEnterContext(eParserContext.ForLoop);
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

            LogExitContext(eParserContext.ForLoop);
        }

        private void WhileLoop()
        {
            LogEnterContext(eParserContext.BlockStatement);
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

            LogExitContext(eParserContext.BlockStatement);
        }

        private void CalleeParams(bool required)
        {
            LogEnterContext(eParserContext.CalleeParams);

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

            LogExitContext(eParserContext.CalleeParams);
        }

        private void CallerParams(bool required)
        {
            LogEnterContext(eParserContext.CallerParams);
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

            LogExitContext(eParserContext.CallerParams);
        }

        /// <summary>
        /// Simply return Expression ;
        /// </summary>
        private void ReturnStatement()
        {
            AdvanceToken();

            if (mToken.Code != eOneCharSyntaxToken.Semi.Code())
            {
                ConsumeExpression();
            }

            ConsumeSemiColon();
        }

        private void IfStatement()
        {
            LogEnterContext(eParserContext.BlockStatement);

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

            LogExitContext(eParserContext.BlockStatement);
        }

        /// <summary>
        /// VariableCreation: VariableAssignment | VariableDeclaration
        /// VariableAssignment: let Identifier : OptionalType = Expression;
        /// VariableDeclaration: let Identifier : Type;
        /// </summary>
        private void VariableCreation()
        {
            LogEnterContext(eParserContext.VariableCreation);
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

                    LogExitContext(eParserContext.VariableCreation);
                    return;
                }
            }

            // If we have a missing initializer, expect there will be a semi colon.
            if (ConsumeExpectedToken(eOneCharSyntaxToken.Equal, eParseError.MissingInitializer))
            {
                // Expect assignment after equals.
                ConsumeExpression();
            }

            ConsumeSemiColon();

            LogExitContext(eParserContext.VariableCreation);
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
            LogEnterContext(eParserContext.Statement);

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

            LogExitContext(eParserContext.Statement);
        }

        /// <summary>
        /// Only assign, call, increement, decrement statements allowed.
        /// </summary>
        private void CodeStatement()
        {
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
        }

        private void AdvanceToken()
        {
            mTokenizer.NextToken();
            mParserContext.AddToken(mToken, mCurrentContext);

#if (DEBUG)
            if (!IsEof)
            {
                if (AppSettings.LogParserDetails)
                {
                    mToken.PrintTokenShort();
                }
            }
#endif
        }

        private bool ConsumeSemiColon(Enum error = null)
        {
            if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(eParseError.MissingSemicolon, mParserContext));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(error, mParserContext, eOneCharSyntaxToken.Semi));
                }

                return false;
            }

            return true;
        }

        private bool ConsumeExpectedToken(eOneCharSyntaxToken token, Enum error = null)
        {
            if (mToken.Code == token.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(
                        eParseError.MissingSyntaxToken, mParserContext, token));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(
                        error, mParserContext, token));
                }

                return false;
            }

            return true;
        }

        private bool ConsumeExpectedToken(eTwoCharSyntaxToken token, Enum error = null)
        {
            if (mToken.Code == token.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(
                        eParseError.MissingSyntaxToken, mParserContext, token));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(
                        error, mParserContext, token));
                }

                return false;
            }

            return true;
        }

        private bool ConsumeExpectedToken(eThreeCharSyntaxToken token, Enum error = null)
        {
            if (mToken.Code == token.Code())
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(
                        eParseError.MissingSyntaxToken, mParserContext, token));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(
                        error, mParserContext, token));
                }

                return false;
            }

            return true;
        }

        private bool ConsumeIdentifier(Enum error = null)
        {
            if (mToken.Type == eTokenType.Identifier)
            {
                AdvanceToken();
            }
            else
            {
                if (error == null)
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(eParseError.MissingIdentifier, mParserContext));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(error, mParserContext));
                }

                return false;
            }

            return true;
        }

        private bool ConsumeIdentifierOrType(Enum error = null)
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
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(eParseError.MissingTypeSpecifier, mParserContext));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(error, mParserContext));
                }

                return false;
            }

            return true;
        }

        private bool ConsumeExpression(Enum error = null)
        {
            if (ParserUtils.IsExpressionStartToken(mToken))
            {
                Expression();
            }
            else
            {
                if (error == null)
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(eParseError.MissingExpression, mParserContext));
                }
                else
                {
                    ErrorLogger.LogError(ParseErrorFactory.CreateError(error, mParserContext));
                }

                return false;
            }

            return true;
        }

        private void LogEnterContext(eParserContext nt)
        {
            ParserContext.Context.Push(nt);
            mCurrentContext = nt;

#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                Console.WriteLine($"Entering {nt}");
            }
#endif
        }

        private void LogExitContext(eParserContext nt)
        {
            Trace.Assert(ParserContext.Context.Count > 0);
            Trace.Assert(nt == ParserContext.CurrentContext);
            mCurrentContext = ParserContext.Context.Pop();

#if (DEBUG)
            if (AppSettings.LogParserDetails)
            {
                Console.WriteLine($"Exiting {nt}");
            }
#endif
        }

        public ParserContext ParserContext => mParserContext;
        private ParserContext mParserContext;
        private Tokenizer mTokenizer;
        private Tokenizer.Token mToken;
        private bool IsEof => mToken.Type == eTokenType.EndOfStream;

        private eParserContext mCurrentContext = eParserContext.File;

        public ErrorLogger ErrorLogger => ParserContext.ErrorLogger;
    }
}
