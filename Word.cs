using System;
using System.Linq;
using System.Text.RegularExpressions;
using SqlCompiler.StringScanner;
using System.Collections.Generic;

namespace SqlCompiler.RegexLexer
{
    public class Word : Regex, IDelimiter
    {
        public bool IsStateTransition => doesPopState || pushState != null;
        internal bool doesPopState;
        internal State pushState;
        internal bool isDelim;
        internal bool isNewLine;
        internal string token;
        private readonly Lexer parent;

        public Word(Lexer parent, string rePattern,
                          string token, bool isDelim) : base(rePattern)
        {
            this.parent = parent;
            this.isDelim = isDelim;
            this.token = token;
        }
        public Word PushState(string state)
        {
            pushState = parent.State(state);
            return this;
        }
        public Word PopState()
        {
            doesPopState = true;
            return this;
        }

        public Word NewLine()
        {
            isNewLine = true;
            return this;
        }
    }

    public class WordList : DelimiterCollection<Word>
    {
        public WordList() : this(new List<Word>()) { }
        public WordList(List<Word> words) : base(words) { }
        public override bool IsReadOnly => false;

        // TODO: Memoize
        public WordList GetDelims()
        {
            return new WordList(this.Where<Word>(w => w.isDelim).ToList());
        }

        /// <summary>
        /// Returns the longest Delim, or longest word if none.
        /// </summary>
        /// <returns>The matched word, and the match.</returns>
        /// <param name="toMatch">The string to match.</param>
        public (Word, Match) WordMatch(string toMatch)
        {
            return Delimiters.Select(w => (w, w.Match(toMatch)))
                             .Where(m => m.Item2.Success)
                             .OrderByDescending(m => m.Item1.isDelim)
                             .ThenByDescending(m => m.Item2.Length)
                             .FirstOrDefault();
        }
    }
}
