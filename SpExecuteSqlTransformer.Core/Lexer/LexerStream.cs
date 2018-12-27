using System;
using System.Linq;

namespace SpExecuteSqlTransformer.Core.Lexer
{
    public interface ILexerStream
    {
        int Position { get;  }
        int Length { get; }
        char Current{get;}
        char Next();
        void MoveToNextIfPossible();
        bool CharsLeft{get;}
        bool MatchesExactly(String matchString, bool mustBeTerminatedByWhiteSpaceOrSpecialChar);
        void MoveTo(int index);
        void Move(int i);
        bool IsNext(char c);   
        bool IsNextWhiteSpaceOrTerminatingSpecialChar();
        bool EndOfStream { get; }
        bool IsWhiteSpaceOrTerminatingSpecialChar(int index);
        bool IsTerminatingSpecialChar(int index);
        string Substring(int start, int end);
    }

    public class LexerStream : ILexerStream
    {
        public LexerStream(string input)
        {
            this.Input = input;
            this.Position = 0;
            this.Length = input.Length;

        }
        public string Input { get; private set; }
        public int Position { get; private set; }
        public int Length { get; private set; }

        public StateType State { get; set; }

        public char Current
        {
            get
            {
                if (EndOfStream)
                    throw new InvalidOperationException(
                        "Unable to get current character as the end of the lexer stream was reached (Position == Length).\r\n" +
                        $"You may want to check for '{nameof(CharsLeft)}' before accessing {nameof(Current)}.");
                return Input[Position];
            }
        }

        public char Next()
        {
            Position++;
            EnsureCorrectIndex(Position, endOfStreamAllowed: false);
            return Current;
        }

        public bool CharsLeft
        {
            get{
                return Position < (Length-1);
            }
        }

        public bool EndOfStream => Position == Length;

        public bool MatchesExactly(String matchString, bool mustBeTerminatedByWhiteSpaceOrSpecialChar)
        {
            var index = Input.IndexOf(matchString, Position, StringComparison.InvariantCultureIgnoreCase);

            if(index == Position)
            {
                if(!mustBeTerminatedByWhiteSpaceOrSpecialChar)
                    return true;

                var isTerminated = IsWhiteSpaceOrTerminatingSpecialChar(Position + matchString.Length);
                if(isTerminated)
                    return true;
            }
            return false;
        }

        public bool IsNextWhiteSpaceOrTerminatingSpecialChar()
        {
            if(!CharsLeft)
                return true;
            return IsWhiteSpaceOrTerminatingSpecialChar(Position+1);
        }

        public bool IsWhiteSpaceOrTerminatingSpecialChar(int index)
        {
            if(index == Input.Length) //end of string also means terminated
                return true;
            var character = Input[index];
            var terminating = char.IsWhiteSpace(character) || GetTerminatingSpecialChars().Any(c => character == c);
            return terminating;
        }
        
        public bool IsTerminatingSpecialChar(int index)
        {
            var character = Input[index];
            return GetTerminatingSpecialChars().Any(c => character == c);
        }

        public char[] GetTerminatingSpecialChars()
        {
            return new[]{',', '=', ';', '(', ')' };
        }

        public void Move(int i)
        {
            var moveTo = Position + i;
            MoveTo(moveTo);
        }

        public void MoveTo(int index)
        {
            EnsureCorrectIndex(index, endOfStreamAllowed: true);
            Position = index;
        }

        private void EnsureCorrectIndex(int index, bool endOfStreamAllowed)
        {
            if (index < 0)
                throw new ArgumentException("Index must be zero or greater than zero");

            if (endOfStreamAllowed)
            {
                if (index > Length)
                    throw new IndexOutOfRangeException($"Index must not be greater than {Input.Length} but was {index}");
            }
            else
            {
                if (index >= Length)
                    throw new IndexOutOfRangeException($"Index must not be greater than {Input.Length - 1} but was {index}");
            }
        }

        public bool IsNext(char c)
        {
            if(!CharsLeft)
                return false;
            return Input[Position+1] == c;                
        }

        public string Substring(int start, int end)
        {
            EnsureCorrectIndex(start, endOfStreamAllowed: false);
            EnsureCorrectIndex(end, endOfStreamAllowed: false);
            return Input.Substring(start, end-start+1);
        }

        public void MoveToNextIfPossible()
        {
            if (Position < Length)
                Position++;
            EnsureCorrectIndex(Position, endOfStreamAllowed: true);          
        }
    }
}