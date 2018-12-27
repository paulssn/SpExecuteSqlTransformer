using NUnit.Framework;
using SpExecuteSqlTransformer.Core.Lexer;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void Tokenize()
        {
            var input = @"exec sp_executesql 
                        N'UPDATE [User] SET [Name] = @Name WHERE [ID] = @ID AND LoginCount = @LoginCount AND Location = @Location;'
                        ,
                        N'@ID uniqueidentifier,@Name nvarchar(100),@DateOfBirth datetime,@LoginCount int,@Location nvarchar(255)'
                        , 
                        @ID='00000002-0000-0000-0000-000000000002',@Name='Alex',@LoginCount=42,@Location=@1";

            var lexer = new Lexer();
            var tokenList = lexer.Tokenize(input);

            Assert.That(tokenList.Count, Is.EqualTo(21));
            Assert.That(tokenList[0].Type, Is.EqualTo(TokenType.ExecKeyWord));
            Assert.That(tokenList[0].StringRepresentation, Is.EqualTo("exec"));
            Assert.That(tokenList[1].Type, Is.EqualTo(TokenType.ExecuteSqlKeyword));
            Assert.That(tokenList[1].StringRepresentation, Is.EqualTo("sp_executesql"));
            Assert.That(tokenList[2].Type, Is.EqualTo(TokenType.StringLiteral));
            Assert.That(tokenList[2].StringRepresentation, Is.EqualTo("N'UPDATE [User] SET [Name] = @Name WHERE [ID] = @ID AND LoginCount = @LoginCount AND Location = @Location;'"));
            Assert.That(tokenList[3].Type, Is.EqualTo(TokenType.Comma));
            Assert.That(tokenList[3].StringRepresentation, Is.EqualTo(","));
            Assert.That(tokenList[4].Type, Is.EqualTo(TokenType.StringLiteral));
            Assert.That(tokenList[4].StringRepresentation, Is.EqualTo("N'@ID uniqueidentifier,@Name nvarchar(100),@DateOfBirth datetime,@LoginCount int,@Location nvarchar(255)'"));
            Assert.That(tokenList[5].Type, Is.EqualTo(TokenType.Comma));
            Assert.That(tokenList[5].StringRepresentation, Is.EqualTo(","));
            Assert.That(tokenList[6].Type, Is.EqualTo(TokenType.Variable));
            Assert.That(tokenList[6].StringRepresentation, Is.EqualTo("@ID"));
            Assert.That(tokenList[7].Type, Is.EqualTo(TokenType.Equals));
            Assert.That(tokenList[7].StringRepresentation, Is.EqualTo("="));
            Assert.That(tokenList[8].Type, Is.EqualTo(TokenType.StringLiteral));
            Assert.That(tokenList[8].StringRepresentation, Is.EqualTo("'00000002-0000-0000-0000-000000000002'"));
            Assert.That(tokenList[9].Type, Is.EqualTo(TokenType.Comma));
            Assert.That(tokenList[9].StringRepresentation, Is.EqualTo(","));
            Assert.That(tokenList[10].Type, Is.EqualTo(TokenType.Variable));
            Assert.That(tokenList[10].StringRepresentation, Is.EqualTo("@Name"));
            Assert.That(tokenList[11].Type, Is.EqualTo(TokenType.Equals));
            Assert.That(tokenList[11].StringRepresentation, Is.EqualTo("="));
            Assert.That(tokenList[12].Type, Is.EqualTo(TokenType.StringLiteral));
            Assert.That(tokenList[12].StringRepresentation, Is.EqualTo("'Alex'"));
            Assert.That(tokenList[13].Type, Is.EqualTo(TokenType.Comma));
            Assert.That(tokenList[13].StringRepresentation, Is.EqualTo(","));
            Assert.That(tokenList[14].Type, Is.EqualTo(TokenType.Variable));
            Assert.That(tokenList[14].StringRepresentation, Is.EqualTo("@LoginCount"));
            Assert.That(tokenList[15].Type, Is.EqualTo(TokenType.Equals));
            Assert.That(tokenList[15].StringRepresentation, Is.EqualTo("="));
            Assert.That(tokenList[16].Type, Is.EqualTo(TokenType.Word));
            Assert.That(tokenList[16].StringRepresentation, Is.EqualTo("42"));
            Assert.That(tokenList[17].Type, Is.EqualTo(TokenType.Comma));
            Assert.That(tokenList[17].StringRepresentation, Is.EqualTo(","));
            Assert.That(tokenList[18].Type, Is.EqualTo(TokenType.Variable));
            Assert.That(tokenList[18].StringRepresentation, Is.EqualTo("@Location"));
            Assert.That(tokenList[19].Type, Is.EqualTo(TokenType.Equals));
            Assert.That(tokenList[19].StringRepresentation, Is.EqualTo("="));
            Assert.That(tokenList[20].Type, Is.EqualTo(TokenType.Variable));
            Assert.That(tokenList[20].StringRepresentation, Is.EqualTo("@1"));
        }

        [Test]
        public void Tokenize_SingleCharWordParameterAtTheVeryEndIsTreatedCorrectly()
        {
            var input = @"@IncomingState_InDeletion=4";
            var lexer = new Lexer();

            var tokenList = lexer.Tokenize(input);

            Assert.That(tokenList.Count, Is.EqualTo(3));
            Assert.That(tokenList[0].Type, Is.EqualTo(TokenType.Variable));
            Assert.That(tokenList[0].StringRepresentation, Is.EqualTo("@IncomingState_InDeletion"));
            Assert.That(tokenList[1].Type, Is.EqualTo(TokenType.Equals));
            Assert.That(tokenList[1].StringRepresentation, Is.EqualTo("="));
            Assert.That(tokenList[2].Type, Is.EqualTo(TokenType.Word));
            Assert.That(tokenList[2].StringRepresentation, Is.EqualTo("4"));
        }

        [Test]
        public void Tokenize_WhiteSpaceAtTheVeryEnd_DoesNotThrow()
        {
            var input = @"foo ";
            var lexer = new Lexer();

            var tokenList = lexer.Tokenize(input);

            Assert.That(tokenList.Count, Is.EqualTo(1));
            Assert.That(tokenList[0].Type, Is.EqualTo(TokenType.Word));
            Assert.That(tokenList[0].StringRepresentation, Is.EqualTo("foo"));
        }
    }
}