// See https://aka.ms/new-console-template for more information
using tlCsharp; 


Console.WriteLine( "Hello, World!" );


// string source = "6 + (4*2)/5 - 3*(4+(4*4 +5))";
// string source = "6 + (4*2)/5 - 3*(4+(4*4 +5));";
// string source = "+++++;";
// string source = "1+2+3+4;";
// string source = "let x = 5*2; update x to x * 2; update x to x + 1; x;";

// string source = "let x = 9; if (x < 5) { update x to x + 10; let y = 5; }; x+y;";

string source = """
fun bump(n) {
    let x = n + 1;
    if (x > 5) {
        update x to x * 2;
    };
    return x;
}
let a = invoke bump(5);
let b = invoke bump(a);  
update b to b + invoke bump(1);
if (b > 20) {
    update b to b - a;
};
b;
""";

Console.WriteLine("Source = $source");
Console.WriteLine("====");

Lexer /*-*/ lexer  = new(source);
List<Token> tokens = lexer.ScanTokens();
foreach( Token token in tokens ){  Console.WriteLine( token );  }

Parser     parser  = new( tokens, shouldLog_ : true );
List<Stmt> program = parser.Parse();

foreach( Stmt currStatement in program ){  Helpers.PrettyPrint( currStatement, 0 );  }

Compiler compiler = new( shouldLog_ : true );

CompileResult result = compiler.Compile( program );
Console.WriteLine( "=========" );

for( int index = 0; index < result.Instructions.Count; ++index ){
    Instruction instruction = result.Instructions[ index ];
    Console.WriteLine( $"[{index}] {instruction}");
}

Machine machine = new();

List<int?> finalStack = machine.Run( result.Instructions );

Console.WriteLine("=========");
Console.WriteLine( $"Final stack = ");
foreach( int? val in finalStack ){  Console.WriteLine( val );  }



public static class Helpers {

    public static void PrettyPrint( Expr expr, int indent ) {

        string padding = string.Concat( Enumerable.Repeat( "    ", indent ) );

        if( expr is Expr.NumberLiteralCase exprNum ){
            Console.WriteLine( $"{padding}Number({exprNum.Value})" );

        }else if( expr is Expr.BinaryCase exprBin ){
            Console.WriteLine( $"{padding}Binary({exprBin.Operator.type})" );
            PrettyPrint( exprBin.Left , indent + 1 );
            PrettyPrint( exprBin.Right, indent + 1);
        }else if( expr is Expr.VariableCase exprVar ){
            Console.WriteLine( $"{padding}Variable named {exprVar.Name}" );
            
        }else if( expr is Expr.FunctionCallCase exprFnc ){
            Console.WriteLine( $"{padding} FunctionCall to ${exprFnc.Name}" );
            Console.WriteLine( $"{padding} Args:");
            foreach( Expr currArg in exprFnc.Arguments ){  PrettyPrint(currArg, indent + 2);  }
        }      
    }


    public static void PrettyPrint( Stmt stmt, int indent ) { 
        string padding = string.Concat( Enumerable.Repeat( "    ", indent ) );
        if( stmt is Stmt.VarDeclarationCase stmtVar ){
            Console.WriteLine( $"{padding}VarDeclaration named {stmtVar.Name}" );
            PrettyPrint( stmtVar.Initializer, indent + 1);
        }else if( stmt is Stmt.ExpressionStmtCase stmtXpr ){
            Console.WriteLine( $"{padding}ExpressionStmt" );
            PrettyPrint( stmtXpr.Expression, indent + 1 );
        }else if( stmt is Stmt.VarUpdateCase stmtUpd ){
            Console.WriteLine( $"{padding}VarUpdate name {stmtUpd.Name}" );
            PrettyPrint( stmtUpd.Value, indent + 1 );
        }else if( stmt is Stmt.IfStmtCase stmtIfC ){
            Console.WriteLine( $"{padding} If statement" );
            Console.WriteLine( $"{padding} Condition" );
            PrettyPrint( stmtIfC.Condition, indent + 2 );
            Console.WriteLine( $"{padding} Body" );
            foreach( Stmt bodyItem in stmtIfC.Body ){  PrettyPrint( bodyItem, indent + 2 );  }
        }else if( stmt is Stmt.FunctionDeclarationCase stmtFnc ){
            Console.WriteLine( $"{padding}FunctionDeclaration named ${stmtFnc.Name}" );
            Console.WriteLine( $"{padding}Parameters:");
            foreach( string p in stmtFnc.Parameters ){  Console.WriteLine( $"{padding} {p}" );  }
            Console.WriteLine( $"{padding}Body:" );
            foreach( Stmt item in stmtFnc.Body ){  PrettyPrint( item, indent + 2 );  }
        }else if( stmt is Stmt.ReturnStmtCase stmtRtn ){
            Console.WriteLine( $"{padding}ReturnStmt" );
            PrettyPrint( stmtRtn.Value, indent + 1 );
        }
    }
} 


