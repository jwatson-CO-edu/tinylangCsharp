namespace fuscol{

public class FuSCoL {

    public static void RunProgSource( string source, bool shouldLog = true ){
        Lexer /*-*/ lexer  = new(source);
        List<Token> tokens = lexer.ScanTokens();
        foreach( Token token in tokens ){  Console.WriteLine( token );  }

        Parser     parser  = new( tokens, shouldLog_ : shouldLog );
        List<Stmt> program = parser.Parse();
        if( shouldLog ){
            foreach( Stmt currStatement in program ){  Helpers.PrettyPrint( currStatement, 0 );  }
        }
        Compiler /**/ compiler = new( shouldLog_ : shouldLog );
        CompileResult result   = compiler.Compile( program );

        if( shouldLog ){  
            Console.WriteLine( "=========" );  
            for( int index = 0; index < result.Instructions.Count; ++index ){
                Instruction instruction = result.Instructions[ index ];
                Console.WriteLine( $"[{index}] {instruction}");
            }
        }

        Machine    machine    = new();
        List<int?> finalStack = machine.Run( result.Instructions );

        if( shouldLog ){  Console.WriteLine("=========");  }
        Console.WriteLine( $"Final stack = ");
        foreach( int? val in finalStack ){  Console.WriteLine( val );  }
    }

}

}