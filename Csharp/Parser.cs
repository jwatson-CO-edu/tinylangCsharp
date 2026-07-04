namespace tlCsharp{


public class Parser ( List<Token> tokens_, bool shouldLog_ ) {
    protected List<Token> tokens /*----*/ = tokens_;
    protected bool /*--*/ shouldLog /*-*/ = shouldLog_;
    protected int /*---*/ currentPosition = 0;


    public void Log( string stmt ) {
        if( shouldLog ){  Console.WriteLine( stmt );  }
    }


    protected Token Peek() {  return tokens[ currentPosition ];  }


    protected bool IsAtEnd() {  return Peek().type == TokenType.END_OF_FILE;  }


    protected Token Previous() {  return tokens[ currentPosition - 1 ];  }


    protected Token Advance() {
        if( !IsAtEnd() ){  currentPosition += 1;  }
        return Previous();
    }


    protected bool Check( TokenType type ) {
        if( IsAtEnd() ){  return type == TokenType.END_OF_FILE;  }
        return Peek().type == type;
    }


    protected Token Consume( TokenType type, string message ) {
        if( Check( type ) ){  return Advance();  }
        throw new ArgumentException( $"Unexpected token: {message}" );
    }


    protected bool Match( params TokenType[] types ) {
        foreach( TokenType type in types ){
            if( Check( type ) ){
                Advance();
                return true;
            }
        }
        return false;
    }


    private Stmt ParseFunctionDeclaration() {
        string     name = Consume( TokenType.IDENTIFIER, "Function needs a name" ).literal;
        List<Stmt> body = [];
        Consume( TokenType.OPEN_PARENTHESIS, "Expected ( in function definition" );
        List<string> parameters = [];
        while( !Check( TokenType.CLOSE_PARENTHESIS ) ){
            parameters.Add( Consume( TokenType.IDENTIFIER, "Expected identifier").literal );
            while( Match( TokenType.COMMA ) ){
                parameters.Add( Consume( TokenType.IDENTIFIER, "Expected identifier" ).literal );
            }
        }
        Consume( TokenType.CLOSE_PARENTHESIS, "Expected ) in function definition" );
        Consume( TokenType.OPEN_BRACE, "Expected {" );
        while( !Check( TokenType.CLOSE_BRACE ) && !IsAtEnd() ){  body.Add( ParseStatement() );  }
        Consume( TokenType.CLOSE_BRACE, "Expected }" );
        return Stmt.FunctionDeclaration( name, parameters, body );
    }


    private Stmt ParseTopLevelStatement() {
        if( Match( TokenType.FUN ) ){
            return ParseFunctionDeclaration();
        }
        return ParseStatement();
    }


    public List<Stmt> Parse() {
        
        Log( "parse() Top level public parse function called" );
        
        List<Stmt> statements = [];
        
        while( !IsAtEnd() ) {
            statements.add(parseTopLevelStatement())
        }

        log("parse() done parsing expression, verifying EOF exists")
        consume(TokenType.END_OF_FILE, "Expected EOF to terminate the program")

        return statements
    }


}



}