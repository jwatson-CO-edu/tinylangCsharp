
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


    public Context GetNestedContext( bool isFunctionBody = false,  bool isForLoopBody = false ){
        return new Context(){
            depth /*----*/ = depth + 1,   
            nextLocalSlot  = (depth + 1) * VM.FRAME_SIZE,
            isFunctionBody = isFunctionBody,
            isForLoopBody  = isForLoopBody,
            parent /*---*/ = this
        };
    }


    public bool KeyExists( string key ){
        if( locals.TryGetValue( key, out int? _ ) ){  return true;  }
        if( parent is not null ){  return parent.KeyExists( key );  }
        return false;
    }


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
}

}