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



public class Stmt { 
    public record VarDeclaration ( string Name, Expr Initializer );
    public record ExpressionStmt ( Expr Expression );
    public record VarUpdate ( string Name, Expr Value );
    public record IfStmt ( Expr Condition, List<Stmt> Body );
    public record FunctionDeclaration ( string Name, List<string> Parameters, List<Stmt> Body );
    public record ReturnStmt ( Expr Value );
}


}

