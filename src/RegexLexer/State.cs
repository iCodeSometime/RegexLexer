using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace RegexLexer
{
    public class State
    {
        internal readonly WordList words = new WordList();
        private readonly Lexer parent;
        public readonly string name;

        public State(Lexer parent, string stateName)
        {
            this.parent = parent;
            this.name = stateName;
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
    public class StateContainer : Stack<State>
    {
        public uint LineNum { get; private set; }
        public uint CharNum { get; private set; }
        public StateContainer()
        {
            LineNum = 1;
            CharNum = 1;
        }

        public void NewLine()
        {
            LineNum++;
            CharNum = 1;
        }
        public void IncrementChar(uint count)
        {
            CharNum += count;
        }

        public void IncrementChar(int count)
        {
            IncrementChar((uint)count);
        }
        #region proxying
        public WordList GetDelims() => Peek().GetDelims();
        public WordList words => Peek().words;
        public string name => Peek().name;
        #endregion
    }
}
