using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equ.Domain;
using Equ.Enums;
using System.Text.RegularExpressions;
using Equ.Exceptions;

namespace Equ.Utilities
{
    class Tokeniser
    {
        private static Tokeniser tokeniser = null;

        public LinkedList<TokenType> TokenTypes { get; set; }

        public Tokeniser()
        {
            TokenTypes = new LinkedList<TokenType>();
        }

        public static Tokeniser GetTokeniser()
        {
            return tokeniser ?? (tokeniser = CreateTokeniser());
        }

        private static Tokeniser CreateTokeniser()
        {
            Tokeniser localTokeniser = new Tokeniser();

            localTokeniser.AddTokenType("[+-]", TokenEnum.Plusminus);
            localTokeniser.AddTokenType("[*/]", TokenEnum.Multdiv);
            localTokeniser.AddTokenType("\\^", TokenEnum.Raised);
            localTokeniser.AddTokenType("[%]", TokenEnum.Modulo);

            localTokeniser.AddTokenType("\\(", TokenEnum.OpenBracket);
            localTokeniser.AddTokenType("\\)", TokenEnum.CloseBracket);
            localTokeniser.AddTokenType("(?:\\d+\\.?|\\.\\d)\\d*(?:[Ee][-+]?\\d+)?", TokenEnum.Number);
            localTokeniser.AddTokenType("[a-zA-Z]\\w*", TokenEnum.Variable);
            localTokeniser.AddTokenType("[=]", TokenEnum.EqualSign);

            return localTokeniser;
        }

        private void AddTokenType(string pattern, TokenEnum tokenEnum)
        {
            TokenTypes.AddLast(new TokenType(new Regex("^(" + pattern + ")"), tokenEnum));
        }

        public LinkedList<Token> Tokenise(string equation)
        {
            LinkedList<Token> tokens = new LinkedList<Token>();

            while (!equation.Equals(""))
            {
                bool hasMatch = false;

                foreach (TokenType tokentype in TokenTypes)
                {
                    Match match = tokentype.Rgx.Match(equation);

                    if (!match.Success) continue;
                    
                    hasMatch = true;
                    string tokenSequence = match.Value;

                    equation = tokentype.Rgx.Replace(equation, "");
                    tokens.AddLast(new Token(tokentype.TypeEnum, tokenSequence));
                    break;
                }
                
                if (!hasMatch)
                    throw new EquationFormatException("Unexpected token at '{0}'", equation);
            }

            return tokens;
        }
    }
}
