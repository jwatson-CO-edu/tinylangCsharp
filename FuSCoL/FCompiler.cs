
namespace fuscol{


////////// COMPONENT CLASSES ///////////////////////////////////////////////////////////////////////

/// <summary>
/// Local lookup of named address slots, Can be a function body
/// </summary>
// ALERT: In C#, STRUCTS ARE PASS BY VALUE, CLASSES ARE PASS BY REFERENCE!
public class Context{
    public int /*---------------*/ depth;  
    public Dictionary<string,int?> locals;
    public int? /*--------------*/ nextLocalSlot;
    public bool /*--------------*/ isFunctionBody;
    public bool /*--------------*/ isForLoopBody;
    public Context? /*----------*/ parent;

    public Context(){  locals = [];  nextLocalSlot = 0;  isFunctionBody = false;  isForLoopBody = false;  parent = null;  }


    /// <summary>
    /// Return a context within this one
    /// </summary>
    public Context GetNestedContext( bool isFunctionBody = false,  bool isForLoopBody = false ){
        return new Context(){
            depth /*----*/ = depth + 1,   
            nextLocalSlot  = (depth + 1) * VM.FRAME_SIZE,
            isFunctionBody = isFunctionBody,
            isForLoopBody  = isForLoopBody,
            parent /*---*/ = this
        };
    }


    /// <summary>
    /// Does the key exist in this or parent context?
    /// </summary>
    public bool KeyExists( string key ){
        if( locals.TryGetValue( key, out int? _ ) ){  return true;  }
        if( parent is not null ){  return parent.KeyExists( key );  }
        return false;
    }


    /// <summary>
    /// Return a value (in this or parent context) associated with the `key`
    /// </summary>
    public int? Get( string key ){
        if( locals.TryGetValue( key, out int? val ) ){  return val;  }
        if( parent is not null ){  return parent.Get( key );  }
        return null;
    }


    /// <summary>
    /// Return the currently avaialble slot number and increment slot number
    /// </summary>
    public int? GetNextSlot(){
        int? rtn = nextLocalSlot;
        nextLocalSlot += 1;
        return rtn;
    }


    /// <summary>
    /// Get all keys and parent's keys, recursive
    /// </summary>
    public List<string> Keys(){
        List<string> rtnLst = [.. locals.Keys];
        if( parent is not null ){  rtnLst.AddRange( parent.Keys() );  } 
        return rtnLst;
    }
};


/// <summary>
/// List of named parameters and the address of a stack frame
/// </summary>
// ALERT: In C#, STRUCTS ARE PASS BY VALUE, CLASSES ARE PASS BY REFERENCE!
public class FunctionSignature{
    public List<string> parameters;
    public int? /*---*/ address;

    public FunctionSignature(){  parameters = [];  address = null;  }
}


/// <summary>
/// Function name, Instruction index, and number of args
/// </summary>
// ALERT: In C#, STRUCTS ARE PASS BY VALUE, CLASSES ARE PASS BY REFERENCE!
public struct PendingFunctionCall{
    public string name;
    public int    instructionIndex; 
    public int    arity;
}


////////// COMPILER ////////////////////////////////////////////////////////////////////////////////

public class FCompiler( bool shouldLog_ = true ) {

    protected bool /*----------------------------*/ shouldLog /*------*/ = shouldLog_;
    protected Dictionary<string,FunctionSignature?> functionSignatures   = [];
    protected List<PendingFunctionCall> /*-------*/ pendingFunctionCalls = [];
    protected int /*-----------------------------*/ nextUniqueNumber     = 1;
    protected Context /*-------------------------*/ mainContext /*----*/ = new();


    /// <summary>
    /// If logging is enabled, Then print the text to the console
    /// </summary>
    public void Log( string stmt ) {  if( shouldLog ){  Console.WriteLine( stmt );  }  }
    
    
    /// <summary>
    /// Unary / Binary operator `Token` --to-> `Instruction` lookup
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
    protected void Emit( Expr expr, List<Instruction> instructions ) {
        int logId = GetNextUnique();

        Log( $"[{logId}] Current context locals:" );
        foreach( string key in mainContext.locals.Keys ){  Console.WriteLine( $"\t{key}: {mainContext.locals[key]}" );  }

        // Number Literal: Push an int onto the stack
        if( expr is Expr.NumberLiteralCase exprNL ){
            Log( $"[{logId}] Emit called with number literal ${exprNL.Value}" );
            instructions.Add( new Instruction.PushInt( exprNL.Value ) );

        // Binary Operation: Apply an operation to two literals
        }else if( expr is Expr.BinaryCase exprBN ){
            Log( $"[{logId}] Emit called with binary expression ${exprBN}");
            Log( $"[{logId}] Recursing on the left side");
            Emit( exprBN.Left, instructions );

            Log( $"[{logId}] Recursing on the right side" );
            Emit( exprBN.Right, instructions );
            
            Log( $"[{logId}] Now adding binary operator instructionForOperator({exprBN.Operator})");
            instructions.Add( InstructionForOperator( exprBN.Operator ) );
        
        // Variable Value: Load the value at the given slot
        }else if( expr is Expr.VariableCase exprVR ){

            int? slot = mainContext.Get( exprVR.Name );

            if( slot == null ){  throw new InvalidOperationException( 
                $"Referencing undefined variable ${exprVR.Name}" 
            );  }

            Log( $"[{logId}] Fetch variable {exprVR.Name} from local context at slot {slot}" );

            instructions.Add( new Instruction.LoadLocal( slot ) );

        // Function Call: 
        }
    }
}

}