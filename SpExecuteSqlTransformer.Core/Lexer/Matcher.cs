namespace SpExecuteSqlTransformer.Core.Lexer
{
    public interface IMatcher
    {
        Token GetToken(ILexerStream lexerStream);
    }

    public abstract class KeywordMatcherBase : BaseMatcher
    {
        protected KeywordMatcherBase()
        {
        }

        public override Token GetToken(ILexerStream lexerStream)
        {
            var keyWordString = GetKeyWordString();
            if (MatchesExactly(lexerStream, keyWordString))
            {
                var token = new Token();
                token.Start = lexerStream.Position;
                token.End = lexerStream.Position + keyWordString.Length - 1;
                token.Type = GetTokenType();
                token.StringRepresentation = keyWordString;
                SetStreamPositionToTokenEnd(lexerStream, token);
                return token;
            }
            return GetNullToken();
        }

        protected virtual bool MatchesExactly(ILexerStream lexerStream, string keyWordString)
        {
            return lexerStream.MatchesExactly(keyWordString, MustBeTerminatedByWhiteSpaceOrSpecialChar());
        }

        protected virtual bool MustBeTerminatedByWhiteSpaceOrSpecialChar()
        {
            return true;
        }

        protected abstract TokenType GetTokenType();

        protected abstract string GetKeyWordString();
    }

    public abstract class BaseMatcher : IMatcher
    {
        protected BaseMatcher(LexerStream stream){

        }
        protected BaseMatcher(){

        }

        public abstract Token GetToken(ILexerStream lexerStream);

        protected Token GetNullToken()
        {
            var token = new Token();
            token.Type = TokenType.NullToken;
            return token;
        }

        protected Token GetErrorToken(TokenType type, string message, string stringRepresentation)
        {
            var token = new Token();
            token.HasError = true;
            token.Type = type;
            token.ErrorMessage = message;
            token.StringRepresentation = stringRepresentation;
            return token;
        }

        protected void SetStreamPositionToTokenEnd(ILexerStream lexerStream, Token token)
        {
            var positionToSet = token.End+1;
            lexerStream.MoveTo(positionToSet);
        }
    }

    public class ExecKeywordMatcher : KeywordMatcherBase
    {
        public const string Exec = "exec";
        protected override string GetKeyWordString() => Exec;

        protected override TokenType GetTokenType() => TokenType.ExecKeyWord;
    }

    public class ExecuteSqlKeywordMatcher : KeywordMatcherBase
    {
        public const string SpExecuteSql = "sp_executesql";
        protected override string GetKeyWordString() => SpExecuteSql;

        protected override TokenType GetTokenType() => TokenType.ExecuteSqlKeyword;
    }    

    public class StringMatcher : BaseMatcher
    {
        const char StringTerminationChar = '\'';
        public StringMatcher()
        {
        }

        public override Token GetToken(ILexerStream lexerStream)
        {
            var start = lexerStream.Position;
            if (!lexerStream.MatchesExactly("'", false) && !lexerStream.MatchesExactly("N'", false))
                return GetNullToken();
            
            if(lexerStream.MatchesExactly("N'", false))
                lexerStream.Move(1);
            
            try
            {
                while(lexerStream.CharsLeft)
                {
                    if(lexerStream.Next() == StringTerminationChar)
                    {
                        if(IsEscapeSequence(lexerStream))
                        {
                            if(lexerStream.CharsLeft)
                                lexerStream.Next();
                        }else
                        {
                            var token = new Token();
                            token.Type = TokenType.StringLiteral;
                            token.Start = start;
                            token.End = lexerStream.Position;
                            token.StringRepresentation = lexerStream.Substring(token.Start, token.End);
                            return token;
                        }                    	
                    }                
                }

                var stringRepresentation = lexerStream.Substring(start, lexerStream.Position);
                var errorToken = GetErrorToken(TokenType.StringLiteral, "String not terminated.", stringRepresentation);
                errorToken.Start = start;
                errorToken.End = lexerStream.Position;
                return errorToken;
            }
            finally
            {
                lexerStream.MoveToNextIfPossible();
            }
        }

        private bool IsEscapeSequence(ILexerStream stream)
        {
            return stream.IsNext(StringTerminationChar);            
        }
    }

    public abstract class PunctuationMatcher : KeywordMatcherBase
    {
        protected override bool MustBeTerminatedByWhiteSpaceOrSpecialChar() => false;
    }

    public class SemicolonMatcher : PunctuationMatcher
    {
        protected override string GetKeyWordString() => ";";

        protected override TokenType GetTokenType() => TokenType.Semicolon;
    }

    public class CommaMatcher : PunctuationMatcher
    {
        protected override string GetKeyWordString() => ",";

        protected override TokenType GetTokenType() => TokenType.Comma;
    }

    public class EqualsSignMatcher : PunctuationMatcher
    {
        protected override string GetKeyWordString() => "=";

        protected override TokenType GetTokenType() => TokenType.Equals;
    }

    public class VariableMatcher : BaseMatcher
    {
        public override Token GetToken(ILexerStream lexerStream)
        {
            if(lexerStream.MatchesExactly("@", false))
            {
                var start = lexerStream.Position;
                var token = new Token();                    
                token.Type = TokenType.Variable;
                token.Start = start;

                while(lexerStream.CharsLeft)
                {
                    if(lexerStream.IsNextWhiteSpaceOrTerminatingSpecialChar())
                    {                        
                        break;
                    }
                    lexerStream.Move(1);                    
                }
                token.End = lexerStream.Position;                        
                token.StringRepresentation = lexerStream.Substring(token.Start, token.End);
                SetStreamPositionToTokenEnd(lexerStream, token);                                        
                return token;
            }
            return GetNullToken();
        }
    }

    public class WordMatcher : BaseMatcher
    {
        public override Token GetToken(ILexerStream lexerStream)
        {
            if(lexerStream.IsWhiteSpaceOrTerminatingSpecialChar(lexerStream.Position))
                return GetNullToken();

            var start = lexerStream.Position;
            while(lexerStream.CharsLeft && !lexerStream.IsNextWhiteSpaceOrTerminatingSpecialChar())
            {
                lexerStream.Move(1);
            }
            var token = new Token();
            token.Start = start;
            token.End = lexerStream.Position;
            token.StringRepresentation = lexerStream.Substring(token.Start, token.End);
            token.Type = TokenType.Word;
            SetStreamPositionToTokenEnd(lexerStream, token);
            return token;
        }
    }

    public class AnySpecialCharMatcher : BaseMatcher
    {
        public override Token GetToken(ILexerStream lexerStream)
        {
            if (!lexerStream.IsTerminatingSpecialChar(lexerStream.Position))
                return GetNullToken();

            var token = new Token();
            token.Start = lexerStream.Position;
            token.End = lexerStream.Position;
            token.StringRepresentation = lexerStream.Substring(token.Start, token.End);
            token.Type = TokenType.AnySpecialChar;
            SetStreamPositionToTokenEnd(lexerStream, token);
            return token;
        }
    }
}