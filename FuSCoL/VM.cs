namespace fuscol{


/// <summary>
/// Is this more like a computer?
/// </summary>
public class VM( bool shouldLog_ = true ) {

    protected const int /*---*/ STACK_MAX    = 1024;
    protected const int /*---*/ FRAME_SIZE   =   64;
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


    public int? Get( int index ){
        if( index >= STACK_MAX ){  throw new InvalidOperationException( $"BAD STACK ADDRESS: {index}" );  }
        return stack[ index ];
    }


    public void LoadToStack( int index ){
        Push( Get( index ) );
    }


    public void LoadFrame(){

    }


}



}