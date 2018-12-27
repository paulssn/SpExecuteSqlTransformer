using System;
using System.Collections.Generic;

namespace SpExecuteSqlTransformer.Core.Parser
{
    public class ParseResult
    {
        public ParseResult(string sqlStatement)
        {
            Parameters = new List<Parameter>();
            SqlStatement = sqlStatement;
        }

        public string SqlStatement { get; }
        public List<Parameter> Parameters { get; }
        public bool HasError { get; set; }
        public Exception Exception { get; set; }
    }

    public class Parameter
    {
        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}