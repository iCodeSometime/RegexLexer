using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SqlCompiler.StringScanner;

namespace SqlCompiler.RegexLexer
{
    public class Lexer
    {
        private readonly Dictionary<string, LexerState> definedStates = new Dictionary<string, LexerState>();

        public LexerState State(string stateName)
        {
            if (!definedStates.ContainsKey(stateName))
            {
                definedStates.Add(stateName, new LexerState(this, stateName));
            }
            return definedStates[stateName];
        }

        public List<LexerToken> Lex(string content)
        {
            uint lineNum = 1;
            uint charNum = 1;
            List<SyntaxException> foundExceptions = new List<SyntaxException>();
            List<LexerToken> tokens = new List<LexerToken>();
            Scanner scanner = new Scanner(content);
            Stack<LexerState> state = new Stack<LexerState>();

            // Start off in the default state.
            state.Push(State("default"));
            do
            {
                // Create token
                LexerWord lexData;
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
                tokens.Add(new LexerToken(lexData.token,
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

        private (LexerWord, string) ReadMatch(Scanner scanner, LexerState state)
        {
            var word = scanner.ReadNextWordOrDelim(state.GetDelims().Cast<Regex>());
            var (lexData, regexMatch) =
                state.matches.Select(lm => (lm, lm.Match(word)))
                     .Where(m => m.Item2.Success)
                     .OrderByDescending(m => m.Item1.isDelim)
                     .ThenByDescending(m => m.Item2.Length)
                     .FirstOrDefault();
            if (regexMatch == null)
            {
                throw new SyntaxException(
                    $"\"{word}\" is not valid in the \"{state.stateName}\" state."
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
