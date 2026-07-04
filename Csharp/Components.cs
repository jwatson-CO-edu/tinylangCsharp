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



public class Expr {
    public record NumberLiteral ( int Value );
    public record Binary ( Expr Left, Token Operator, Expr Right );
    public record Variable ( string Name );
    public record FunctionCall ( string Name, List<Expr> Arguments );
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


}

