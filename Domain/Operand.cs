using System;
using Equ.Enums;
using Equ.Exceptions;

namespace Equ.Domain
{
    public class Operand
    {
        public int ConstantValue { get; set; }
        public int Coefficient { get; set; }
        public int QuadraticCoefficient { get; set; }

        public int DenominatorCoefficient { get; set; }
        public int DenominatorQuadraticCoefficient { get; set; }
        public int DenominatorConstantValue { get; set; }

        public Operand()
        {
            ConstantValue = 0;
            Coefficient = 0;
            QuadraticCoefficient = 0;

            DenominatorCoefficient = 0;
            DenominatorQuadraticCoefficient = 0;
            DenominatorConstantValue = 1;
        }

        public Operand(Token token)
        {
            switch (token.TypeEnum)
            {
                case TokenEnum.Number:
                    ConstantValue = Int32.Parse(token.Sequence);
                    break;
                case TokenEnum.Variable:
                    Coefficient = 1;
                    break;

                default:
                    throw new EvaluationException("Failed to parse token '{0}' to operand", token.Sequence);
            }

            DenominatorCoefficient = 0;
            DenominatorQuadraticCoefficient = 0;
            DenominatorConstantValue = 1;
        }

        public static Operand operator +(Operand a, Operand b)
        {
            a.Coefficient += b.Coefficient;
            a.ConstantValue += + b.ConstantValue;
            a.QuadraticCoefficient += b.QuadraticCoefficient;
            
            return a;
        }

        public static Operand operator -(Operand a, Operand b)
        {
            a.Coefficient -= b.Coefficient;
            a.ConstantValue -= b.ConstantValue;
            a.QuadraticCoefficient -= b.QuadraticCoefficient;
            return a;
        }

        public static Operand operator *(Operand a, Operand b)
        {
            int c = a.QuadraticCoefficient,
                d = a.Coefficient,
                e = a.ConstantValue;

            int f = b.QuadraticCoefficient,
                g = b.Coefficient,
                h = b.ConstantValue;

            // If the product has a power higher than 2
            if ((c != 0 && (f != 0 || g != 0)) || (f != 0 && (c != 0 || d != 0)))
            {
                throw new EvaluationException("Solving equation with a power higher than 2 is not supported(required) yet.");
            }

            // Formula: (cx^2 + dx + e)(fx^2 + gx + h) = 
            a.QuadraticCoefficient = c * h + d * g + e * f;
            a.Coefficient = d * h + e * g;
            a.ConstantValue = e * h;

            return a;
        }

        public static Operand operator /(Operand a, Operand b)
        {
            if (b.Coefficient != 0 || b.QuadraticCoefficient != 0)
            {
                // if b is not only a constant, the equation will be a fractional equation.
                throw new EvaluationException("Solving fractional equation is not supported yet.");
            }
            else
            {
                if (b.ConstantValue != 0)
                {
                    a.ConstantValue /= b.ConstantValue;

                    if (a.Coefficient != 0)
                        a.DenominatorConstantValue = b.ConstantValue;
                }
                else
                {
                    throw new EvaluationException("Trying to divide by 0.");
                }
            }
            return a;
        }

        public static Operand operator ^(Operand a, Operand b)
        {
            if (b.Coefficient != 0 || b.QuadraticCoefficient != 0)
            {
                throw new EvaluationException("Solving exponential function is not supported(required) yet.");
            }

            // For case like: (2x + 3)^2
            // Formula (ax + b)^2 = (a^2)x^2 + 2abx + b ^ 2, where a can be 0
            if (a.QuadraticCoefficient == 0 && b.ConstantValue == 2)
            {
                a.QuadraticCoefficient = (int)Math.Pow(a.Coefficient, b.ConstantValue);
                a.Coefficient = 2 * a.Coefficient * a.ConstantValue;
                a.ConstantValue = (int)Math.Pow(a.ConstantValue, b.ConstantValue);
            }
            else if (a.Coefficient == 0 && a.QuadraticCoefficient == 0)
            {
                a.ConstantValue = (int)Math.Pow(a.ConstantValue, b.ConstantValue);
            }
            else
            {
                throw new EvaluationException("Solving equation with a power higher than 2 is not supported(required) yet.");
            }

            return a;
        }

        public static Operand operator %(Operand a, Operand b)
        { 
            if (a.QuadraticCoefficient != 0
                || b.QuadraticCoefficient != 0
                || a.Coefficient != 0
                || b.Coefficient != 0)
            {
                throw new EvaluationException("Modulo applied on variable is not supported.");
            }
            else
            {
				a.ConstantValue %= b.ConstantValue;
            }

            return a;
        }
    }
}
