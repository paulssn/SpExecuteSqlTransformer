using SpExecuteSqlTransformer.Core.Parser;

namespace SpExecuteSqlTransformer.Core.Manipulators
{
    public interface IManipulator
    {
        string Manipulate(ParseResult parseResult, bool previousManipulatorsSuccessful, string currentStatement);
    }    
}
