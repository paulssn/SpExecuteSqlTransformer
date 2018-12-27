using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpExecuteSqlTransformer.Core.Lexer;
using SpExecuteSqlTransformer.Core.Parser;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class TokenParserTests
    {
        [Test]
        public void Parse_SimpleStatement()
        {
            var result = Parse(SimpleStatement);
            AssertSimpleStatement(result);
        }

        [Test]
        public void Parse_SimpleStatement_With_ExecuteSql()
        {
            var result = Parse(ExecuteSql, SimpleStatement);
            AssertSimpleStatement(result);
        }

        [Test]
        public void Parse_SimpleStatement_With_ExecAndExecuteSql()
        {
            var result = Parse(Exec, ExecuteSql, SimpleStatement);
            AssertSimpleStatement(result);
        }

        [Test]
        public void Parse_SimpleStatement_TerminatedWithSemicolon()
        {
            var result = Parse(SimpleStatement, Semicolon);
            AssertSimpleStatement(result);
        }

        [Test]
        public void Parse_UnexpectedToken_ResultWithErrorAndUnexpectedTokenException()
        {
            var result = Parse(SimpleStatement, SimpleStatement);

            Assert.That(result.HasError, Is.True);
            Assert.That(result.Exception, Is.Not.Null);
            Assert.That(result.Exception.GetType(), Is.EqualTo(typeof(UnexpectedTokenException)));
            Assert.That(((UnexpectedTokenException)result.Exception).ExpectedTokens.Length, Is.EqualTo(1));
            Assert.That(((UnexpectedTokenException)result.Exception).ExpectedTokens.First(), Is.EqualTo(TokenType.Comma));
            Assert.That(((UnexpectedTokenException)result.Exception).ActualToken, Is.Not.Null);
            Assert.That(((UnexpectedTokenException)result.Exception).ActualToken.Type, Is.EqualTo(TokenType.StringLiteral));
        }

        [Test]
        public void Parse_StatementWithOneParameterWithClosingSemicolon()
        {
            var result = Parse(
                SimpleStatement, 
                Comma, 
                DummyString,
                Comma,
                GetVariable("@ID"),
                EqualsSign,
                GetString("'test'"),
                Semicolon
                );
            
            Assert.That(result.HasError, Is.False);
            Assert.That(result.SqlStatement, Is.EqualTo(SimpleStatementString));
            Assert.That(result.Parameters.Count, Is.EqualTo(1));
            Assert.That(result.Parameters.First().Name, Is.EqualTo("@ID"));
            Assert.That(result.Parameters.First().Value, Is.EqualTo("'test'"));
        }

        [Test]
        public void Parse_StatementWithMultipleParametersWithDifferentTypes()
        {
            var result = Parse(
                SimpleStatement,
                Comma,
                DummyString,
                Comma,
                GetVariable("@foo"),
                EqualsSign,
                GetVariable("@f"),
                Comma,
                GetVariable("@bar"),
                EqualsSign,
                GetWord("1234"),
                Comma,
                GetVariable("@ID"),
                EqualsSign,
                GetString("N'asdf'")
                );

            Assert.That(result.HasError, Is.False);
            Assert.That(result.SqlStatement, Is.EqualTo(SimpleStatementString));
            Assert.That(result.Parameters.Count, Is.EqualTo(3));
            Assert.That(result.Parameters[0].Name, Is.EqualTo("@foo"));
            Assert.That(result.Parameters[0].Value, Is.EqualTo("@f"));
            Assert.That(result.Parameters[1].Name, Is.EqualTo("@bar"));
            Assert.That(result.Parameters[1].Value, Is.EqualTo("1234"));
            Assert.That(result.Parameters[2].Name, Is.EqualTo("@ID"));
            Assert.That(result.Parameters[2].Value, Is.EqualTo("N'asdf'"));
        }

        private void AssertSimpleStatement(ParseResult result)
        {
            Assert.That(result.HasError, Is.False);
            Assert.That(result.SqlStatement, Is.EqualTo(SimpleStatementString));
            Assert.That(result.Parameters.Count, Is.EqualTo(0));
        }

        private static Token SimpleStatement => GetString(SimpleStatementString);
        private static Token StatementWithOneParameter => GetString(StatementWithOneParameterString);
        private static Token DummyString => GetString(DummyStringString);
        private static string SimpleStatementString => "N'select * from foo'";

        private static string StatementWithOneParameterString => "N'select * from foo where ID = @ID'";
        private static string DummyStringString => "N'DummyString'";

        private static ParseResult Parse(params Token[] token)
        {
            return new TokenParser(GetTokenList(token)).Parse();
        }

        private static List<Token> GetTokenList(params Token[] token) => token.ToList();

        private static Token Exec => GetToken(TokenType.ExecKeyWord, "exec");
        private static Token ExecuteSql => GetToken(TokenType.ExecuteSqlKeyword, "sp_executesql");
        private static Token Comma => GetToken(TokenType.Comma, ",");
        private static Token Semicolon => GetToken(TokenType.Semicolon, ";");
        private static Token EqualsSign => GetToken(TokenType.Equals, "=");

        private static Token GetString(string stringRepresentation) => GetToken(TokenType.StringLiteral, stringRepresentation);
        private static Token GetWord(string stringRepresentation) => GetToken(TokenType.Word, stringRepresentation);
        private static Token GetVariable(string stringRepresentation) => GetToken(TokenType.Variable, stringRepresentation);

        private static Token[] GetStringParameter(string variableName, string variableValue)
        {
            Token token = GetString(variableValue);
            return GetParameter(variableName, token);
        }    

        private static Token[] GetWordParameter(string variableName, string variableValue)
        {
            Token token = GetWord(variableValue);
            return GetParameter(variableName, token);
        }

        private static Token[] GetVariableParameter(string variableName, string variableValue)
        {
            Token token = GetVariable(variableValue);
            return GetParameter(variableName, token);
        }

        private static Token[] GetParameter(string variableName, Token token)
        {
            return new[] { GetVariable(variableName), EqualsSign, token };
        }

        private static Token GetToken(TokenType type, string stringRepresentation)
        {
            return new Token()
            {
                Type = type,
                StringRepresentation = stringRepresentation,
            };
        }
    }
}