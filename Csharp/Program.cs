// See https://aka.ms/new-console-template for more information
using tlCsharp; 

static class Helpers {

    static void PrettyPrint( Expr expr, int indent ) {

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


    static void PrettyPrint( Stmt stmt, int indent ) { 
        string padding = string.Concat( Enumerable.Repeat( "    ", indent ) );
        if( stmt is Stmt.VarDeclarationCase stmtVar ){
            Console.WriteLine( $"{padding}VarDeclaration named {stmtVar.Name}" );
            PrettyPrint( stmtVar.Initializer, indent + 1)
        }else if( stmt is Stmt.ExpressionStmtCase stmtXpr ){
            Console.WriteLine( $"{padding}ExpressionStmt" );
            PrettyPrint( stmtXpr.Expression, indent + 1 );
        }else if( stmt is Case stmt ){
            // FIXME: START HERE!
        }else if( stmt is Case stmt ){
            
        }else if( stmt is Case stmt ){
            
        }else if( stmt is Case stmt ){
            
        }


        is Stmt.VarUpdate -> {
            Console.WriteLine("${padding}VarUpdate name ${stmt.name}")
            PrettyPrint(stmt.value, indent + 1)
        }
        is Stmt.IfStmt -> {
            Console.WriteLine("${padding} If statement")
            Console.WriteLine("${padding} Condition")
            prettyPrint(stmt.condition, indent + 2)
            Console.WriteLine("${padding} Body")
            stmt.body.forEach { bodyItem: Stmt -> prettyPrint(bodyItem, indent + 2) }
        }
        is Stmt.FunctionDeclaration -> {
            Console.WriteLine("${padding}FunctionDeclaration named ${stmt.name}")
            Console.WriteLine("${padding}Parameters:")
            stmt.parameters.forEach { p: String -> Console.WriteLine("${padding} $p")}
            Console.WriteLine("${padding}Body:")
            stmt.body.forEach {item: Stmt -> prettyPrint(item, indent + 2)}
        }
            is Stmt.ReturnStmt -> {
            Console.WriteLine("${padding}ReturnStmt")
            prettyPrint(stmt.value, indent + 1)
        }
    }
    } 

}

Console.WriteLine( "Hello, World!" );


string source = "6 + (4*2)/5 - 3*(4+(4*4 +5))";
// string source = "+++++";
// string source = "1+2+3+4";
// string source = "let x = 5*2; update x to x * 2; update x to x + 1; x;";
// string source = "let x = 9; if (x < 5) { update x to x + 10; let y = 5; }; x+y;";

// string source = """
// fun bump(n) {
//     let x = n + 1;
//     if (x > 5) {
//     update x to x * 2;
//     };
//     return x;
// }
// let a = invoke bump(5);
// let b = invoke bump(a);  
// update b to b + invoke bump(1);
// if (b > 20) {
//     update b to b - a;
// };
// b;
// """;






Console.WriteLine("Source = $source");
Console.WriteLine("====");

Lexer /*-*/ lexer  = new(source);
List<Token> tokens = lexer.ScanTokens();
// tokens.forEach { token: Token -> Console.WriteLine(token)}

Parser     parser  = new( tokens, shouldLog_ : false );
List<Stmt> program = parser.Parse();

foreach( Stmt currStatement in program ){  PrettyPrint(currStatement, 0);  }

program.forEach { :  -> }


val result: CompileResult = Compiler(shouldLog = false).compile(program)
Console.WriteLine("=========")
result.instructions.forEachIndexed { index: Int, instruction: Instruction  -> Console.WriteLine("[$index]$instruction") }

val finalStack: List<Int> = Machine().run(result.instructions)
Console.WriteLine("=========")
Console.WriteLine("Final stack = $finalStack")
