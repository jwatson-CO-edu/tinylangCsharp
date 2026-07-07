
namespace tlCsharp{

public class Compiler( bool shouldLog_ = true ) {

    /// <summary>
    /// Local lookup of named address slots, Can be a function body
    /// </summary>
    protected struct LocalContext{
        public Dictionary<string,int> locals;
        public int /*--------------*/ nextLocalSlot;
        public bool /*-------------*/ isFunctionBody;

        public LocalContext(){  locals = [];  nextLocalSlot = 0;  isFunctionBody = false;  }

        /// <summary>
        /// Return the currently avaialble slot number and increment slot number
        /// </summary>
        public int GetNextSlot(){
            int rtn = nextLocalSlot;
            nextLocalSlot += 1;
            return rtn;
        }
    };


    /// <summary>
    /// List of named parameters and the address of a stack frame
    /// </summary>
    protected struct FunctionSignature{
        public List<string> parameters;
        public int /*----*/ address;

        public FunctionSignature(){  parameters = [];  address = -1;  }
    }


    /// <summary>
    /// Function name, Instruction index, and number of args
    /// </summary>
    protected struct PendingFunctionCall{
        public string name;
        public int    instructionIndex; 
        public int    arity;
    }

    
    protected bool /*---------------------------*/ shouldLog /*------*/ = shouldLog_;
    protected Dictionary<string,FunctionSignature> functionSignatures   = [];
    protected List<PendingFunctionCall> /*------*/ pendingFunctionCalls = [];
    protected int /*----------------------------*/ nextUniqueNumber     = 1;


    /// <summary>
    /// If logging is enabled, Then print the text to the console
    /// </summary>
    public void Log( string stmt ) {  if( shouldLog ){  Console.WriteLine( stmt );  }  }


    /// <summary>
    /// `Token` --to-> `Instruction` lookup
    /// </summary>
    protected static Instruction InstructionForOperator( Token op ) {
        return op.type switch{ // `swtich` EXPRESSION, NOT statement
            TokenType.PLUS /*---*/ => new Instruction.Add(),
            TokenType.MINUS /*--*/ => new Instruction.Sub(),
            TokenType.STAR /*---*/ => new Instruction.Mul(),
            TokenType.SLASH /*--*/ => new Instruction.Div(),
            TokenType.LESS_THAN    => new Instruction.LessThan(),
            TokenType.GREATER_THAN => new Instruction.GreaterThan(),
            _ /*----------------*/ => throw new InvalidOperationException( $"Invalid operator {op}" )
        };
    }


    /// <summary>
    /// For each pending function call, Create a function call instruction
    /// </summary>
    protected void PatchFunctionCalls( List<Instruction> instructions ) {   
        foreach( PendingFunctionCall pendingCall in pendingFunctionCalls ){
            if( !functionSignatures.TryGetValue( pendingCall.name, out FunctionSignature signature ) )
                throw new InvalidOperationException( "Unreachable" );
            instructions[ pendingCall.instructionIndex ] = new Instruction.CallFunction( signature.address, pendingCall.arity );
        }
    }


    /// <summary>
    /// Assign `address` to function `name`
    /// </summary>
    protected void SetFunctionAddress( string name, int address ) {
        if( !functionSignatures.TryGetValue( name, out FunctionSignature signature ) )
                throw new InvalidOperationException( "shouldnt be possible" );
        signature.address = address;
    }


    /// <summary>
    /// Return a unique identifier, and increment the identifier
    /// </summary>
    protected int GetNextUnique(){
        int rtn = nextUniqueNumber;
        nextUniqueNumber += 1;
        return rtn;
    }


    /// <summary>
    /// Interpret the expression as `Instruction`s, Add the instruction(s) to the list of instructions
    /// </summary>
    protected void Emit( Expr expr, List<Instruction> instructions, LocalContext context ) {
        int logId = GetNextUnique();

        // Number Literal: Push an int onto the stack
        if( expr is Expr.NumberLiteralCase exprNL ){
            Log( $"[{logId}] Emit called with number literal ${exprNL.Value}" );
            instructions.Add( new Instruction.PushInt( exprNL.Value ) );

        // Binary Operation: Apply an operation to two literals
        }else if( expr is Expr.BinaryCase exprBN ){
            Log( $"[{logId}] Emit called with binary expression ${expr}");
            Log( $"[{logId}] Recursing on the left side");
            Emit( exprBN.Left, instructions, context );

            Log( $"[{logId}] Recursing on the right side" );
            Emit( exprBN.Right, instructions, context );
            
            Log("[$logId] Now adding binary operator ${instructionForOperator(expr.operator)}");
            instructions.Add( InstructionForOperator( exprBN.Operator ) );
        
        // Variable Value: Load the value at the given slot
        }else if( expr is Expr.VariableCase exprVR ){
            int slot = context.locals[ exprVR.Name ];
            if( slot == 0 ){
                throw new InvalidOperationException( $"Referencing undefined variable ${exprVR.Name}" );
            }
            instructions.Add( new Instruction.LoadLocal( slot ) );

        // Function Call: 
        }else if( expr is Expr.FunctionCallCase exprFC ){

            if (!functionSignatures.TryGetValue( exprFC.Name, out FunctionSignature signature))
                throw new InvalidOperationException( "Calling unknown function" );

            if( signature.parameters.Count != exprFC.Arguments.Count ){
                throw new InvalidOperationException( "Function expected a different number of args than it received" );
            }

            // For each argument, Add an instruction to get its value
            foreach( Expr argument in exprFC.Arguments ){  Emit( argument, instructions, context );  }

            // If function call has an address, Then add the call instruction, Else call is pending, send call instruction to end
            if( signature.address > 0 ){
                instructions.Add( new Instruction.CallFunction( signature.address, exprFC.Arguments.Count ) );
            }else{
                PendingFunctionCall fc = new(){
                    name = exprFC.Name,
                    instructionIndex = instructions.Count, 
                    arity = exprFC.Arguments.Count
                };
                pendingFunctionCalls.Add( fc );
                instructions.Add( new Instruction.CallFunction( 999, 999 ) ); 
            }
        }
    }


    /// <summary>
    /// Interpret the statement as `Instruction`s, Add the instruction(s) to the list of instructions
    /// </summary>
    protected void Emit( Stmt stmt, List<Instruction> instructions, LocalContext context ) {
        
        // Single Expression: Add instructions for that expression
        if( stmt is Stmt.ExpressionStmtCase stmtES ){
            Emit( stmtES.Expression, instructions, context );

        // Variable Declaration: Store a value in a new named slot
        }else if( stmt is Stmt.VarDeclarationCase stmtVD ){
            if( context.locals.TryGetValue( stmtVD.Name, out _ ) )
                throw new InvalidOperationException( $"Duplicate definition of variable detected ${stmtVD.Name}" );
            
            Log( "WARNING: I DON'T KNOW WHAT THIS DOES" );
            Emit( stmtVD.Initializer, instructions, context ); // WARNING: I DON'T KNOW WHAT THIS DOES
            
            int slot = context.GetNextSlot();
            context.locals[ stmtVD.Name ] = slot;
            instructions.Add( new Instruction.StoreLocal( slot ) );

        // Variable Update: Store a value in an existing named slot
        }else if( stmt is Stmt.VarUpdateCase stmtVU ){
            int slot = context.locals[ stmtVU.Name ];
            if( slot == 0 ){
                throw new InvalidOperationException( $"Failed to update a variable with ${stmtVU.Name} as name before declaration" );
            }
            Emit( stmtVU.Value, instructions, context );
            instructions.Add( new Instruction.StoreLocal( slot ) );

        /*
        * instructions before the if statement
        * instructions nededed to evaluate the condition of the if statement
        * jump if false instruction 
        * instructions for each statement in the body of the if-statement {..}
        * target jump location
        */
        }else if( stmt is Stmt.IfStmtCase stmtIF ){
            Log( "WARNING: INSPECT CONDITIONAL INSTRUCTIONS CLOSELY" );
            Emit( stmtIF.Condition, instructions, context );
            int jumpInstructionIndex = instructions.Count;
            instructions.Add( new Instruction.JumpIfFalse( 999 ) );
            foreach( Stmt bodyStatement in stmtIF.Body ){  Emit( bodyStatement, instructions, context );  }
            int realJumpLocation = instructions.Count;
            instructions[ jumpInstructionIndex ] = new Instruction.JumpIfFalse( realJumpLocation );
        
        // Function Declaration: Error!
        }else if( stmt is Stmt.FunctionDeclarationCase ){
            throw new InvalidOperationException( "Functions can only be declared at the top level" );
            
        // Return Statement: Store return value and pop stack frame
        }else if( stmt is Stmt.ReturnStmtCase stmtRT ){
            if( !context.isFunctionBody ){
                throw new InvalidOperationException( "Return statements may only appear within the body of a function" );
            }
            Emit( stmtRT.Value, instructions, context );
            instructions.Add( new Instruction.Return() );
        }
    }   


    /// <summary>
    /// Store arguments in the local context, Add instructions in the function body
    /// </summary>
    protected void EmitFunctionDeclaration( Stmt.FunctionDeclarationCase stmt, List<Instruction> instructions ) {
        if( stmt.Body[~1] is not Stmt.ReturnStmtCase ) {
            throw new InvalidOperationException( "Functions need to end with a return statement." );
        }

        LocalContext functionContext = new(){  isFunctionBody = true  };

        foreach( string parameter in stmt.Parameters ){
            if( !functionContext.locals.TryGetValue( parameter, out int _ ) )
                throw new InvalidOperationException( "Duplicate param defeinition" );
            int slot = functionContext.nextLocalSlot;
            functionContext.nextLocalSlot += 1;
            functionContext.locals[ parameter ] = slot;
        }

        foreach( Stmt bodyStatement in stmt.Body ){  Emit( bodyStatement, instructions, functionContext );  }
    }
}   


}