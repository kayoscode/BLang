using BLang.Utils;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;

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
            }

            // TODO: theres more creiteria
            if (mAtEOF)
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
            }

            LogExitNonTerminal(eNonTerminal.File);
        }

        /// <summary>
        /// Identifier SubModule;
        /// SubModule ; | Identifier SubModule
        /// </summary>
        private void ImportStatement()
        {
            bool identifierFound = false;

            LogEnterNonTerminal(eNonTerminal.ImportStatement);
            AdvanceToken();

            while (mToken.Type == eTokenType.Identifier && !mAtEOF)
            {
                identifierFound = true;
                AdvanceToken();

                if (mToken.Code == eOneCharSyntaxToken.Period.Code())
                {
                    AdvanceToken();
                }
            }

            // There must be at least one identifier to qualify as a module.
            if (!identifierFound)
            {
                // AddError() and recover;
            }

            // An import statement must end with a semicolon.
            if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
            {
                AdvanceToken();
            }
            else
            {
                // AddError() and recover;
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
                }
            }
            else
            {
                // AddError() 
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
            LogExitNonTerminal(eNonTerminal.Function);
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
                    // TODO: definitely need to work out whether im advancing 
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
                }

                // Now it must end with a semicolon.
                if (mToken.Code == eOneCharSyntaxToken.Semi.Code())
                {
                    AdvanceToken();
                }
                else
                {
                    // AddError
                }
            }
            else
            {
                // LogError -> expected identifier.
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

            LogExitNonTerminal(eNonTerminal.OptionalType);
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
                }
            }

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
                // AddError
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
    }
}
