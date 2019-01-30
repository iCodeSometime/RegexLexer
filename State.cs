using System;
using System.Linq;
using System.Collections.Generic;

namespace SqlCompiler.RegexLexer
{
    public class State
    {
        internal readonly List<Word> matches = new List<Word>();
        private readonly Lexer parent;
        public readonly string stateName;
        public State(Lexer parent, string stateName)
        {
            this.parent = parent;
            this.stateName = stateName;
        }

        public Word AddDelim(string re, string token)
        {
            return AddLexerWord(re, token, true);
        }
        public Word AddWord(string re, string token)
        {
            return AddLexerWord(re, token, false);
        }

        public List<Word> AddNewlineDelilms(string token)
        {
            var ret = new List<Word>();
            ret.Add(AddDelim(@"\r\n", token).NewLine());
            ret.Add(AddDelim(@"\n", token).NewLine());
            ret.Add(AddDelim(@"\r", token).NewLine());
            return ret;
        }

        public List<Word> GetDelims()
        {
            return matches.Where(m => m.isDelim).ToList();
        }

        private Word AddLexerWord(string re, string token, bool isDelim)
        {
            Word match = new Word(parent, re, token, isDelim);
            matches.Add(match);
            return match;
        }

    }
}
