using Antlr4.Runtime;

namespace FunctEngine;

public class CustomErrorListener : BaseErrorListener
{
    public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
        int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Console.WriteLine($"❌ Error de sintaxis en línea {line}, columna {charPositionInLine}: {msg}");
    }
}