using System;

namespace SqlCompiler.RegexLexer
{
    public class Token
    {
        public readonly string id;
        public readonly string value;
        public readonly uint lineNum;
        public readonly uint charNum;

        public Token(string id, string value, uint lineNum, uint charNum)
        {
            this.id = id;
            this.value = value;
            this.lineNum = lineNum;
            this.charNum = charNum;
        }
        public Token(string id, string value)
        {
            this.id = id;
            this.value = value;
        }
    }
}
