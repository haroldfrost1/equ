using System;

namespace Equ.Exceptions
{
    public class EquationFormatException : Exception
    {
        public EquationFormatException()
        {
        }

        public EquationFormatException(string message, string sequence) : base(string.Format(message, sequence))
        {
        }

        public EquationFormatException(string message) : base(message)
		{
		}
    }
}