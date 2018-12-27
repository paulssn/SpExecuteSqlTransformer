using System;
using System.Collections.Generic;
using SpExecuteSqlTransformer.Core.Parser;

namespace SpExecuteSqlTransformer.Core.Manipulators
{
    public class StringUnwrapper : IManipulator
    {
        public string Manipulate(ParseResult parseResult, bool previousManipulatorsSuccessful, string currentStatement)
        {
            if (parseResult.HasError || !previousManipulatorsSuccessful)
                return currentStatement;

            const char quot = '\'';

            var stringToUnwrap = currentStatement;

            if (!(stringToUnwrap.StartsWith("'") && stringToUnwrap.Length >= 2) &&
                !(stringToUnwrap.StartsWith("N'") && stringToUnwrap.Length >= 3) || !stringToUnwrap.EndsWith("'"))
                throw new InvalidOperationException($"String \"{stringToUnwrap}\" seems to not be a valid sql string");

            var i = 0;
            var result = new List<char>();

            if (stringToUnwrap[i] == 'N')
                i++;
            i++;
            
            while (i < stringToUnwrap.Length)
            {
                var current = stringToUnwrap[i];
                if(current == quot)
                {
                    //last char?
                    if (i == stringToUnwrap.Length - 1)
                        break;

                    if (i + 1 == stringToUnwrap.Length - 1)
                        throw GetInvalidOperationException(result);

                    //escape sequence => skip char
                    if (stringToUnwrap[i + 1] == quot)
                        i += 2;
                    else
                        throw GetInvalidOperationException(result);
                }
                else
                {
                    i++;
                }
                result.Add(current);
            }

            return new string(result.ToArray());
        }

        private static InvalidOperationException GetInvalidOperationException(List<char> result)
        {
            return new InvalidOperationException("Unexpected end of string.\r\n" +
                                        $"Unwrapped string before exception was: {new string(result.ToArray())}");
        }
    }
}
