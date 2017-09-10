using System;

namespace Equ
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] testArgs = {"calc", "1+2x^2= -30"};

            Calculator calculator = new Calculator();
            calculator.Accept(testArgs);
        }
    }
}
