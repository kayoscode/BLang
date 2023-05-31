using BLang.Error;
using BLang.Utils;
using System.Diagnostics;

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

                while (IsModItem())
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
                        CodeBlock();
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

            if (mToken.Code == eTwoCharSyntaxToken.Arrow.Code())
            {
                AdvanceToken();
                // Single line code block.
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
            }
            else if (mToken.Code == eOneCharSyntaxToken.OpenBrace.Code())
            {
                // Multiline block ending with a close brace.
                AdvanceToken();

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
        /// Code that is converted to instructions in the text segment.
        /// List<Statement>
        /// </summary>
        private void StatementList()
        {
            //while ()
            LogEnterNonTerminal(eNonTerminal.StatementList);
            LogExitNonTerminal(eNonTerminal.StatementList);
        }

        private bool IsStatementToken()
        {
            return mToken.Code == eReserveWord.Let.Code() ||        // Varaible creation.
                   // Logic statement
                   mToken.Code == eReserveWord.If.Code() ||
                   // Loop statement
                   mToken.Code == eReserveWord.While.Code() ||
                   mToken.Code == eReserveWord.For.Code() ||
                   // Function call
                   mToken.Type == eTokenType.Identifier ||
                   // Return statement
                   mToken.Code == eReserveWord.Return.Code();
        }

        /// <summary>
        /// A single statement. Each one ends with a semicolon.
        /// Statement: VariableCreation | LogicalStatement | LoopStatement | FunctionCall | ReturnStatement
        /// </summary>
        private void Statement()
        {
            LogEnterNonTerminal(eNonTerminal.Statement);
            LogExitNonTerminal(eNonTerminal.Statement);
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
                    // AddError
                    Debugger.Break();
                }
            }
            else
            {
                if (mToken.Type == eTokenType.Identifier ||
                    mToken.Type == eTokenType.Integer ||
                    mToken.Type == eTokenType.FloatingPoint ||
                    mToken.Type == eTokenType.Char ||
                    mToken.Type == eTokenType.String)
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError();
                    Debugger.Break();
                }
            }

            LogExitNonTerminal(eNonTerminal.Expression);
        }

        #region Utilities 

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

        #endregion

        private ParserContext mParserContext = new();
        private Tokenizer mTokenizer;
        private Tokenizer.Token mToken;
        private bool mAtEOF = false;

        public ErrorLogger ErrorLogger => ParserContext.ErrorLogger;
    }
}
