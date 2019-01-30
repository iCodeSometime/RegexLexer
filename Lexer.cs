using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SqlCompiler.StringScanner;

namespace SqlCompiler.RegexLexer
{
    public class Lexer
    {
        private readonly Dictionary<string, State> definedStates = new Dictionary<string, State>();

        public State State(string stateName)
        {
            if (!definedStates.ContainsKey(stateName))
            {
                definedStates.Add(stateName, new State(this, stateName));
            }
            return definedStates[stateName];
        }

        public IEnumerable<Token> Lex(string content)
        {
            uint lineNum = 1;
            uint charNum = 1;
            List<SyntaxException> foundExceptions = new List<SyntaxException>();
            List<Token> tokens = new List<Token>();
            Scanner scanner = new Scanner(content);
            Stack<State> state = new Stack<State>();

            // Start off in the default state.
            state.Push(State("default"));
            do
            {
                // Create token
                Word lexData;
                string match;
                try
                {
                    (lexData, match) = ReadMatch(scanner, state.Peek());
                }
                catch (SyntaxException e)
                {
                    foundExceptions.Add(e);
                    continue;
                }
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
            } while (scanner.Peek() != -1);

            if (foundExceptions.Count > 0)
            {
                throw new AggregateException(foundExceptions);
            }
            return tokens;
        }

        private (Word, string) ReadMatch(Scanner scanner, State state)
        {
            var word = scanner.ReadNextWordOrDelim(state.GetDelims().Cast<Regex>());
            var (lexData, regexMatch) =
                state.matches.Select(lm => (lm, lm.Match(word)))
                     .Where(m => m.Item2.Success)
                     .OrderByDescending(m => m.Item1.isDelim)
                     .ThenByDescending(m => m.Item2.Length)
                     .FirstOrDefault();

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



//test.State("default").AddDelim("/\*", 'block_comment_start').PushState("block_comment");
//test.State("block_comment").AddWord("\.\*", 'comment_body');
//test.State("block_comment").AddDelim("\*/").PopState();
//test.State("default").AddDelim("'", "begin_string").PushState("string");
//test.State("string").AddWord(".*", "string_body")
//test.State("string").AddDelim("'", "end_string").PopState()
//test.State("default").MatchWord("f");
