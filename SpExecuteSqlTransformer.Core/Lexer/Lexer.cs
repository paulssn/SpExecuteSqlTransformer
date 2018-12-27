using System;
using System.Collections.Generic;

namespace SpExecuteSqlTransformer.Core.Lexer
{
    public enum StateType
    {
        WhiteSpace,
        Word,
        String,
        Punctuation
    }

    public class Lexer
    {       
        public string Input { get; private set; }
        public int Position { get; private set; }
        public int Length { get; private set; }

        public StateType State { get; set; }

        public LexerStream LexerStream { get; set; }

        public List<Token> Tokenize(string input)
        {
            var resultList = new List<Token>();

            if (input == null)
                throw new ArgumentNullException();
            LexerStream = new LexerStream(input);
            Input = input;
            Length = input.Length;
            Position = 0;

            var matcherList = GetMatchers();

            while (!LexerStream.EndOfStream)
            {
                if(char.IsWhiteSpace(LexerStream.Current))
                {
                    LexerStream.MoveToNextIfPossible();
                }
                else
                {
                    var token = GetToken(matcherList);
                    resultList.Add(token);
                }                
            }

            return resultList;
        }

        private List<IMatcher> GetMatchers()
        {
            return new List<IMatcher>()
            {
                new ExecKeywordMatcher(),
                new ExecuteSqlKeywordMatcher(),
                new VariableMatcher(),
                new StringMatcher(),
                new CommaMatcher(),
                new EqualsSignMatcher(),
                new SemicolonMatcher(),
                new WordMatcher(),
                new AnySpecialCharMatcher(),
            };
        }

        private Token GetToken(List<IMatcher> matchers){
            foreach(var matcher in matchers)
            {
                var result = matcher.GetToken(LexerStream);
                if(result.Type != TokenType.NullToken)
                    return result;
            }
            throw new InvalidOperationException("No matcher handled stream"); //TODO
        }
    }
}