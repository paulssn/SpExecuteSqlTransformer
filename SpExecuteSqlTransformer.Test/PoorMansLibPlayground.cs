using NUnit.Framework;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.Tokenizers;

namespace SpExecuteSqlTransformer.Test
{
    [Explicit("Playground for manual tests, spikes and debugging only")]
    [TestFixture]
    public class PoorMansLibPlayground
    {
        [Test]
        public void Parse()
        {
            //var input = "select * from users where id = '1234' and name = @name";

            var input = "set @p3 = convert(xml, N'<L><I>Incoming</I></L>')";

            var parser = new TSqlStandardParser();
            var tokenizer = new TSqlStandardTokenizer();
            var sqlTree = parser.ParseSQL(tokenizer.TokenizeSQL(input));            
        }
    }
}
