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
        // Lexer class is not actually thread safe.
        private readonly ConcurrentDictionary<string, State> definedStates = new ConcurrentDictionary<string, State>();

        public State State(string stateName)
        {
            return definedStates.GetOrAdd(stateName, new State(this, stateName));
        }

        public IEnumerable<Token> Lex(string content)
        {
            StateContainer state = new StateContainer();
            Token token;
            Scanner scanner = new Scanner(content);

            // Start off in the default state.
            state.Push(State("default"));
            while ((token = ReadToken(scanner, state)) != null)
            {
                yield return token;
            } 
        }

        private Token ReadToken(Scanner scanner, StateContainer state)
        {
            var word = scanner.Read(state.GetDelims() as DelimiterCollection<Word>);
            if (word == null) return null;

            var (lexData, regexMatch) = state.words.WordMatch(word);
            string invalid = word.Substring(regexMatch?.Length ?? 0);
            if (!String.IsNullOrEmpty(invalid))
            {
                throw new SyntaxException(
                    $"\"{invalid}\" is not a valid token in the \"{state.name}\" state."
                );
            }

            Token ret = new Token(lexData.token, regexMatch.Value, state.LineNum, state.CharNum);

            ProcessState(lexData, regexMatch.Value, state);

            return ret;
        }

        private void ProcessState(Word word, string match, StateContainer state)
        {
            if (word.doesPopState)
            {
                state.Pop();
            }
            if (word.pushState != null)
            {
                state.Push(word.pushState);
            }
            if (word.isNewLine) state.NewLine();
            else
            {
                state.IncrementChar(match.Length);
            }
        }
    }

    public class SyntaxException : Exception {
        public SyntaxException() : base() { }
        public SyntaxException(string message) : base(message) { }
    }
}
