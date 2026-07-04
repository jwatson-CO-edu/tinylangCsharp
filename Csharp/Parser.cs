namespace tlCsharp{


public class Parser ( List<Token> tokens_, bool shouldLog_ ) {
    protected List<Token> tokens /*----*/ = tokens_;
    protected bool /*--*/ shouldLog /*-*/ = shouldLog_;
    protected int /*---*/ currentPosition = 0;


    public void Log( string stmt ) {
        if( shouldLog ){  Console.WriteLine( stmt );  }
    }


    public Parse(): List<Stmt> {
        log("parse() Top level public parse function called")
        val statements: MutableList<Stmt> = mutableListOf()
        while (!isAtEnd()) {
        statements.add(parseTopLevelStatement())
        }

        log("parse() done parsing expression, verifying EOF exists")
        consume(TokenType.END_OF_FILE, "Expected EOF to terminate the program")

        return statements
    }


}



}