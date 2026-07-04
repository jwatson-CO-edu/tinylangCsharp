
namespace tlCsharp{

/// <summary>
/// Separate an input string into `Tokens`s
/// </summary>
public class Lexer( string input_ ){

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

}



}

