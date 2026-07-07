namespace tlCsharp{

public enum TokenType {
    NUMBER,
    PLUS,
    MINUS,
    STAR,
    SLASH,
    OPEN_PARENTHESIS,
    CLOSE_PARENTHESIS,
    END_OF_FILE,
    LET,
    IDENTIFIER,
    EQUAL,
    SEMICOLON,
    UPDATE,
    TO,
    LESS_THAN,
    GREATER_THAN,
    OPEN_BRACE,
    CLOSE_BRACE,
    IF,
    FUN,
    COMMA,
    RETURN,
    INVOKE
}



public class Token( TokenType type_, string lit = "" ) {
    public TokenType type    = type_;
    public string    literal = lit;
}



public abstract record Expr {
    public sealed record NumberLiteralCase ( int Value ) : Expr;
    public sealed record BinaryCase ( Expr Left, Token Operator, Expr Right ) : Expr;
    public sealed record VariableCase ( string Name ) : Expr;
    public sealed record FunctionCallCase ( string Name, List<Expr> Arguments ) : Expr;

    public static Expr NumberLiteral( int Value ) => new NumberLiteralCase( Value ); 
    public static Expr Binary( Expr Left, Token Operator, Expr Right ) => new BinaryCase( Left, Operator, Right ); 
    public static Expr Variable( string Name ) => new VariableCase( Name ); 
    public static Expr FunctionCall( string Name, List<Expr> Arguments ) => new FunctionCallCase( Name, Arguments );

}



public abstract record Stmt{

    // concrete cases - names are internal implementation detail
    public sealed record VarDeclarationCase( string Name, Expr Initializer ) : Stmt;
    public sealed record ExpressionStmtCase( Expr Expression ) : Stmt;
    public sealed record VarUpdateCase( string Name, Expr Value ) : Stmt;
    public sealed record IfStmtCase( Expr Condition, List<Stmt> Body ) : Stmt;
    public sealed record FunctionDeclarationCase( string Name, List<string> Parameters, List<Stmt> Body ) : Stmt;
    public sealed record ReturnStmtCase( Expr Value ) : Stmt;

    // factory methods - this is the API you actually call
    public static Stmt VarDeclaration( string name, Expr initializer ) => new VarDeclarationCase( name, initializer );
    public static Stmt ExpressionStmt( Expr expression ) => new ExpressionStmtCase( expression );
    public static Stmt VarUpdate( string name, Expr value ) => new VarUpdateCase( name, value );
    public static Stmt IfStmt( Expr condition, List<Stmt> body ) => new IfStmtCase( condition, body );
    public static Stmt FunctionDeclaration( string name, List<string> parameters, List<Stmt> body ) => 
        new FunctionDeclarationCase( name, parameters, body );
    public static Stmt ReturnStmt( Expr value ) => new ReturnStmtCase( value );
}


public abstract record Instruction {
    // Private constructor: no code outside this file can subclass Instruction,
    // because you can't call a private constructor from outside.
    // Nested types CAN call it (C# lets nested types see the outer type's privates).
    private Instruction() { }

    public sealed record PushInt( int Value ) : Instruction;

    public sealed record Add /*---*/ : Instruction {  public override string ToString() => "Add";  }
    public sealed record Sub /*---*/ : Instruction {  public override string ToString() => "Sub";  }
    public sealed record Mul /*---*/ : Instruction {  public override string ToString() => "Mul";  }
    public sealed record Div /*---*/ : Instruction {  public override string ToString() => "Div";  }
    public sealed record LessThan    : Instruction {  public override string ToString() => "LessThan";  }
    public sealed record GreaterThan : Instruction {  public override string ToString() => "GreaterThan";  }

    public sealed record LoadLocal( int? Slot ) : Instruction;
    public sealed record StoreLocal( int Slot ) : Instruction;
    public sealed record JumpIfFalse( int? Target ) : Instruction;
    public sealed record Jump( int? Target ) : Instruction;
    public sealed record CallFunction( int? Address, int Arity ) : Instruction;
    
    public sealed record Return : Instruction {  public override string ToString() => "Return";  }
}


public class CallFrame{
    public int? /*-*/ returnAddress = null;
    public List<int?> locals /*--*/ = [];
}


public record class CompileResult(
    List<Instruction> Instructions
);


}

