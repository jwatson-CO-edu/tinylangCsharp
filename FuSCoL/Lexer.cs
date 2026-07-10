
namespace fuscol{

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
    protected bool IsAtEnd() {  return currentPosition >= input.Length;  }

    
    /// <summary>
    /// Return character at cursor without advancing 
    /// </summary>
    protected char Peek() {
        if( IsAtEnd() ){  return 'd';  }
        return input[ currentPosition ];
    }


    /// <summary>
    /// Read in a Number token 
    /// </summary>
    protected void ScanNumber() {
        int startingPositionOfNumber = currentPosition - 1;
        while( char.IsDigit( Peek() ) ){  Advance();  }
        tokens.Add( new Token( TokenType.NUMBER, input[ startingPositionOfNumber .. currentPosition ] ) );
    }

    
    /// <summary>
    /// Read in an Identifier token 
    /// </summary>
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
            tokens.Add( new Token( TokenType.TO ) );
        }else if( literal == "for" ){
            tokens.Add( new Token( TokenType.FOR ) );
        }else{
            tokens.Add( new Token( TokenType.IDENTIFIER, literal ) );
        }
    }


    /// <summary>
    /// Read in a token of 2 repeated chars
    /// </summary>
    protected bool ScanDbbl( char chr, TokenType tknDbbl ) {
        int startingPosition = currentPosition - 1;
        if( Peek() == chr ){  
            Advance();
            tokens.Add( new Token( tknDbbl, input[ startingPosition .. currentPosition ] ) );
            return true;  
        }
        return false;
    }


    /// <summary>
    /// Read in one token of any type 
    /// </summary>
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
                    if( !ScanDbbl( currentCharacter, TokenType.DBBL_EQUAL ) ){
                        tokens.Add( new Token( TokenType.EQUAL ) );
                    }
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
                    if( !ScanDbbl( currentCharacter, TokenType.DBBL_PLUS ) ){
                        tokens.Add( new Token( TokenType.PLUS ) );
                    }
                    break;
                case '-':
                    if( !ScanDbbl( currentCharacter, TokenType.DBBL_MINUS ) ){
                        tokens.Add( new Token( TokenType.MINUS ) );
                    }
                    break;
                case '*':
                    if( !ScanDbbl( currentCharacter, TokenType.DBBL_STAR ) ){
                        tokens.Add( new Token( TokenType.STAR ) );
                    }
                    break;
                case '/':
                    if( !ScanDbbl( currentCharacter, TokenType.DBBL_SLASH ) ){
                        tokens.Add( new Token( TokenType.SLASH ) );
                    }
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


    /// <summary>
    /// Convert `input` to a list of `Token`s 
    /// </summary>
    public List<Token> ScanTokens() {
        while( !IsAtEnd() ){  ScanNextToken();  }
        tokens.Add( new Token( TokenType.END_OF_FILE ) );
        return tokens;
    }

}


}

