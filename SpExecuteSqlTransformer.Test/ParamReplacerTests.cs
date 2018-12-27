using NUnit.Framework;
using SpExecuteSqlTransformer.Core.Manipulators;
using SpExecuteSqlTransformer.Core.Parser;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class ParamReplacerTests
    {
        [Test]
        public void NoParameters_NothingIsReplaced()
        {
            var resultString = Replace("select * from user");
            Assert.That(resultString, Is.EqualTo("select * from user"));
        }

        [Test]
        public void DoesNotUseStatementFromParseResult_But_FromCurrentString()
        {
            var parseResult = GetParseResult("dummyString", Param("@ID", "'1234'"));
            var currentString = "select * from Users where id = @ID";

            var resultString = Replace(parseResult, currentString, true);

            Assert.That(resultString, Is.EqualTo("select * from Users where id = '1234'"));
        }

        [Test]
        public void SingleParameter_IsReplaced()
        {
            var resultString = Replace(
                "select * from Users where id = @ID", 
                Param("@ID", "'1234'"));

            Assert.That(resultString, Is.EqualTo("select * from Users where id = '1234'"));
        }

        [Test]
        public void MultipleParameters_AreReplaced()
        {
            var resultString = Replace(
                "select * from Users where id = @ID and number = @Number and foo = @foo",
                Param("@ID", "'asdf'"),
                Param("@Number", "42"),
                Param("@foo", "@x")
                );

            Assert.That(resultString, 
                Is.EqualTo("select * from Users where id = 'asdf' and number = 42 and foo = @x"));
        }

        [Test]
        public void ParameterNameContainingOtherParametersName()
        {
            var resultString = Replace(
                "UPDATE [TestTable] SET [TestID] = @TestID, [TestIDClassID] = @TestIDClassID",
                Param("@TestID", "NULL"),
                Param("@TestIDClassID", "NULL")
                );

            Assert.That(resultString,
                Is.EqualTo("UPDATE [TestTable] SET [TestID] = NULL, [TestIDClassID] = NULL"));
        }

        [Test]
        public void ParseResultHasError_InputIsNotChanged()
        {
            var currentString = "select * from Users where id = @ID";
            var parseResult = GetParseResult("dummyString", Param("@ID", "'1234'"));
            parseResult.HasError = true;

            var resultString = Replace(parseResult, currentString, true);

            Assert.That(resultString, Is.EqualTo("select * from Users where id = @ID"));
        }

        [Test]
        public void PreviousManipulatorsNotSuccessful_InputIsNotChanged()
        {
            var parseResult = GetParseResult("dummyString", Param("@ID", "'1234'"));
            var currentString = "select * from Users where id = @ID";

            var resultString = Replace(parseResult, currentString, false);

            Assert.That(resultString, Is.EqualTo("select * from Users where id = @ID"));
        }

        private Parameter Param(string name, string value) => new Parameter(name, value);

        private string Replace(string statement, params Parameter[] parameters)
        {
            var parseResult = GetParseResult(statement, parameters);
            return Replace(parseResult, statement, true);
        }

        private string Replace(ParseResult parseResult, string currentString, bool previousManipulatorsSuccessful)
        {
            var replacer = GetReplacer();
            return replacer.Manipulate(parseResult, previousManipulatorsSuccessful, currentString);
        }

        private ParamReplacer GetReplacer() => new ParamReplacer();

        private ParseResult GetParseResult(string statement, params Parameter[] parameters)
        {
            var result = new ParseResult(statement);
            result.Parameters.AddRange(parameters);
            return result;
        }
    }
}
