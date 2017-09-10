using System;
using Equ.Exceptions;

namespace Equ.Domain
{
    public class Operation
    {
        public Operation()
        {
        }

        public static Operand Evaluate(Token token, Operand a, Operand b)
        {
            Operand result = new Operand();

            switch (token.Sequence)
            {
                case "+":
                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                case "^":
                    result = a ^ b;
                    break;
                case "%":
                    result = a % b;
                    break;
                default:
                    throw new EvaluationException("Unexcepted operation symbol: '{0}'", token.Sequence);
            }

            return result;
        }
    }
}
