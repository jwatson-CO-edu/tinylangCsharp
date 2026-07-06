
namespace tlCsharp{

public class Compiler( bool shouldLog_ = true ) {

    protected struct LocalContext{
        public Dictionary<string,int> locals;
        public int /*--------------*/ nextLocalSlot;
        public bool /*-------------*/ isFunctionBody;

        public LocalContext(){  locals = [];  nextLocalSlot = 0;  isFunctionBody = false;  }
    };


    protected struct FunctionSignature{
        public List<string> parameters;
        public int /*----*/ address;

        public FunctionSignature(){  parameters = [];  address = -1;  }
    }


    protected struct PendingFunctionCall{
        public string name;
        public int    instructionIndex; 
        public int    arity;
    }

    
    protected bool /*---------------------------*/ shouldLog /*------*/ = shouldLog_;
    protected Dictionary<string,FunctionSignature> functionSignatures   = [];
    protected List<PendingFunctionCall> /*------*/ pendingFunctionCalls = [];
    protected int /*----------------------------*/ nextUniqueNumber     = 1;


    public void Log( string stmt ) {  if( shouldLog ){  Console.WriteLine( stmt );  }  }


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


    protected void PatchFunctionCalls( List<Instruction> instructions ) {   
        foreach( PendingFunctionCall pendingCall in pendingFunctionCalls ){
            if (!functionSignatures.TryGetValue(pendingCall.name, out FunctionSignature signature))
                throw new InvalidOperationException("Unreachable");
            instructions[ pendingCall.instructionIndex ] = new Instruction.CallFunction( signature.address, pendingCall.arity );
        }
    }


    protected void SetFunctionAddress( string name, int address ) {
        if (!functionSignatures.TryGetValue(name, out FunctionSignature signature))
                throw new InvalidOperationException( "shouldnt be possible" );
        signature.address = address;
    }


    protected int GetNextUnique(){
        int rtn = nextUniqueNumber;
        nextUniqueNumber += 1;
        return rtn;
    }


    protected void Emit( Expr expr, List<Instruction> instructions, LocalContext context ) {
    
        int logId = GetNextUnique();

        if( expr is Expr.NumberLiteralCase exprNL ){
            Log( $"[{logId}] Emit called with number literal ${exprNL.Value}" );
            instructions.Add( new Instruction.PushInt(exprNL.Value) );
        }else if( expr is Expr.BinaryCase exprBN ){
            Log( $"[{logId}] Emit called with binary expression ${expr}");
            Log( $"[{logId}] Recursing on the left side");
            Emit( exprBN.Left, instructions, context );

            Log( $"[{logId}] Recursing on the right side" );
            Emit( exprBN.Right, instructions, context );
            
            Log("[$logId] Now adding binary operator ${instructionForOperator(expr.operator)}");
            instructions.Add( InstructionForOperator( exprBN.Operator ) );
        }else if( expr is Expr.VariableCase exprVR ){
            int slot = context.locals[ exprVR.Name ];
            if( slot == 0 ){
                throw new InvalidOperationException( $"Referencing undefined variable ${exprVR.Name}" );
            }
            instructions.Add( new Instruction.LoadLocal( slot ) );
        }else if( expr is Expr.FunctionCallCase exprFC ){

            if (!functionSignatures.TryGetValue( exprFC.Name, out FunctionSignature signature))
                throw new InvalidOperationException( "Calling unknown function" );

            if( signature.parameters.Count != exprFC.Arguments.Count ){
                throw new InvalidOperationException( "Function expected a different number of args than it received" );
            }

            foreach( Expr argument in exprFC.Arguments ){
                Emit( argument, instructions, context );
            }

            if (signature.address > 0) {
                instructions.Add( new Instruction.CallFunction( signature.address, exprFC.Arguments.Count ) );
            } else {
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


    protected void Emit( Stmt stmt, List<Instruction> instructions, LocalContext context ) {
        
        if( stmt is Stmt.ExpressionStmtCase stmtES ){
            Emit( stmtES.Expression, instructions, context);
        }else if( stmt is Stmt.VarDeclarationCase ){
            // FIXME: START HERE
        }else if( stmt is Stmt.VarUpdateCase ){
            
        }else if( stmt is Stmt.IfStmtCase ){
            
        }else if( stmt is Stmt.FunctionDeclarationCase ){
            
        }else if( stmt is Stmt.ReturnStmtCase ){
            
        }

        
        when () {
            is Stmt.ExpressionStmt -> {
                
            }
            is Stmt.VarDeclaration -> {
                if (context.locals.containsKey(stmt.name)) {
                error("Duplicate definition of variable detected ${stmt.name}")
                }
                emit(stmt.initializer, instructions, context)
                val slot: Int = context.nextLocalSlot
                context.nextLocalSlot = context.nextLocalSlot + 1
                context.locals[stmt.name] = slot
                instructions.add(Instruction.StoreLocal(slot))
            }
            is Stmt.VarUpdate -> {
                val slot: Int? = context.locals[stmt.name]
                if (slot == null) {
                error("Failed to update a variable with ${stmt.name} as name before declaration")
                }
                emit(stmt.value, instructions, context)
                instructions.add(Instruction.StoreLocal(slot))
            }
        /*
            * instructions before the if statement
            * instructions nededed to evaluate the condition of the if statement
            * jump if false instruction 
            * instructions for each statement in the body of the if-statement {..}
            * target jump location
            */
            is Stmt.IfStmt -> {
            emit(stmt.condition, instructions, context)
            val jumpInstructionIndex: Int = instructions.size
            instructions.add(Instruction.JumpIfFalse(999))
            stmt.body.forEach { bodyStatement: Stmt -> emit(bodyStatement, instructions, context) }
            val realJumpLocation: Int = instructions.size
            instructions[jumpInstructionIndex] = Instruction.JumpIfFalse(realJumpLocation)
            }
            is Stmt.FunctionDeclaration -> {
            error("Functions can only be declared at the top level")
            }
            is Stmt.ReturnStmt -> {
            if (!context.isFunctionBody) {
                error("Return statements may only appear within the body of a function")
            }
            emit(stmt.value, instructions, context)
            instructions.add(Instruction.Return)
            }
        }
    }   

    protected void EmitFunctionDeclaration( Stmt.FunctionDeclarationCase stmt, List<Instruction> instructions ) {
        if( stmt.Body[~1] is not Stmt.ReturnStmtCase ) {
            throw new InvalidOperationException( "Functions need to end with a return statement." );
        }

        LocalContext functionContext = new(){
            isFunctionBody = true
        };

        foreach( string parameter in stmt.Parameters ){
            if (!functionContext.locals.TryGetValue(parameter, out int _))
                throw new InvalidOperationException( "Duplicate param defeinition" );
            int slot = functionContext.nextLocalSlot;
            functionContext.nextLocalSlot += 1;
            functionContext.locals[parameter] = slot;
        }

        foreach( Stmt bodyStatement in stmt.Body ){
            
        }

            
    }
        
        .forEach {  -> 
            emit(bodyStatement, instructions, functionContext)
        }
    }

}   


}