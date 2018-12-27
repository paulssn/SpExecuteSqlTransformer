using NUnit.Framework;
using SpExecuteSqlTransformer.Core.Manipulators;
using SpExecuteSqlTransformer.Core.Parser;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class FormatterTests
    {
        [Test]
        public void HappyPath_Format()
        {
            var input = "select * from users where name = 'Alex'";
            var parseResult = new ParseResult(input);

            var result = new Formatter().Manipulate(parseResult, true, input);

            Assert.That(result, Is.EqualTo(@"SELECT *
FROM users
WHERE NAME = 'Alex'
"
));
        }

        [Test]
        public void ParseResultIsNull_IsFormattedAnyWay()
        {
            var input = "select * from users where name = 'Alex'";
            var parseResult = new ParseResult(input);
            parseResult.HasError = true;

            var result = new Formatter().Manipulate(parseResult, true, input);

            Assert.That(result, Is.EqualTo(@"SELECT *
FROM users
WHERE NAME = 'Alex'
"
));
        }

        [Test]
        public void PreviousManipulatorsNotSuccessful_IsFormattedAnyWay()
        {
            var input = "select * from users where name = 'Alex'";

            var result = new Formatter().Manipulate(null, false, input);

            Assert.That(result, Is.EqualTo(@"SELECT *
FROM users
WHERE NAME = 'Alex'
"
));
        }

        [Test]
        public void NotASqlString_IsReturned()
        {
            var input = "foo bar";

            var result = new Formatter().Manipulate(null, false, input);

            Assert.That(result, Is.EqualTo("foo bar\r\n"));
        }

        [Test]
        public void ParsingError_HasErrorOutputPrefix()
        {
            var input = "select * from 'adsf";

            var result = new Formatter().Manipulate(null, false, input);

            Assert.That(result, Is.EqualTo(Formatter.ErrorOutputPrefix + "SELECT *\r\nFROM 'adsf'\r\n"));
        }
    }
}
