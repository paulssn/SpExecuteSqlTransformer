using SpExecuteSqlTransformer.Core.Parser;

namespace SpExecuteSqlTransformer.Core.Manipulators
{
    public class Formatter : IManipulator
    {
        public const string ErrorOutputPrefix = "-- [SQL_FORMATTING_ERROR] \r\n";

        public string Manipulate(ParseResult parseResult, bool previousManipulatorsSuccessful, string currentStatement)
        {
            var formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter();
            formatter.ErrorOutputPrefix = ErrorOutputPrefix;
            var formattingManager = new PoorMansTSqlFormatterLib.SqlFormattingManager(formatter);
            currentStatement = formattingManager.Format(currentStatement);
            return currentStatement;
        }
    }
}
