using Equ.Enums;
using System;
namespace Equ.Domain
{
    /// <summary>
    /// A Token that represents a mathematical symbol, e.g. "+", "-", "*", "/"
    /// </summary>
    public class Token
    {
        public TokenEnum TypeEnum { get; set; }
        public string Sequence { get; set; }

        public Token(TokenEnum typeEnum, string sequence)
        {
            TypeEnum = typeEnum;
            Sequence = sequence;
        }
    }
}
