namespace fuscol{

public enum TokenType {
    ///// tinylang ///////////////////////////////
    /// Basic Syntax / Blocks ///
    SEMICOLON,
    OPEN_BRACE,
    CLOSE_BRACE,
    END_OF_FILE,
    /// Math ///
    NUMBER,
    PLUS,
    MINUS,
    STAR,
    SLASH,
    OPEN_PARENTHESIS,
    CLOSE_PARENTHESIS,
    /// Comparison / Conditional ///
    LESS_THAN,
    GREATER_THAN,
    IF,
    /// Variables ///
    LET,
    IDENTIFIER,
    EQUAL,
    UPDATE,
    TO,
    /// Functions ///
    FUN,
    COMMA,
    RETURN,
    INVOKE,
    ///// FuSCoL /////////////////////////////////
    /// Math ///
    // Binary //
    DBBL_STAR,
    // Unary //
    DBBL_PLUS,
    DBBL_MINUS,
    /// Loops ///
    FOR,
    /// Comparison / Conditional ///
    DBBL_EQUAL,
    /// Comments ///
    DBBL_SLASH,
}



public class Token( TokenType type_, string lit = "" ) {
    public TokenType type    = type_;
    public string    literal = lit;

    public override string ToString() => $"<T: {type} = '{literal}'>";
}



public abstract record Expr {
    public sealed record NumberLiteralCase ( int Value ) : Expr {  
        public override string ToString() => $"<E: Number = {Value}>";  
    }
    public sealed record UnaryCase ( Token Operator, string Name ) : Expr {  
        public override string ToString() => $"<E: Op = {Operator} {Name}>";  
    }
    public sealed record BinaryCase ( Expr Left, Token Operator, Expr Right ) : Expr {  
        public override string ToString() => $"<E: Op = {Left} {Operator} {Right}>";  
    }
    public sealed record VariableCase ( string Name ) : Expr {  
        public override string ToString() => $"<E: Var = {Name}>";  
    }
    public sealed record FunctionCallCase ( string Name, List<Expr> Arguments ) : Expr {  
        public override string ToString() => $"<E: Var = {Name}, {Arguments}>";  
    }

    public static Expr NumberLiteral( int Value ) => new NumberLiteralCase( Value ); 
    public static Expr Unary( Token Operator, string Name ) => new UnaryCase( Operator, Name ); 
    public static Expr Binary( Expr Left, Token Operator, Expr Right ) => new BinaryCase( Left, Operator, Right ); 
    public static Expr Variable( string Name ) => new VariableCase( Name ); 
    public static Expr FunctionCall( string Name, List<Expr> Arguments ) => new FunctionCallCase( Name, Arguments );

}


public abstract record Stmt{

    // concrete cases - names are internal implementation detail
    public sealed record VarDeclarationCase( string Name, Expr Initializer ) : Stmt {  
        public override string ToString() => $"<S : Declare = {Name}, {Initializer}>";  
    }

    public sealed record ExpressionStmtCase( Expr Expression ) : Stmt {  
        public override string ToString() => $"<S: Expr = {Expression}>";  
    }

    public sealed record VarUpdateCase( string Name, Expr Value ) : Stmt {  
        public override string ToString() => $"<S: Update = {Name}, {Value}>";  
    }

    public sealed record IfStmtCase( Expr Condition, List<Stmt> Body ) : Stmt {  
        public override string ToString() => $"<S: if = {Condition}, {Body}>";  
    }

    public sealed record FunctionDeclarationCase( string Name, 
                                                  List<string> Parameters, 
                                                  List<Stmt> Body ) : Stmt {  
        public override string ToString() => $"<S: Function = {Name}, {Parameters}, {Body}>";  
    }

    public sealed record ForLoopCase( List<Stmt> Counter, List<Stmt> Body ) : Stmt {  
        public override string ToString() => $"<S: For = {Counter}, {Body}>";  
    }

    public sealed record ReturnStmtCase( Expr Value ) : Stmt {  
        public override string ToString() => $"<S: Return = {Value}>";  
    }

    // factory methods - this is the API you actually call
    public static Stmt VarDeclaration( string name, Expr initializer ) => new VarDeclarationCase( name, initializer );
    public static Stmt ExpressionStmt( Expr expression ) => new ExpressionStmtCase( expression );
    public static Stmt VarUpdate( string name, Expr value ) => new VarUpdateCase( name, value );
    public static Stmt IfStmt( Expr condition, List<Stmt> body ) => new IfStmtCase( condition, body );
    public static Stmt FunctionDeclaration( string name, List<string> parameters, List<Stmt> body ) => 
        new FunctionDeclarationCase( name, parameters, body );
    public static Stmt ForLoop( List<Stmt> counter, List<Stmt> body ) => new ForLoopCase( counter, body );
    public static Stmt ReturnStmt( Expr value ) => new ReturnStmtCase( value );
}


public abstract record Instruction {
    // Private constructor: no code outside this file can subclass Instruction,
    // because you can't call a private constructor from outside.
    // Nested types CAN call it (C# lets nested types see the outer type's privates).
    private Instruction() { }

    /// Stack ///
    public sealed record HALT() : Instruction {  public override string ToString() => "<I: HALT>";  }

    /// Stack ///
    public sealed record PushInt( int Value ) : Instruction {  public override string ToString() => $"<I: Push {Value}>";  }

    /// Unary Ops ///
    public sealed record Increment : Instruction {  public override string ToString() => "<I: Incr>";  }
    public sealed record Decrement : Instruction {  public override string ToString() => "<I: Decr>";  }
    
    /// Binary Ops ///
    public sealed record Add /*---*/ : Instruction {  public override string ToString() => "<I: Add>";  }
    public sealed record Sub /*---*/ : Instruction {  public override string ToString() => "<I: Sub>";  }
    public sealed record Mul /*---*/ : Instruction {  public override string ToString() => "<I: Mul>";  }
    public sealed record Div /*---*/ : Instruction {  public override string ToString() => "<I: Div>";  }
    public sealed record Exp /*---*/ : Instruction {  public override string ToString() => "<I: Exp>";  }
    public sealed record LessThan    : Instruction {  public override string ToString() => "<I: LessThan>";  }
    public sealed record GreaterThan : Instruction {  public override string ToString() => "<I: GreaterThan>";  }

    /// Function / Context / Loop ///
    public sealed record LoadLocal( int? Slot ) /*------------*/ : Instruction {  public override string ToString() => $"<I: Load {Slot}>";  }
    public sealed record StoreLocal( int? Slot ) /*-----------*/ : Instruction {  public override string ToString() => $"<I: Store {Slot}>";  }
    public sealed record JumpIfFalse( int? Target ) /*--------*/ : Instruction {  public override string ToString() => $"<I: Jump If False to {Target}>";  }
    public sealed record Jump( int? Target ) /*---------------*/ : Instruction {  public override string ToString() => $"<I: Jump to {Target}>";  }
    public sealed record CallFunction( int? Address, int Arity ) : Instruction {  public override string ToString() => $"<I: Call {Address}>";  }
    
    public sealed record Return : Instruction {  public override string ToString() => "<I: Return>";  }
}


public class CallFrame{
    public int? /*-*/ returnAddress = null;
    public List<int?> locals /*--*/ = [];
}


public record class CompileResult(
    List<Instruction> Instructions
);


}

