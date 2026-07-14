namespace fuscol{


/// <summary>
/// Insterpret tokens as syntax
/// </summary>
public class Parser ( List<Token> tokens_, bool shouldLog_ ) {
    protected List<Token> tokens /*----*/ = tokens_;
    protected bool /*--*/ shouldLog /*-*/ = shouldLog_;
    protected int /*---*/ currentPosition = 0;


    /// <summary>
    /// Condtionally print a `stmt` 
    /// </summary>
    public void Log( string stmt ) {  if( shouldLog ){  Console.WriteLine( stmt );  }  }


    /// <summary>
    /// Return the token at the cursor 
    /// </summary>
    protected Token Peek() {  return tokens[ currentPosition ];  }


    /// <summary>
    /// Return true if the token at the cursor is End-Of-File (EOF) 
    /// </summary>
    protected bool IsAtEnd() {  return Peek().type == TokenType.END_OF_FILE;  }


    /// <summary>
    /// Return the token immediately before the cursor 
    /// </summary>
    protected Token Previous() {  return tokens[ currentPosition - 1 ];  }


    /// <summary>
    /// Move the cursor forward one token and return the previous token 
    /// </summary>
    protected Token Advance() {
        if( !IsAtEnd() ){  currentPosition += 1;  }
        return Previous();
    }


    /// <summary>
    /// Return true if the token at the cursor has the given `type`
    /// </summary>
    protected bool Check( TokenType type ) {
        if( IsAtEnd() ){  return type == TokenType.END_OF_FILE;  }
        return Peek().type == type;
    }


    /// <summary>
    /// If the token at the cursor has the given `type`, then return it and advance, Otherwise ERROR
    /// </summary>
    protected Token Consume( TokenType type, string message ) {
        Log( $"Attempt to Consume token of type {type}: {Peek()}" );
        if( Check( type ) ){  return Advance();  }
        throw new ArgumentException( $"Unexpected token: {message}" );
    }


    /// <summary>
    /// Multiple `Advance()`: Check the next N tokens are of the given `types`, advancing if true
    /// </summary>
    protected bool Match( params TokenType[] types ) {
        foreach( TokenType type in types ){
            if( Check( type ) ){
                Advance();
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Parse an {integer, parenthetical expression, identifier, }
    /// </summary>
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
        if( Match( TokenType.IDENTIFIER ) ){  return Expr.Variable( Previous().literal );  }

        // Case 4: Function invocation
        if( Match( TokenType.INVOKE ) ){
            string     name = Consume( TokenType.IDENTIFIER, "Expected identifier" ).literal;
            List<Expr> args = [];
            Consume( TokenType.OPEN_PARENTHESIS, "Expected (" );
            while( !Check( TokenType.CLOSE_PARENTHESIS ) ){
                args.Add( ParseCompareExpression() );
                while( Match( TokenType.COMMA ) ){  args.Add( ParseCompareExpression() );  }
            }
            Consume( TokenType.CLOSE_PARENTHESIS, "Expected )" );
            return Expr.FunctionCall( name, args );
        }

        throw new ArgumentException( "Unable to parse factor, expected a number or open parenthesis" );
    }


    /// <summary>
    /// Parse power
    /// </summary>
    protected Expr ParseExponent() {
        Log( "ParseExponent() called, will start by calling parseFactor()" );
        Expr workingExpression = ParseFactor();
        Log( "ParseExponent() done parsing term, now checking for * / Term" );
        while( Match( TokenType.DBBL_STAR ) ){
            Log( "ParseExponent() found a ${previous()}, will parse term again" );
            Token Operator = Previous();
            Expr parsedFactor = ParseFactor();
            workingExpression = Expr.Binary( workingExpression, Operator, parsedFactor );
        }
        return workingExpression;
    }


    /// <summary>
    /// Parse multiplication or division
    /// </summary>
    protected Expr ParseMultDiv() {
        Log( "ParseMultDiv() called, will start by calling parseFactor()" );
        Expr workingExpression = ParseExponent();
        Log( "ParseMultDiv() done parsing term, now checking for * / Term" );
        while( Match( TokenType.STAR, TokenType.SLASH ) ){
            Log( "ParseMultDiv() found a ${previous()}, will parse term again" );
            Token Operator = Previous();
            Expr parsedFactor = ParseExponent();
            workingExpression = Expr.Binary( workingExpression, Operator, parsedFactor );
        }
        return workingExpression;
    }


    /// <summary>
    /// Parse a math expression (including function calls)
    /// </summary>
    protected Expr ParseExpression() {
        Log( "parseExpression() called, will start by parsing term()" );
        Expr workingExpression = ParseMultDiv();
        Log( "parseExpression() done parsing term, now checking for + - Term" );
        while( Match( TokenType.PLUS, TokenType.MINUS ) ){
            Log( "parseExpression() found a ${previous()}, will parse term again" );
            Token Operator  = Previous();
            Expr parsedTerm = ParseMultDiv();
            // Left unfolding that maintains left-to-right execution of terms of the same precedence
            workingExpression = Expr.Binary( workingExpression, Operator, parsedTerm );
        }
        return workingExpression;
    }


    /// <summary>
    /// Parse a comparison expression (including math expressions)
    /// </summary>
    protected Expr ParseCompareExpression() {
        Expr workingExpression = ParseExpression();
        if( Match( TokenType.LESS_THAN, TokenType.GREATER_THAN ) ){
            Token Operator /*---*/ = Previous();
            Expr  parsedExpression = ParseExpression();
            workingExpression = Expr.Binary( workingExpression, Operator, parsedExpression );
        }
        return workingExpression;
    }


    /// <summary>
    /// Parse a statement that is a math expression
    /// </summary>
    protected Stmt ParseExpressionStatment() {
        Expr expression = ParseCompareExpression();
        Consume( TokenType.SEMICOLON, "Expected ; following expression statement" );
        return Stmt.ExpressionStmt( expression );
    }


    /// <summary>
    /// Parse a return statement
    /// </summary>
    protected Stmt ParseReturnStatement() {
        Expr value = ParseCompareExpression();
        Consume( TokenType.SEMICOLON, "Expected ;" );
        return Stmt.ReturnStmt( value );
    }


    /// <summary>
    /// Parse a variable declaration statement (including math expression)
    /// </summary>
    protected Stmt ParseVarDeclaration() {
        Log( "Entered var declaration!" );
        string name = Consume( TokenType.IDENTIFIER, "Expect a name for a variable declaration" ).literal;
        Consume( TokenType.EQUAL, "Expected = sign after variable name" );
        Expr initializer = ParseExpression();
        Consume( TokenType.SEMICOLON, "Expected ; following variable declaration" );
        return Stmt.VarDeclaration( name, initializer );
    }


    /// <summary>
    /// Parse a variable value update
    /// </summary>
    protected Stmt ParseVarUpdate() {
        string name = Consume( TokenType.IDENTIFIER, "must specify variable name to update" ).literal;
        Consume( TokenType.TO, "Expected literal 'to'" );
        Expr value = ParseExpression();
        Consume( TokenType.SEMICOLON, "Expected semicolon to end statement" );
        return Stmt.VarUpdate( name, value );
    }


    /// <summary>
    /// Parse a condtional statement
    /// </summary>
    protected Stmt ParseIfStatement() {
        Consume( TokenType.OPEN_PARENTHESIS, "Expected ( after if" );
        Expr condition = ParseCompareExpression();
        Consume( TokenType.CLOSE_PARENTHESIS, "Expected ) after if" ); 
        Consume( TokenType.OPEN_BRACE, "Expected { after )" );
        List<Stmt> body = [];
        while( !Check( TokenType.CLOSE_BRACE ) && !IsAtEnd() ){  body.Add( ParseStatement() );  }
        Consume( TokenType.CLOSE_BRACE, "Expected } to end if statement body" );
        Consume( TokenType.SEMICOLON, "Expected ; to end if statement" );
        return Stmt.IfStmt( condition, body );
    }


    /// <summary>
    /// Parse a `for` loop
    /// </summary>
    protected Stmt ParseForLoop() {

        Consume( TokenType.OPEN_PARENTHESIS, "Expected ( after if" );
        Consume( TokenType.LET, "Expected keyword `let`" );
        Stmt init = ParseVarDeclaration();
        Stmt test = ParseStatement();
        Stmt incr = ParseStatement();
        Consume( TokenType.CLOSE_PARENTHESIS, "Expected ) after if" );
        List<Stmt> counter = [init, test, incr,]; 
        Consume( TokenType.OPEN_BRACE, "Expected { after )" );
        List<Stmt> body = [];
        while( !Check( TokenType.CLOSE_BRACE ) && !IsAtEnd() ){  body.Add( ParseStatement() );  }
        Consume( TokenType.CLOSE_BRACE, "Expected } to end if statement body" );
        Consume( TokenType.SEMICOLON, "Expected ; to end if statement" );
        return Stmt.ForLoop( counter, body );
    }


    /// <summary>
    /// Parse a statement
    /// </summary>
    protected Stmt ParseStatement() {
        if( Match( TokenType.RETURN ) ){  return ParseReturnStatement();  }
        if( Match( TokenType.LET    ) ){  return ParseVarDeclaration();   }
        if( Match( TokenType.UPDATE ) ){  return ParseVarUpdate(); /*--*/ }
        if( Match( TokenType.IF     ) ){  return ParseIfStatement(); /**/ }
        if( Match( TokenType.FOR    ) ){  return ParseForLoop(); /*----*/ }
        return ParseExpressionStatment();
    }


    /// <summary>
    /// Parse a function declaration
    /// </summary>
    protected Stmt ParseFunctionDeclaration() {
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


    /// <summary>
    /// Parse any statement
    /// </summary>
    protected Stmt ParseTopLevelStatement() {
        if( Match( TokenType.FUN ) ){  return ParseFunctionDeclaration();  }
        return ParseStatement();
    }


    /// <summary>
    /// Top-level public function that runs the parser
    /// </summary>
    public List<Stmt> Parse() {
        List<Stmt> statements = [];
        Log( "parse() Top level public parse function called" );
        while( !IsAtEnd() ){  statements.Add( ParseTopLevelStatement() );  }
        Log( "parse() done parsing expression, verifying EOF exists" );
        Consume( TokenType.END_OF_FILE, "Expected EOF to terminate the program" );
        return statements;
    }


}



}