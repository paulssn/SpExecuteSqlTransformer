using NFluent;
using NUnit.Framework;
using SpExecuteSqlTransformer.Core.Manipulators;
using SpExecuteSqlTransformer.Core.Parser;
using System;

namespace SpExecuteSqlTransformer.Test
{
    [TestFixture]
    public class StringUnwrapperTests
    {        
        [Test]
        [TestCase("teststring")]
        [TestCase("'teststring")]
        [TestCase("teststring'")]
        [TestCase("'")]
        [TestCase("N'")]
        public void StringWithInvalidStartOrEnd_ThrowsException(string stringToUnwrap)
        {
            Check.ThatCode(() => Unwrap(stringToUnwrap))
                .Throws<InvalidOperationException>()
                .WithMessage($"String \"{stringToUnwrap}\" seems to not be a valid sql string");
        }

        [TestCase("''", "")]
        [TestCase("N''", "")]
        [TestCase("N'asdf'", "asdf")]
        [TestCase("'asdf'", "asdf")]

        [TestCase("' ''a'' '", " 'a' ")]
        [TestCase("'a = ''asdf'''", "a = 'asdf'")]
        [TestCase("''''", "'")]
        [TestCase("''''''", "''")]
        [TestCase("' ''a'' '", " 'a' ")]

        [TestCase("N' ''a'' '", " 'a' ")]
        [TestCase("N'a = ''asdf'''", "a = 'asdf'")]
        [TestCase("N''''", "'")]
        [TestCase("N''''''", "''")]
        [TestCase("N' ''a'' '", " 'a' ")]
        public void StringUnwrappedCorrectly(string stringToUnwrap, string expectedResult)
        {
            var result = Unwrap(stringToUnwrap);

            Check.That(result).IsEqualTo(expectedResult);
        }

        [Test]
        [TestCase("'asd'f'", "asd")]
        [TestCase("'asdf''", "asdf")]
        [TestCase("'''", "")]
        [TestCase("'as'''df'", "as'")]
        public void InvalidString_ThrowsException(string stringToUnwrap, string expectedParsedString)
        {
            Check.ThatCode(() => Unwrap(stringToUnwrap))
                .Throws<InvalidOperationException>()
                .WithMessage("Unexpected end of string.\r\n" +
                    $"Unwrapped string before exception was: {expectedParsedString}");
        }

        [Test]
        public void ParseResultHasError_InputIsNotChanged()
        {
            var input = "'asdf'";
            var parseResult = new ParseResult(input)
            {
                HasError = true
            };
            var result = new StringUnwrapper().Manipulate(parseResult, true, input);
            Check.That(result).IsEqualTo(input);
        }

        [Test]
        public void PreviousManipulatorsNotSuccessful_InputIsNotChanged()
        {
            var input = "'asdf'";
            var parseResult = new ParseResult(input);
            var result = new StringUnwrapper().Manipulate(parseResult, false, input);
            Check.That(result).IsEqualTo(input);
        }

        private object Unwrap(string stringToUnwrap)
        {
            var parseResult = new ParseResult(stringToUnwrap);
            return Unwrap(parseResult, stringToUnwrap);
        }

        private string Unwrap(ParseResult parseResult, string currentString)
        {
            var unwrapper = new StringUnwrapper();
            return unwrapper.Manipulate(parseResult, true, currentString);
        }
    }    
}
