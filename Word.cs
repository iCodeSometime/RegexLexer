using System;
using System.Text.RegularExpressions;

namespace SqlCompiler.RegexLexer
{
    public class Word : Regex
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
}
