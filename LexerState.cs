using System;
using System.Linq;
using System.Collections.Generic;

namespace SqlCompiler.RegexLexer
{
    public class LexerState
    {
        internal readonly List<LexerWord> matches = new List<LexerWord>();
        private readonly Lexer parent;
        public readonly string stateName;
        public LexerState(Lexer parent, string stateName)
        {
            this.parent = parent;
            this.stateName = stateName;
        }

        public LexerWord AddDelim(string re, string token)
        {
            return AddLexerWord(re, token, true);
        }
        public LexerWord AddWord(string re, string token)
        {
            return AddLexerWord(re, token, false);
        }

        public List<LexerWord> AddUniversalNewline(string token)
        {
            var ret = new List<LexerWord>();
            ret.Add(AddDelim(@"\r\n", token).NewLine());
            ret.Add(AddDelim(@"\n", token).NewLine());
            ret.Add(AddDelim(@"\r", token).NewLine());
            return ret;
        }

        public List<LexerWord> GetDelims()
        {
            return matches.Where(m => m.isDelim).ToList();
        }

        private LexerWord AddLexerWord(string re, string token, bool isDelim)
        {
            LexerWord match = new LexerWord(parent, re, token, isDelim);
            matches.Add(match);
            return match;
        }

    }
}
