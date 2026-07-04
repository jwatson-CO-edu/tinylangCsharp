
namespace tlCsharp{

/// <summary>
/// Separate an input string into `Tokens`s
/// </summary>
public class Lexer( string input_ ) {

    protected string /**/ input = input_;
    protected List<Token> tokens /*----*/ = [];
    protected int /*---*/ currentPosition = 0;

    /// <summary>
    /// Return current char and advance the cursor by 1 character
    /// </summary>
    protected char Advance() {
        char returnCharacter = input[ currentPosition ];
        currentPosition += 1;
        return returnCharacter;
    }

    
    /// <summary>
    /// Return true if the cursor 
    /// </summary>
    protected bool IsAtEnd() {
        return currentPosition >= input.Length;
    }

    
    protected char Peek() {
        if( IsAtEnd() ){  return 'd';  }
        return input[currentPosition];
    }


    protected void ScanNumber() {
        int startingPositionOfNumber = currentPosition - 1;
        while( char.IsDigit( Peek() ) ){  Advance();  }
        tokens.Add( new Token( TokenType.NUMBER, input[ startingPositionOfNumber .. currentPosition ] ) );
    }

    
    protected void ScanIdentifier() {
        int startingPosition = currentPosition - 1;
        
        while( char.IsLetter( Peek() ) ){  Advance();  }
        
        string literal = input[ startingPosition .. currentPosition ];
        
        if (literal == "fun") {
           tokens.Add( new Token( TokenType.FUN ) );
        }else if( literal == "invoke" ){
            tokens.Add( new Token( TokenType.INVOKE ) );
        }else if( literal == "return" ){
            tokens.Add( new Token( TokenType.RETURN ) );
        }else if( literal == "if" ){
            tokens.Add( new Token( TokenType.IF ) );
        }else if( literal == "let" ){
            tokens.Add( new Token( TokenType.LET ) );
        }else if( literal == "update" ){
            tokens.Add( new Token( TokenType.UPDATE ) );
        }else if( literal == "to" ){
            tokens.Add( new Token(TokenType.TO ) );
        }else{
            tokens.Add( new Token( TokenType.IDENTIFIER, literal ) );
        }
    }


    protected void ScanNextToken() {
        char currentCharacter = Advance();

        if( char.IsDigit( currentCharacter ) ){
            ScanNumber();
        }else if( char.IsLetter( currentCharacter ) ){
            ScanIdentifier();
        }else if( char.IsWhiteSpace( currentCharacter ) ){
            // PASS
        }else{
            switch( currentCharacter ){
                case ',':
                    tokens.Add( new Token( TokenType.COMMA ) );
                    break;
                case '{':
                    tokens.Add( new Token( TokenType.OPEN_BRACE ) );
                    break;
                case '}':
                    tokens.Add( new Token( TokenType.CLOSE_BRACE ) );
                    break;
                case '=':
                    tokens.Add( new Token( TokenType.EQUAL ) );
                    break;
                case ';':
                    tokens.Add( new Token( TokenType.SEMICOLON ) );
                    break;
                case '(':
                    tokens.Add( new Token( TokenType.OPEN_PARENTHESIS ) );
                    break;
                case ')':
                    tokens.Add( new Token( TokenType.CLOSE_PARENTHESIS ) );
                    break;
                case '+':
                    tokens.Add( new Token( TokenType.PLUS ) );
                    break;
                case '-':
                    tokens.Add( new Token( TokenType.MINUS ) );
                    break;
                case '*':
                    tokens.Add( new Token( TokenType.STAR ) );
                    break;
                case '/':
                    tokens.Add( new Token( TokenType.SLASH ) );
                    break;
                case '>':
                    tokens.Add( new Token( TokenType.GREATER_THAN ) );
                    break;
                case '<':
                    tokens.Add( new Token( TokenType.LESS_THAN ) );
                    break;
                default:
                    throw new ArgumentException( $"Unexpected token: {currentCharacter}" );
            }
        }        
    }


    public List<Token> ScanTokens() {
        while( !IsAtEnd() ){  ScanNextToken();  }
        tokens.Add( new Token( TokenType.END_OF_FILE ) );
        return tokens;
    }

}



}

