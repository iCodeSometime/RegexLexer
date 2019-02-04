using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using SqlCompiler.StringScanner;

namespace SqlCompiler.RegexLexer
{
    public class Lexer
    {
        private readonly ConcurrentDictionary<string, State> definedStates = new ConcurrentDictionary<string, State>();

        public State State(string stateName)
        {
            return definedStates.GetOrAdd(stateName, new State(this, stateName));
        }

        public IEnumerable<Token> Lex(string content)
        {
            uint lineNum = 1;
            uint charNum = 1;
            List<SyntaxException> foundExceptions = new List<SyntaxException>();
            List<Token> tokens = new List<Token>();
            Scanner scanner = new Scanner(content);
            Stack<State> state = new Stack<State>();
            string match = "";

            // Start off in the default state.
            state.Push(State("default"));
            do
            {
                // Create token
                Word lexData;
                try
                {
                    (lexData, match) = ReadMatch(scanner, state.Peek());
                }
                catch (SyntaxException e)
                {
                    foundExceptions.Add(e);
                    continue;
                }
                if (match == null) break;
                tokens.Add(new Token(lexData.token,
                                          match, lineNum, charNum));
                
                // Handle State
                if (lexData.doesPopState)
                {
                    state.Pop();
                }
                if (lexData.pushState != null)
                {
                    state.Push(lexData.pushState);
                }

                // Handle Newline
                if (lexData.isNewLine)
                {
                    lineNum += 1;
                    charNum = 1;
                }
                else
                {
                    charNum += (uint)match.Length;
                }
            } while (match != null);

            if (foundExceptions.Count > 0)
            {
                throw new AggregateException(foundExceptions);
            }
            return tokens;
        }

        private (Word, string) ReadMatch(Scanner scanner, State state)
        {
            var word = scanner.Read(state.GetDelims() as DelimiterCollection<Word>);

            if (word == null) return (null, null);
            var (lexData, regexMatch) = state.words.WordMatch(word);

            string invalid = word.Substring(regexMatch?.Length ?? 0);
            if (!String.IsNullOrEmpty(invalid))
            {
                throw new SyntaxException(
                    $"\"{invalid}\" is not a valid token in the \"{state.stateName}\" state."
                );
            }

            return (lexData, regexMatch.Value);
        }
    }

    public class SyntaxException : Exception {
        public SyntaxException() : base() { }
        public SyntaxException(string message) : base(message) { }
    }
}
