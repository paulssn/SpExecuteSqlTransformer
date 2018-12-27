using System;
using System.Collections.Generic;
using SpExecuteSqlTransformer.Core.Lexer;
using System.Linq;

namespace SpExecuteSqlTransformer.Core.Parser
{
    public class UnexpectedTokenException : Exception
    {
        public UnexpectedTokenException(TokenType[] expectedTokens, Token actualToken)
        {
            this.ExpectedTokens = expectedTokens;
            this.ActualToken = actualToken;            
        }

        public override string Message => $"Unexpected token at {ActualToken.Start}. Expected: {string.Join(", ", ExpectedTokens)}. Actual: {ActualToken.Type}.";

        public TokenType[] ExpectedTokens { get; set; }
        public Token ActualToken { get; set; }
    }

    public class NoTokensLeftException : Exception
    {
        public NoTokensLeftException(TokenType[] expectedTokens)
        {
            this.ExpectedTokens = expectedTokens;

        }
        public TokenType[] ExpectedTokens { get; set; }
    }

    public class TokenParser
    {
        public Stack<Token> TokenStack { get; set; }

        public TokenParser(List<Token> tokenList)
        {
            var reversedList = new List<Token>(tokenList);
            reversedList.Reverse();
            TokenStack = new Stack<Token>(reversedList);
        }

        public ParseResult Parse()
        {
            ParseResult result = null;
            try
            {
                ExpectOptional(TokenType.ExecKeyWord);
                ExpectOptional(TokenType.ExecuteSqlKeyword);
                result = new ParseResult(ExpectString().StringRepresentation);

                //we may be done here, in case we have only a statement without parameters
                if (NoTokensLeft)
                    return result;

                //in case we have only the statement and it's terminated with a semicolon
                ExpectOptional(TokenType.Semicolon);

                //again, we may be done here
                if (NoTokensLeft)
                    return result;

                //the string with sql parameter declarations; no need to do anything with it at the moment
                //just acknowledge it exists
                Expect(TokenType.Comma);
                Expect(TokenType.StringLiteral);

                var parameters = ParseParameters();
                result.Parameters.AddRange(parameters);

                return result;
            }
            catch (Exception e)
            {
                if (result == null)
                    result = new ParseResult(string.Empty);
                result.Exception = e;
                result.HasError = true; 
                return result;
            }            
        }

        private List<Parameter> ParseParameters()
        {
            var parameters = new List<Parameter>();
            while (TokensLeft)
            {
                var parameter = ExpectParameter();
                parameters.Add(parameter);
                ExpectOptional(TokenType.Semicolon);
            }
            return parameters;
        }

        private Parameter ExpectParameter()
        {
            Expect(TokenType.Comma);
            var variableName = Expect(TokenType.Variable);
            Expect(TokenType.Equals);
            var variableValue = Expect(TokenType.Variable, TokenType.Word, TokenType.StringLiteral);
            return new Parameter(variableName.StringRepresentation, variableValue.StringRepresentation);
        }

        private Token ExpectString() => Expect(TokenType.StringLiteral);

        private Token ExpectOptional(TokenType tokenType)
        {
            if (NoTokensLeft)
                return null;

            if (TokenStack.Peek().Type == tokenType)
            {
                return TokenStack.Pop();
            }
            return null;
        }

        private Token Expect(params TokenType[] tokenTypes)
        {
            if (NoTokensLeft)
                throw new NoTokensLeftException(tokenTypes);

            if (tokenTypes.Any(t => t == TokenStack.Peek().Type))
            {
                return TokenStack.Pop();
            }
            throw GetUnexpectedTokenException(tokenTypes, TokenStack.Peek());
        }

        private bool NoTokensLeft => !TokensLeft;

        private bool TokensLeft => TokenStack.Count > 0;

        private Exception GetUnexpectedTokenException(TokenType[] expectedTokenTypes, Token actualToken)
        {
            return new UnexpectedTokenException(expectedTokenTypes, actualToken);
        }
    }
}