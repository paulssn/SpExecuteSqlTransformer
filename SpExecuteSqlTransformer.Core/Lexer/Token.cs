using System.Diagnostics;

namespace SpExecuteSqlTransformer.Core.Lexer
{
    public enum TokenType
    {
        NullToken,
        Comma,
        Semicolon,
        Equals,
        StringLiteral,
        Variable,
        Word,
        ExecKeyWord,
        ExecuteSqlKeyword,
        AnySpecialChar,
    }

    [DebuggerDisplay("{Type}, {StringRepresentation}")]
    public class Token
    {
        public TokenType Type { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string StringRepresentation { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }  
}