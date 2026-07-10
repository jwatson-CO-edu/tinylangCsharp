namespace fuscol{

public class FuSCoL {

    public static void RunProgSource( string source ){
        Lexer /*-*/ lexer  = new(source);
        List<Token> tokens = lexer.ScanTokens();
        foreach( Token token in tokens ){  Console.WriteLine( token );  }

        Parser     parser  = new( tokens, shouldLog_ : true );
        List<Stmt> program = parser.Parse();
        foreach( Stmt currStatement in program ){  Helpers.PrettyPrint( currStatement, 0 );  }

        Compiler /**/ compiler = new( shouldLog_ : true );
        CompileResult result   = compiler.Compile( program );

        Console.WriteLine( "=========" );

        for( int index = 0; index < result.Instructions.Count; ++index ){
            Instruction instruction = result.Instructions[ index ];
            Console.WriteLine( $"[{index}] {instruction}");
        }

        Machine    machine    = new();
        List<int?> finalStack = machine.Run( result.Instructions );

        Console.WriteLine("=========");
        Console.WriteLine( $"Final stack = ");
        foreach( int? val in finalStack ){  Console.WriteLine( val );  }
    }

}

}