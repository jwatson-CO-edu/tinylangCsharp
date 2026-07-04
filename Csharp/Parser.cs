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


    protected Expr ParseFactor() {
        
        Log( "parseFactor() called" );

        // Case 1: integer
        if( Match( TokenType.NUMBER ) ){
            Log( "parseFactor() matched a ${previous()} , will return literal" );
            return Expr.NumberLiteral( int.Parse( Previous().literal ) );
        }

        // Case 2: ( E )
        if( Match( TokenType.OPEN_PARENTHESIS ) ){
            Log( "parseFactor() matched an open parenthesis, will parse expression now" );
            Expr parsedExpression = ParseExpression();
            Log( "parseFactor() done parsing expression, checking for close parenthesis" );
            Consume( TokenType.CLOSE_PARENTHESIS, "Failed to parse a factor, expected ) following (" ); 
            return parsedExpression;
        }

        // Case 3: identifier
        if( Match( TokenType.IDENTIFIER ) ){
            return Expr.Variable( Previous().literal );
        }

        // Case 4: Function invocation
        if (match(TokenType.INVOKE)) {
            val name: String = consume(TokenType.IDENTIFIER, "Expected identifier").literal
            consume(TokenType.OPEN_PARENTHESIS, "Expected (")
            val args: MutableList<Expr> = mutableListOf()
            while(!check(TokenType.CLOSE_PARENTHESIS)) {
                args.add(parseCExpression())
                while (match(TokenType.COMMA)) {
                    args.add(parseCExpression())
                }
            }
            consume(TokenType.CLOSE_PARENTHESIS, "Expected )")
            return Expr.FunctionCall(name, args)
        }

        throw new ArgumentException( "Unable to parse factor, expected a number or open parenthesis" );
    }


    protected Expr ParseTerm() {
        Log( "parseTerm() called, will start by calling parseFactor()" );
        Expr workingExpression = ParseFactor();
        Log( "parseTerm() done parsing term, now checking for * / Term" );
        while( Match( TokenType.STAR, TokenType.SLASH ) ){
            Log( "parseTerm() found a ${previous()}, will parse term again" );
            Token Operator = Previous();
            Expr parsedFactor = ParseFactor();
            workingExpression = Expr.Binary( workingExpression, Operator, parsedFactor );
        }
        return workingExpression;
    }


    protected Expr ParseExpression() {
        Log( "parseExpression() called, will start by parsing term()" );
        Expr workingExpression = ParseTerm();
        Log( "parseExpression() done parsing term, now checking for + - Term" );
        while( Match( TokenType.PLUS, TokenType.MINUS ) ){
            Log( "parseExpression() found a ${previous()}, will parse term again" );
            Token Operator  = Previous();
            Expr parsedTerm = ParseTerm();
            // Left unfolding that maintains left-to-right execution of terms of the same precedence
            workingExpression = Expr.Binary( workingExpression, Operator, parsedTerm );
        }
        return workingExpression;
    }


    protected Expr ParseCExpression() {
        Expr workingExpression = ParseExpression();
        if( Match( TokenType.LESS_THAN, TokenType.GREATER_THAN ) ){
            Token Operator /*---*/ = Previous();
            Expr  parsedExpression = ParseExpression();
            workingExpression = Expr.Binary( workingExpression, Operator, parsedExpression );
        }
        return workingExpression;
    }


    protected Stmt ParseExpressionStatment() {
        Expr expression = ParseCExpression();
        Consume( TokenType.SEMICOLON, "Expected ; following expression statement" );
        return Stmt.ExpressionStmt( expression );
    }


    protected Stmt ParseStatement() {
        if( Match( TokenType.RETURN ) ){  return ParseReturnStatement();  }
        if( Match( TokenType.LET    ) ){  return ParseVarDeclaration();   }
        if( Match( TokenType.UPDATE ) ){  return ParseVarUpdate(); /*--*/ }
        if( Match( TokenType.IF     ) ){  return ParseIfStatement(); /**/ }
        return ParseExpressionStatment();
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
            statements.Add( ParseTopLevelStatement() );
        }

        Log( "parse() done parsing expression, verifying EOF exists" );
        
        Consume( TokenType.END_OF_FILE, "Expected EOF to terminate the program" );

        return statements;
    }


}



}