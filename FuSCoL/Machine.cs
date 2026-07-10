
namespace fuscol{


/// <summary>
/// Fake Computer
/// </summary>
public class Machine {

    protected List<int?> /**/ stack     = [];
    protected List<CallFrame> callStack = [];


    /// <summary>
    /// Pop an value from the top of the stack
    /// </summary>
    protected int? Pop() {
        if( stack.Count == 0 ){  throw new InvalidOperationException( "Stack underflow" );  }
        int? rtn = stack[^1];
        stack.RemoveAt( stack.Count-1 );
        return rtn;
    }


    /// <summary>
    /// Expand local vars to the given slot (if needed), then store in that slot
    /// </summary>
    protected static void StoreLocal( List<int?> activeLocals, int? slot, int? value ) {
        if( slot != null ){
            Console.WriteLine( $"About to create {slot-activeLocals.Count} slots!" );
            while ( activeLocals.Count <= slot ){  activeLocals.Add( null );  }
            activeLocals[ (int) slot ] = value;
        }else{
            throw new InvalidOperationException( $"CANNOT STORE AT `null`" );
        }
    }


    /// <summary>
    /// Fetch a value from the specified slot
    /// </summary>
    protected static int? LoadLocal( List<int?> activeLocals, int? slot ) {
        // if( slot == null ){ return null; }
        // slot ??= 0;

        Console.WriteLine( $"Load {slot} from {activeLocals.ToDisplayString()}" );

        if( (slot == null) || (slot >= activeLocals.Count) || (activeLocals[ (int) slot ] == null) ){
            throw new InvalidOperationException( $"Undefined local slot {slot}" );
        }
        return activeLocals[ (int) slot ];
    }


    /// <summary>
    /// Create slots for the function local context
    /// </summary>
    protected List<int?> CreateFunctionLocals( int arity ) {
        List<int?> callLocals = [];
        List<int?> arguments  = [];

        for( int i = 0; i < arity; ++i ){  arguments.Add( Pop() );  }
        
        arguments.Reverse();

        for( int index = 0; index < arguments.Count; ++index ){
            StoreLocal( callLocals, index, arguments[ index ] );
        }

        return callLocals;
    }
    

    /// <summary>
    /// (protected) Execute program as a list of instructions
    /// </summary>
    protected int? Execute( Instruction instruction, List<int?> activeLocals,
                            int? nextInstructionPointer, Action<List<int?>> restoreLocals ) 
    {

        Console.WriteLine( $"Execute {instruction} on locals: {activeLocals.ToDisplayString()}" );

        if( instruction is Instruction.PushInt insVal ){
            stack.Add( insVal.Value );
        }else if( instruction is Instruction.Add ){
            int? right = Pop();
            int? left  = Pop();
            stack.Add( left + right );
        }else if( instruction is Instruction.Sub ){
            int? right = Pop();
            int? left  = Pop();
            stack.Add( left - right );
        }else if( instruction is Instruction.Mul ){
            int? right = Pop();
            int? left  = Pop();
            stack.Add( left * right );
        }else if( instruction is Instruction.Div ){
            int? right = Pop();
            int? left  = Pop();
            stack.Add( left / right );
        }else if( instruction is Instruction.Exp ){
            int? right = Pop();
            int? left  = Pop();
            int  r     = right ?? 0;
            int  l     = left  ?? 0;
            if( (right is not null) & (left is not null) ){
                stack.Add( (int) Math.Pow( l, r ) );
            }else{
                Console.WriteLine( $"One of the operands is NULL: {left} ** {right}" );
                stack.Add( null );
            }
        }else if( instruction is Instruction.LessThan ){
            int? right = Pop();
            int? left  = Pop();
            if( left < right ){  stack.Add(1);  }else{  stack.Add(0);  }
        }else if( instruction is Instruction.GreaterThan ){
            int? right = Pop();
            int? left  = Pop();
            if( left > right ){  stack.Add(1);  }else{  stack.Add(0);  }
        }else if( instruction is Instruction.StoreLocal insSto ){
            StoreLocal( activeLocals, insSto.Slot, Pop() );
        }else if( instruction is Instruction.LoadLocal insLod ){
            stack.Add( LoadLocal( activeLocals, insLod.Slot ) );
        }else if( instruction is Instruction.Jump insJmp ){
            return insJmp.Target;
        }else if( instruction is Instruction.JumpIfFalse insJIF ){
            int? condition = Pop();
            if( condition == 0 ){  return insJIF.Target;  }    
        }else if( instruction is Instruction.CallFunction insCal ){
            List<int?> callLocals = CreateFunctionLocals( insCal.Arity );
            CallFrame frame = new(){
                returnAddress = nextInstructionPointer,
                locals /*--*/ = activeLocals
            };
            callStack.Add( frame );
            restoreLocals( callLocals );
            return insCal.Address;    
        }else if( instruction is Instruction.Return ){
            if( callStack.Count == 0 ){  return null;  }
            CallFrame frame = callStack[^1];
            callStack.RemoveAt( callStack.Count-1 );
            restoreLocals( frame.locals );
            return frame.returnAddress;
        }else{
            throw new InvalidOperationException( $"BONK" );
        }
        return null;
    }


    /// <summary>
    /// (public) Execute program as a list of instructions, Handle the program counter
    /// </summary>
    public List<int?> Run( List<Instruction> instructions ){
        stack.Clear();
        callStack.Clear();
        List<int?> activeLocals /*-----*/ = [];
        int? /*-*/ instructionPointer     = 0;
        int? /*-*/ nextInstructionPointer = null;

        while( instructionPointer < instructions.Count ){

            Console.WriteLine( $"\nInstruction Pointer: {instructionPointer}" );

             nextInstructionPointer = Execute(
                instructions[(int)instructionPointer],
                activeLocals,
                instructionPointer + 1,
                restoredLocals => { activeLocals = restoredLocals; }
            );
            instructionPointer = nextInstructionPointer ?? instructionPointer + 1;
            // Console.WriteLine( $"\nNext Instruction Pointer: {nextInstructionPointer}" );
            // Console.WriteLine( $"Instruction Pointer: ____ {instructionPointer}" );
        }
        return [.. stack];
    }

}

}