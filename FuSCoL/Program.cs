using fuscol;

Console.WriteLine("Hello, World!");


// string source = "** ++ --";
// string source = "**++--";
// string source = "** ++ -- * + -";
string source = "**++--*+-";

Lexer /*-*/ lexer  = new(source);
List<Token> tokens = lexer.ScanTokens();
foreach( Token token in tokens ){  Console.WriteLine( token );  }