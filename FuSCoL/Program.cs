using fuscol;

Console.WriteLine( "Hello, World!" );


// string source = "** ++ --";
// string source = "**++--";
// string source = "** ++ -- * + -";
// string source = "**++--*+-";
// string source = "2**3;";
// string source = "(1+1)**(3*1);";
string source = "for( let ii = 0; ii < 5; ++ii; ){  2 ** ii;  };";

FuSCoL.RunProgSource( source );