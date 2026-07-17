namespace fuscol{

/* ////////// SPECIFICATION ////////////////////////////////////////////////////////////////////////
- Stack is limited to `STACK_MAX` slots
- Each function call jumps to the next whole `FRAME_SIZE`
- The first slot of every frame depth > 0 is the return location for the stack pointer
- The first slot of the root frame (depth == 0) is the return code of the program 
*/

/// <summary>
/// Is this more like a computer?
/// </summary>
public class VM( bool shouldLog_ = true ) {

    public    const int /*---*/ STACK_MAX    = 1024;
    public    const int /*---*/ FRAME_SIZE   =   64;
    protected bool /*--------*/ shouldLog    = shouldLog_;
    protected int?[] /*------*/ stack /*--*/ = new int?[STACK_MAX];
    protected int /*---------*/ stkPtr /*-*/ = -1;
    protected int /*---------*/ depth /*--*/ = -1;
    protected List<CallFrame>   callStack    = [];
    protected List<Instruction> instructions = [];
    

    /// <summary>
    /// Condtionally print a `stmt` 
    /// </summary>
    public void Log( string stmt ) {  if( shouldLog ){  Console.WriteLine( stmt );  }  }


    /// <summary>
    /// Push a value to the top of the stack
    /// </summary>
    public void Push( int? val ) {
        if( stkPtr >= STACK_MAX ){  throw new InvalidOperationException( "Stack overflow" );  }
        stkPtr++;
        stack[ stkPtr ] = val;
    }


    /// <summary>
    /// Pop a value from the top of the stack
    /// </summary>
    public int? Pop() {
        if( stkPtr < 0 ){  throw new InvalidOperationException( "Stack underflow" );  }
        int? rtn = stack[ stkPtr ];
        stkPtr--;
        return rtn;
    }


    /// <summary>
    /// Fetch a value from the stack frame
    /// </summary>
    public int? Get( int index ){
        if( index >= STACK_MAX ){  throw new InvalidOperationException( $"BAD STACK ADDRESS: {index}" );  }
        return stack[ index ];
    }


    /// <summary>
    /// Push a stack frame value
    /// </summary>
    public void LoadToStack( int index ){  Push( Get( index ) );  }


    /// <summary>
    /// Push a new stack frame
    /// </summary>
    public void PushFrame(){
        depth++;
        int last = stkPtr;
        stkPtr = depth * FRAME_SIZE - 1;
        if( (stkPtr+1) >= STACK_MAX ){  throw new InvalidOperationException( $"STACK OVERFLOW: {stkPtr}" );  }
        Push( last );
    }


    /// <summary>
    /// Pop a stack frame
    /// </summary>
    public void PopFrame(){
        if( depth <= 0 ){  throw new InvalidOperationException( $"ATTEMPTED TO POP ROOT FRAME" );  }
        int? ptr = stack[ depth * FRAME_SIZE ];
        if( ptr is not null ){  
            stkPtr = (int) ptr;  
            depth--;
        }else{  throw new InvalidOperationException( $"RETURN POINTER WAS NULL" );  }
    }
}



}