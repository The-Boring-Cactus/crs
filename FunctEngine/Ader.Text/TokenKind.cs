namespace FunctEngine.Ader.Text
{
    public enum TokenKind
    {
        Unknown,
        Word,
        Number,
        QuotedString,
        WhiteSpace,
        VariableName,
        Symbol,
        EOL,
        EOF
    }
}
