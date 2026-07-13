namespace fuscol{


public static class ListExtensions {

    public static string ToDisplayString(this List<int?> list) =>
        "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";


    public static string ToDisplayString(this List<Expr> list) =>
        "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";


    public static string ToDisplayString(this List<Stmt> list) =>
        "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";


    public static string ToDisplayString(this List<string> list) =>
        "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";
}


/// <summary>
/// Functions supporting the FuSCoL interpreter  
/// </summary>
public static class Helpers {

    /// <summary>
    /// Recurively print an expression, with indents  
    /// </summary>
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


    /// <summary>
    /// Recurively print a statement, with indents  
    /// </summary>
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
    
}