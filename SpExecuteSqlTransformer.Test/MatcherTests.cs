using NUnit.Framework;
using SpExecuteSqlTransformer.Core.Lexer;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class MatcherTests
    {
        [Test]
        [TestCase("test", "test", true)]
        [TestCase("test;", "test", true)]
        [TestCase("test,", "test", true)]
        [TestCase("test=", "test", true)]
        [TestCase("test ", "test", true)]
        [TestCase(";", ";", true)]
        [TestCase(";;", ";", true)]
        [TestCase(" test", "test", false)]
        public void LexerStream_MatchesExactly_WorksCorrectly(string input, string matchString, bool expectedResult)
        {
            var stream = new LexerStream(input);
            var result = stream.MatchesExactly(matchString, true);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ExecKeyWordMatcher_MatchesAtBeginning()
        {
            var stream = new LexerStream("exec");
            var matcher = new ExecKeywordMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.ExecKeyWord));
            Assert.That(result.StringRepresentation, Is.EqualTo("exec"));
            Assert.That(result.Start, Is.EqualTo(0));
            Assert.That(result.End, Is.EqualTo(3));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [Test]
        public void ExecKeyWordMatcher_MatchesInTheMiddleOfString()
        {
            var stream = new LexerStream("foo exec bar");
            stream.MoveTo(4);
            var matcher = new ExecKeywordMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.ExecKeyWord));
            Assert.That(result.StringRepresentation, Is.EqualTo("exec"));
            Assert.That(result.Start, Is.EqualTo(4));
            Assert.That(result.End, Is.EqualTo(7));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [Test]
        public void ExecuteSqlKeyWordMatcher()
        {
            var stream = new LexerStream("sp_executesql");
            var matcher = new ExecuteSqlKeywordMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.ExecuteSqlKeyword));
            Assert.That(result.StringRepresentation, Is.EqualTo("sp_executesql"));
            Assert.That(result.Start, Is.EqualTo(0));
            Assert.That(result.End, Is.EqualTo(12));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [Test]
        [TestCase("''")]
        [TestCase("N''")]
        [TestCase("N'asdf'")]
        [TestCase("'asdf'")]
        [TestCase("'a''sdf'")]
        [TestCase("''''")]
        [TestCase("'asdf' ", "'asdf'")]
        [TestCase("N'asdf' ", "N'asdf'")]
        [TestCase("N'asdf", "N'asdf", 0, true)]
        [TestCase("foo N'asdf' bar", "N'asdf'", 4)]
        [TestCase("foo N'asdf bar", "N'asdf bar", 4, true)]
        [TestCase("foo 'asdf', bar", "'asdf'", 4)]
        public void StringMatcher(
            string input, 
            string expectedStringRepresentation = null, 
            int startPosition = 0, 
            bool hasError = false
            )
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);
            var matcher = new StringMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.StringLiteral));
            string expectedString = expectedStringRepresentation ?? input;     
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.HasError, Is.EqualTo(hasError));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition+expectedString.Length-1));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [TestCase("@test")]
        [TestCase("@test=" , "@test")]
        [TestCase("@test asdf" , "@test")]
        [TestCase("asdf @test" , "@test", 5)]
        [TestCase("asdf @test=" , "@test", 5)]
        [TestCase("asdf @test;" , "@test", 5)]
        [TestCase("asdf @test asdf" , "@test", 5)]
        public void VariableMatcherTest(string input, string expectedStringRepresentation = null, int startPosition = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);
            var matcher = new VariableMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.Variable));
            string expectedString = expectedStringRepresentation ?? input;
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition+expectedString.Length-1));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [TestCase(" ")]
        [TestCase("asdf")]
        [TestCase("  asdf", 2)]
        public void VariableMatcher_NoMatch(string input, int position = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(position);
            var matcher = new VariableMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.NullToken));            
            AssertStreamAtCorrectPosition(stream, result, position);
        }

        [TestCase(",")]
        [TestCase(",asdf", ",")]
        [TestCase(", ", ",")]
        [TestCase(" ," , ",", 1)]
        [TestCase(",," , ",", 0)]
        [TestCase(",," , ",", 1)]
        public void CommaMatcher(string input, string expectedStringRepresentation = null, int startPosition = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);
            var matcher = new CommaMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.Comma));
            string expectedString = expectedStringRepresentation ?? input;
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition+expectedString.Length-1));
            AssertStreamAtCorrectPosition(stream, result);
        }


        [TestCase("=")]
        [TestCase("=asdf")]
        [TestCase("= ")]
        [TestCase(" =", 1)]
        [TestCase(" = ", 1)]
        [TestCase(" =asdf", 1)]
        [TestCase("==" , 0)]
        [TestCase("==" ,1)]
        public void EqualsSignMatcher(string input, int startPosition = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);
            var matcher = new EqualsSignMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.Equals));
            string expectedString = "=";
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition+expectedString.Length-1));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [TestCase(";")]
        [TestCase(";asdf")]
        [TestCase("; ")]
        [TestCase(" ;", 1)]
        [TestCase(" ; ", 1)]
        [TestCase(" ;asdf", 1)]
        [TestCase(";;" , 0)]
        [TestCase(";;" ,1)]
        public void SemicolonMatcher(string input, int startPosition = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);
            var matcher = new SemicolonMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.Semicolon));
            string expectedString = ";";
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition+expectedString.Length-1));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [TestCase("1234")]
        [TestCase("adsf")]
        [TestCase("asdf;", "asdf")]
        [TestCase("asdf=", "asdf")]
        [TestCase("asdf ", "asdf")]
        [TestCase("  asdf ", "asdf", 2)]
        [TestCase("  asdf; ", "asdf", 2)]
        [TestCase("@foo=4", "4", 5)]
        public void WordMatcher(string input, string expectedStringRepresentation = null, int startPosition = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);
            var matcher = new WordMatcher();
            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.Word));
            string expectedString = expectedStringRepresentation ?? input;
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition+expectedString.Length-1));
            AssertStreamAtCorrectPosition(stream, result);
        }

        [TestCase(")")]
        [TestCase("(")]
        [TestCase("asdf)", ")", 4)]
        public void AnySpecialCharMatcher(string input, string expectedStringRepresentation = null, int startPosition = 0)
        {
            var stream = new LexerStream(input);
            stream.MoveTo(startPosition);

            var matcher = new AnySpecialCharMatcher();

            var result = matcher.GetToken(stream);
            Assert.That(result.Type, Is.EqualTo(TokenType.AnySpecialChar));
            string expectedString = expectedStringRepresentation ?? input;
            Assert.That(result.StringRepresentation, Is.EqualTo(expectedString));
            Assert.That(result.Start, Is.EqualTo(startPosition));
            Assert.That(result.End, Is.EqualTo(startPosition + expectedString.Length - 1));
            AssertStreamAtCorrectPosition(stream, result);
        }

        private void AssertStreamAtCorrectPosition(LexerStream stream, Token token, int? startPosition = null)
        {
            if(token.Type == TokenType.NullToken) //ensure position was not changed
            {
                Assert.That(startPosition, Is.Not.Null, "No start position was provided");
                Assert.That(stream.Position, Is.EqualTo(startPosition));
                return;
            }
            var expectedEnd = token.End + 1; // token.End == stream.Length - 1 ? token.End : token.End + 1;
            Assert.That(stream.Position, Is.EqualTo(expectedEnd));                        
        }
    }
}