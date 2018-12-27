using SpExecuteSqlTransformer.Core.Parser;
using System.Linq;
using System.Text;

namespace SpExecuteSqlTransformer.Core.Manipulators
{
    public class ParamReplacer : IManipulator
    {
        public string Manipulate(ParseResult parseResult, bool previousManipulatorsSuccessful, string currentStatement)
        {
            if (parseResult.HasError || !previousManipulatorsSuccessful)
                return currentStatement;

            var resultStatement = currentStatement;
            foreach (var parameter in parseResult.Parameters)
            {
                var currentTokenList = new Lexer.Lexer().Tokenize(resultStatement);

                //use SingleOrDefault, as we might have parameters in the parameter list which are actually not used in the statement
                var variableToReplace = currentTokenList.SingleOrDefault(t => t.Type == Lexer.TokenType.Variable
                    && t.StringRepresentation == parameter.Name);
                if (variableToReplace == null)
                    continue;

                var builder = new StringBuilder(resultStatement);
                builder.Remove(variableToReplace.Start, variableToReplace.StringRepresentation.Length);
                builder.Insert(variableToReplace.Start, parameter.Value);
                resultStatement = builder.ToString();
            }
            return resultStatement;
        }
    }
}
