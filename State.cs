using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SqlCompiler.RegexLexer
{
    public class State
    {
        internal readonly WordList words = new WordList();
        private readonly Lexer parent;
        public readonly string stateName;
        public State(Lexer parent, string stateName)
        {
            this.parent = parent;
            this.stateName = stateName;
        }

        public Word AddDelim(string re, string token)
        {
            return AddWord(re, token, true);
        }
        public Word AddNonDelim(string re, string token)
        {
            return AddWord(re, token, false);
        }

        public WordList AddNewlineDelilms(string token)
        {
            var words = new WordList();
            words.Add(AddDelim(@"\r\n", token).NewLine());
            words.Add(AddDelim(@"\n", token).NewLine());
            words.Add(AddDelim(@"\r", token).NewLine());
            return words;
        }

        public WordList GetDelims()
        {
            return words.GetDelims();
        }

        private Word AddWord(string re, string token, bool isDelim)
        {
            Word match = new Word(parent, re, token, isDelim);
            words.Add(match);
            return match;
        }

    }
}
