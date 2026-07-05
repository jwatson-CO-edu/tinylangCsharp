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

    
    protected bool shouldLog = shouldLog_;
    protected Dictionary<string,FunctionSignature> functionSignatures   = [];
    protected List<PendingFunctionCall> /*------*/ pendingFunctionCalls = [];


    public void Log( string stmt ) {
        if( shouldLog ){  Console.WriteLine( stmt );  }
    }


}   


}