
namespace fuscol{

public class Compiler( bool shouldLog_ = true ) {

    /// <summary>
    /// Local lookup of named address slots, Can be a function body
    /// </summary>
    // ALERT: In C#, STRUCTS ARE PASS BY VALUE, CLASSES ARE PASS BY REFERENCE!
    protected class LocalContext{
        public Dictionary<string,int?> locals;
        public int? /*--------------*/ nextLocalSlot;
        public bool /*--------------*/ isFunctionBody;
        public bool /*--------------*/ isForLoopBody;

        public LocalContext(){  locals = [];  nextLocalSlot = 0;  isFunctionBody = false;  isForLoopBody = false;  }

        /// <summary>
        /// Return the currently avaialble slot number and increment slot number
        /// </summary>
        public int? GetNextSlot(){
            int? rtn = nextLocalSlot;
            nextLocalSlot += 1;
            return rtn;
        }
    };


    /// <summary>
    /// List of named parameters and the address of a stack frame
    /// </summary>
    // ALERT: In C#, STRUCTS ARE PASS BY VALUE, CLASSES ARE PASS BY REFERENCE!
    protected class FunctionSignature{
        public List<string> parameters;
        public int? /*---*/ address;

        public FunctionSignature(){  parameters = [];  address = null;  }
    }


    /// <summary>
    /// Function name, Instruction index, and number of args
    /// </summary>
    // ALERT: In C#, STRUCTS ARE PASS BY VALUE, CLASSES ARE PASS BY REFERENCE!
    protected struct PendingFunctionCall{
        public string name;
        public int    instructionIndex; 
        public int    arity;
    }

    
    protected bool /*----------------------------*/ shouldLog /*------*/ = shouldLog_;
    protected Dictionary<string,FunctionSignature?> functionSignatures   = [];
    protected List<PendingFunctionCall> /*-------*/ pendingFunctionCalls = [];
    protected int /*-----------------------------*/ nextUniqueNumber     = 1;


    /// <summary>
    /// If logging is enabled, Then print the text to the console
    /// </summary>
    public void Log( string stmt ) {  if( shouldLog ){  Console.WriteLine( stmt );  }  }


    /// <summary>
    /// Binary operator `Token` --to-> `Instruction` lookup
    /// </summary>
    protected static Instruction InstructionForOperator( Token op ) {
        return op.type switch{ // `swtich` EXPRESSION, NOT statement
            /// Unary (Prefix) ///
            TokenType.DBBL_PLUS  => new Instruction.Increment(),
            TokenType.DBBL_MINUS => new Instruction.Decrement(),
            /// Binary ///
            TokenType.PLUS /*---*/ => new Instruction.Add(),
            TokenType.MINUS /*--*/ => new Instruction.Sub(),
            TokenType.STAR /*---*/ => new Instruction.Mul(),
            TokenType.SLASH /*--*/ => new Instruction.Div(),
            TokenType.DBBL_STAR    => new Instruction.Exp(),
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
            if( !functionSignatures.TryGetValue( pendingCall.name, out FunctionSignature? signature ) )
                throw new InvalidOperationException( "Unreachable" );
            if( signature == null ){  throw new InvalidOperationException( "NULL: Unreachable" );  }
            instructions[ pendingCall.instructionIndex ] = new Instruction.CallFunction( signature.address, pendingCall.arity );
        }
    }


    /// <summary>
    /// Assign `address` to function `name`
    /// </summary>
    protected void SetFunctionAddress( string name, int address ) {
        if( !functionSignatures.TryGetValue( name, out FunctionSignature? signature ) ){
            Console.WriteLine( "Registered Functions:" );
            foreach( string fName in functionSignatures.Keys ){
                Console.WriteLine( $"\t{fName}" );
            }
            throw new InvalidOperationException( "Function not registered?" );
        }
        if( signature == null ){  throw new InvalidOperationException( "NULL: shouldnt be possible" );  }
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

        Log( $"[{logId}] Current context locals:" );
        foreach( string key in context.locals.Keys ){  Console.WriteLine( $"\t{key}: {context.locals[key]}" );  }

        Log( $"[{logId}] Current instructions:" );
        foreach( Instruction instruction in instructions ){  Console.WriteLine( $"\t{instruction}" );  }

        // Number Literal: Push an int onto the stack
        if( expr is Expr.NumberLiteralCase exprNL ){
            Log( $"[{logId}] Emit called with number literal ${exprNL.Value}" );
            instructions.Add( new Instruction.PushInt( exprNL.Value ) );

        // Binary Operation: Apply an operation to two literals
        }else if( expr is Expr.BinaryCase exprBN ){
            Log( $"[{logId}] Emit called with binary expression ${exprBN}");
            Log( $"[{logId}] Recursing on the left side");
            Emit( exprBN.Left, instructions, context );

            Log( $"[{logId}] Recursing on the right side" );
            Emit( exprBN.Right, instructions, context );
            
            Log( $"[{logId}] Now adding binary operator instructionForOperator({exprBN.Operator})");
            instructions.Add( InstructionForOperator( exprBN.Operator ) );
        
        // Variable Value: Load the value at the given slot
        }else if( expr is Expr.VariableCase exprVR ){

            int? slot = context.locals[ exprVR.Name ];

            if( slot == null ){
                throw new InvalidOperationException( $"Referencing undefined variable ${exprVR.Name}" );
            }

            Log( $"[{logId}] Fetch variable {exprVR.Name} from local context at slot {slot}" );

            instructions.Add( new Instruction.LoadLocal( slot ) );

        // Function Call: 
        }else if( expr is Expr.FunctionCallCase exprFC ){

            if( !functionSignatures.TryGetValue( exprFC.Name, out FunctionSignature? signature ) )
                throw new InvalidOperationException( "Calling unknown function" );
            if( signature == null ){  throw new InvalidOperationException( "NULL: Calling unknown function" );  }
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
                    name /*-------*/ = exprFC.Name,
                    instructionIndex = instructions.Count, 
                    arity /*------*/ = exprFC.Arguments.Count
                };
                pendingFunctionCalls.Add( fc );
                instructions.Add( new Instruction.CallFunction( 999, 999 ) ); 
            }
        }else{
            throw new InvalidOperationException( $"BONK" );
        }
    }


    /// <summary>
    /// Store arguments in the local context, Add instructions in the function body
    /// </summary>
    protected void EmitForLoop( Stmt.ForLoopCase stmt, List<Instruction> instructions ) {

        LocalContext forLoopContext = new(){  isForLoopBody = true  };
        int? slot = forLoopContext.GetNextSlot();
        int? instBody = null;
        int? instTest = null;
        int? instUpdt = null;
        

        if( stmt.Counter[0] is Stmt.VarDeclarationCase init ){
            forLoopContext.locals[ init.Name ] = slot;
            Emit( init, instructions, forLoopContext );
            instBody = instructions.Count+1;
            instructions.Add( new Instruction.Jump( instBody ) );

        }else{  throw new InvalidOperationException( $"Expected a counter var initialization, got {stmt.Counter[0]}" );  }

        if( stmt.Counter[1] is Stmt.ExpressionStmtCase test ){

        }else{  throw new InvalidOperationException( $"Expected a counter var test, got {stmt.Counter[1]}" );  }

        if( stmt.Counter[2] is Stmt.VarUpdateCase updt ){

        }else{  throw new InvalidOperationException( $"Expected a counter var update, got {stmt.Counter[2]}" );  }


        foreach( string parameter in stmt.Parameters ){
            // if( !functionContext.locals.TryGetValue( parameter, out int? _ ) )
            if( functionContext.locals.TryGetValue( parameter, out int? _ ) )
                throw new InvalidOperationException( "Duplicate param defeinition" );
            
        }

        foreach( Stmt bodyStatement in stmt.Body ){    }
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
            
            Emit( stmtVD.Initializer, instructions, context );
            
            int? slot = context.GetNextSlot();
            Log( $"Declare Variable {stmtVD.Name} @ Slot {slot}" );

            context.locals[ stmtVD.Name ] = slot;
            instructions.Add( new Instruction.StoreLocal( slot ) );

        // Variable Update: Store a value in an existing named slot
        }else if( stmt is Stmt.VarUpdateCase stmtVU ){
            if( !context.locals.TryGetValue( stmtVU.Name, out _ ) )
                throw new InvalidOperationException( $"Failed to update a variable with {stmtVU.Name} as name before declaration" );
            int? slot = context.locals[ stmtVU.Name ];
            if( slot == null ){
                throw new InvalidOperationException( $"Failed to update a variable with {stmtVU.Name} as name before declaration" );
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

        }else if( stmt is Stmt.ForLoopCase stmtFL ){
            EmitForLoop( stmtFL );

        }else{
            throw new InvalidOperationException( $"BONK" );
        }
    }   


    /// <summary>
    /// Store arguments in the local context, Add instructions in the function body
    /// </summary>
    protected void EmitFunctionDeclaration( Stmt.FunctionDeclarationCase stmt, List<Instruction> instructions ) {
        if( stmt.Body[^1] is not Stmt.ReturnStmtCase ) {
            throw new InvalidOperationException( "Functions need to end with a return statement." );
        }

        LocalContext functionContext = new(){  isFunctionBody = true  };

        foreach( string parameter in stmt.Parameters ){
            // if( !functionContext.locals.TryGetValue( parameter, out int? _ ) )
            if( functionContext.locals.TryGetValue( parameter, out int? _ ) )
                throw new InvalidOperationException( "Duplicate param defeinition" );
            int? slot = functionContext.GetNextSlot();
            functionContext.locals[ parameter ] = slot;
        }

        foreach( Stmt bodyStatement in stmt.Body ){  Emit( bodyStatement, instructions, functionContext );  }
    }


    protected void RegisterSignature( Stmt.FunctionDeclarationCase stmt ) {

        if( functionSignatures.TryGetValue( stmt.Name, out FunctionSignature? _ ) )
            throw new InvalidOperationException( "Duplicate function declaration" );

        HashSet<string> seenParameters = [];

        foreach( string parameter in stmt.Parameters ){
            if( !seenParameters.Add( parameter ) ){
                throw new InvalidOperationException( "Duplicate param definition in function" );
            }
        }

        functionSignatures[ stmt.Name ] = new FunctionSignature(){
            parameters = stmt.Parameters
            // address left null/default here — that's set later by SetFunctionAddress
        };
    }


    public CompileResult Compile( List<Stmt> statements )  {
        Log( "Top level compile function called" );
        LocalContext /**/ mainContext  = new();
        List<Instruction> instructions = [];
        
        instructions.Add( new Instruction.Jump( 999 ) );
        
        // Register function signature
        foreach( Stmt statement in statements ){
            if( statement is Stmt.FunctionDeclarationCase stmtFnc ){  RegisterSignature( stmtFnc );  }
        }
        
        // Parse function bodies
        foreach( Stmt statement in statements ){
            if( statement is Stmt.FunctionDeclarationCase stmtFnc ){
                SetFunctionAddress( stmtFnc.Name, instructions.Count );
                EmitFunctionDeclaration( stmtFnc, instructions );
            }
        }
        
        instructions[0] = new Instruction.Jump( instructions.Count );
        foreach( Stmt statement in statements ){
            if( statement is not Stmt.FunctionDeclarationCase ){
                Emit( statement, instructions, mainContext ); 
            }
        }
        PatchFunctionCalls( instructions );
        return new CompileResult( instructions );
    }

}   


}