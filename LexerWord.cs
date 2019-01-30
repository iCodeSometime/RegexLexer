using System;
using System.Text.RegularExpressions;

namespace SqlCompiler.RegexLexer
{
    public class LexerWord : Regex
    {
        public bool IsStateTransition => doesPopState || pushState != null;
        internal bool doesPopState;
        internal LexerState pushState;
        internal bool isDelim;
        internal bool isNewLine;
        internal string token;
        private readonly Lexer parent;

        public LexerWord(Lexer parent, string rePattern,
                          string token, bool isDelim) : base(rePattern)
        {
            this.parent = parent;
            this.isDelim = isDelim;
            this.token = token;
        }
        public LexerWord PushState(string state)
        {
            pushState = parent.State(state);
            return this;
        }
        public LexerWord PopState()
        {
            doesPopState = true;
            return this;
        }

        public LexerWord NewLine()
        {
            isNewLine = true;
            return this;
        }
    }
}
