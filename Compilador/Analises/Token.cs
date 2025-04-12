// Token.cs
namespace Compilador.Analises
{
    public class Token
    {
        public string Lexeme { get; }
        public string Type { get; }
        public int LineNumber { get; }

        public Token(string lexeme, string type, int lineNumber)
        {
            Lexeme = lexeme;
            Type = type;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"('{Lexeme}', {Type}, {LineNumber})";
        }
    }
}