using System;
namespace Equ.Exceptions
{
    public class EvaluationException : Exception
    {
        public EvaluationException()
        {
        }

        public EvaluationException(string message, string sequence) : base(string.Format(message, sequence))
        {
            
        }

        public EvaluationException(string message) : base(message)
		{

		}
    }
}
