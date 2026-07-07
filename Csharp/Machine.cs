namespace tlCsharp{

public class Machine {

    protected List<int> /*-*/ stack     = [];
    protected List<CallFrame> callStack = [];

    protected Instruction? instruction /*------*/ = null;
    protected List<int>    activeLocals /*-----*/ = [];
    protected int /*----*/ nextInstructionPointer = -1;
    protected List<int>    restoreLocals /*----*/ = [];


    protected int Pop() {
        if( stack.Count == 0 ){  throw new InvalidOperationException( "Stack underflow" );  }
        int rtn = stack[~1];
        stack.RemoveAt( stack.Count-1 );
        return rtn;
    }


    protected static void StoreLocal( List<int> activeLocals, int slot, int value ) {
        while ( activeLocals.Count <= slot ){  activeLocals.Add(0);  }
        activeLocals[slot] = value;
    }


    protected static int LoadLocal( List<int> activeLocals, int slot ) {
        if ( (slot >= activeLocals.Count) || (activeLocals[ slot ] == 0) ) {
            throw new InvalidOperationException( $"Undefined local slot {slot}" );
        }
        return activeLocals[ slot ];
    }


    protected List<int> CreateFunctionLocals( int arity ) {
        List<int> callLocals = [];
        List<int> arguments  = [];

        for( int i = 0; i < arity; ++i ){  arguments.Add( Pop() );  }
        
        arguments.Reverse();

        for( int index = 0; index < arguments.Count; ++index ){
            StoreLocal(callLocals, index, arguments[ index ] );
        }

        return callLocals;
    }
    

    protected int? Execute( Instruction instruction, List<int> activeLocals,
                            int nextInstructionPointer, Action<List<int>> restoreLocals ) {

        if( instruction is Instruction.PushInt insVal ){
            stack.Add( insVal.Value );
        }else if( instruction is Instruction.Add insAdd ){
            int right = Pop();
            int left  = Pop();
            stack.Add( left + right );
        }else if( instruction is Instruction.Sub insSub ){
            int right = Pop();
            int left  = Pop();
            stack.Add( left - right );
        }else if( instruction is Instruction.Mul insMul ){
            int right = Pop();
            int left  = Pop();
            stack.Add( left * right );
        }else if( instruction is Instruction.Div insDiv ){
            int right = Pop();
            int left  = Pop();
            stack.Add( left / right );
        }else if( instruction is Instruction.LessThan insLsT ){
            int right = Pop();
            int left  = Pop();
            if( left < right ){  stack.Add(1);  }else{  stack.Add(0);  }
        }else if( instruction is Instruction.GreaterThan insGrT ){
            int right = Pop();
            int left  = Pop();
            if( left > right ){  stack.Add(1);  }else{  stack.Add(0);  }
        }else if( instruction is Instruction.StoreLocal insSto ){
            StoreLocal( activeLocals, insSto.Slot, Pop() );
        }else if( instruction is Instruction.LoadLocal insLod ){
            stack.Add( LoadLocal( activeLocals, insLod.Slot ) );
        }else if( instruction is Instruction.Jump insJmp ){
            return insJmp.Target;
        }else if( instruction is Instruction.JumpIfFalse insJIF ){
            int condition = Pop();
            if( condition == 0 ){  return insJIF.Target;  }    
        }else if( instruction is Instruction.CallFunction insCal ){
            List<int> callLocals = CreateFunctionLocals( insCal.Arity );
            CallFrame frame = new(){
                returnAddress = nextInstructionPointer,
                locals /*--*/ = activeLocals
            };
            callStack.Add( frame );
            restoreLocals( callLocals );
            return insCal.Address;    
        }else if( instruction is Instruction.Return insRtn ){
            if( callStack.Count == 0 ){  return null;  }
            CallFrame frame = callStack[~1];
            callStack.RemoveAt( callStack.Count-1 );
            restoreLocals( frame.locals );
            return frame.returnAddress;
        }
        return 0;
    }

}

}