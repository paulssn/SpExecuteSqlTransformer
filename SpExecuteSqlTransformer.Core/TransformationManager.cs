using log4net;
using SpExecuteSqlTransformer.Core.Lexer;
using SpExecuteSqlTransformer.Core.Manipulators;
using SpExecuteSqlTransformer.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpExecuteSqlTransformer.Core
{
    public class TransformationResult
    {
        public TransformationResult()
        {
            ManipulatorResults = new List<ManipulatorResult>();
        }

        public string ResultString { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public List<Token> TokenList { get; set; }
        public ParseResult ParseResult { get; set; }
        public List<ManipulatorResult> ManipulatorResults { get; set; }
    }

    public class ManipulatorResult
    {
        public ManipulatorResult(Type manipulatorType, string inputString)
        {
            ManipulatorType = manipulatorType;
            InputString = inputString;
        }

        public Type ManipulatorType { get; set; }
        public bool HasError { get; set; }
        public Exception Exception { get; set; }
        public string InputString { get; set; }
        public string ResultString { get; set; }
    }

    public class TransformationManager : ITransformationManager
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<IManipulator> Manipulators { get; }

        public TransformationManager(IEnumerable<IManipulator> manipulators)
        {
            Manipulators = manipulators.ToList();
        }
        
        public TransformationResult TransformSqlString(string inputSqlString)
        {
            if (inputSqlString == null)
                throw new ArgumentNullException(nameof(inputSqlString));

            var result = new TransformationResult();

            log.Debug("inputSqlString: \r\n" + inputSqlString);

            //Lexing stage
            List <Token> tokenList = null;
            try
            {
                tokenList = new Lexer.Lexer().Tokenize(inputSqlString);
            }
            catch (Exception e)
            {
                log.Debug("Lexing (tokenizing) failed.", e);

                result.ErrorMessage = "Lexing (tokenizing) failed.";
                result.Exception = e;
            }
            result.TokenList = tokenList;

            //Parsing stage
            ParseResult parseResult = null;
            if (tokenList != null)
            {
                parseResult = new TokenParser(tokenList).Parse();
                result.ParseResult = parseResult;
                if (parseResult.HasError)
                {
                    log.Debug("Parsing failed.", parseResult.Exception);

                    result.ErrorMessage = "Parsing failed.";
                    result.Exception = parseResult.Exception;
                }
            }

            //String manipulation stage
            var currentSqlStatement = !parseResult.HasError ? parseResult.SqlStatement : inputSqlString;
            foreach (var manipulator in Manipulators)
            {
                var manipulatorResult = new ManipulatorResult(manipulator.GetType(), currentSqlStatement);

                try
                {
                    var previousManipulatorsSuccessful = result.ManipulatorResults.All(r => !r.HasError);
                    currentSqlStatement = manipulator.Manipulate(parseResult, previousManipulatorsSuccessful, currentSqlStatement);
                    manipulatorResult.ResultString = currentSqlStatement;
                }
                catch (Exception e)
                {
                    log.Warn($"Manipulator '{manipulator}' failed.", e);
                    manipulatorResult.ResultString = currentSqlStatement;
                    manipulatorResult.HasError = true;
                    manipulatorResult.Exception = e;
                }

                result.ManipulatorResults.Add(manipulatorResult);
            }

            result.ResultString = currentSqlStatement;
            return result;
        }
    }
}
