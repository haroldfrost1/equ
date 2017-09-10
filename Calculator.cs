using System;
using System.Text.RegularExpressions;
using Equ.Domain;
using Equ.Exceptions;

namespace Equ
{
    /// <summary>
    /// Calculator class that wraps equation solving functionality
    /// This class itself only handles accepting user ipnut
    /// and getting results from equations
    /// </summary>
    public class Calculator
    {
        /// <summary>
        /// Empty constructor for <see cref="T:Calc.Properties.Calculator"/> class.
        /// Grants access to accept equations as a string[] 
        /// </summary>
        public Calculator()
        {
        }

		/// <summary>
		/// Initializes a new instance of the Calculator with arguments
		/// and solve the equation passed in.
		/// </summary>
		/// <param name="args">A string array that should contain the equation</param>
		public Calculator(string[] args)
        {
            Accept(args);
        }

		/// <summary>
		/// Accept the specified args and creates an equation with them
		/// </summary>
		/// <param name="args">A string array that should contain the equation</param>
		public void Accept(string[] args)
        {
            if (args.Length >= 2)
            {
                if (args[0].ToLower().Equals("calc"))
                {
                    SolveEquation(ExtractEquationFromArguments(args));
                }
                else
                {
                    // print helper
                }
            }
            else
            {
                // print helper
            }
        }

        private void SolveEquation(string equationStr)
        {
			try
			{
                Equation equation = new Equation(equationStr);
				if (equation.Solutions.Count <= 0)
				{
					Console.WriteLine("This equation has no solution.");
				}
				else
				{
					Console.Write("X: ");
					for (int i = 0; i < equation.Solutions.Count; i++)
					{
						Console.Write(equation.Solutions[i]);
						if (equation.Solutions.Count > i + 1)
							Console.Write(", ");
					}
					Console.WriteLine();
				}
			}
			catch (EquationFormatException exception)
			{
				Console.WriteLine("EquationFormatException: {0}", exception);
			}
			catch (EvaluationException exception)
			{
				Console.WriteLine("EvaluationException: {0}", exception);
			}
			catch (Exception exception)
			{
				Console.WriteLine("Unknown exception caught: {0}", exception);
			}
        }

        /// <summary>
        /// This method concatenate any arguments passed in
        /// 
        /// PS: I'm really not if this is needed, the assignment doesn't say
        /// whether each term is passed in as exactly one argument or not.
        /// So this program will treat all the arguments as one string.
        /// </summary>
        /// <returns>The equation from arguments as string</returns>
        /// <param name="args">A string array that should contain the equation</param>
        private string ExtractEquationFromArguments(string[] args)
        {
            string equationStr = "";
            for (int i = 0; i < args.Length; i ++ )
            {
                if (i == 0) continue; // skip the first argument

                equationStr += args[i];
            }

            equationStr = equationStr.Trim();
            equationStr = equationStr.ToLower();
            equationStr = equationStr.Replace(" ", "");

            EliminateMultipleSigns(ref equationStr);

            return equationStr;
        }

        /// <summary>
        /// The procedure to eliminate the multiple concurrent signs in the equation string.
        /// </summary>
        /// <param name="input">Input.</param>
        private void EliminateMultipleSigns(ref string input)
        {
            EliminateSigns(ref input, "[-]{2,}"); // Eliminate minus signs
            EliminateSigns(ref input, "[+]{2,}"); // Eliminate plus signs
		}

        /// <summary>
        /// Eliminates the signs which matches the pattern given.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="regexPattern">Regex pattern.</param>
		private void EliminateSigns(ref string input, string regexPattern)
		{
			Regex regex = new Regex(regexPattern);
            Match match = regex.Match(input);
            switch (regexPattern)
            {
                case "[-]{2,}": // If the string contains more than 2 minus sign in a row
					if (match.Success)
                    {
                        // If the number of minus signs is even, that makes it a positive value
                        if (match.Length % 2 == 0)
                        {
                            input = regex.Replace(input, "+", 1);
                        }
                        // If the number is odd, that makes it a negative number
                        else
                        {
                            input = regex.Replace(input, "-", 1);
                        }
                    }
                    else
                    {
                        break;
                    }

                    // If it has a valid next match, replace them recursively
                    if (match.NextMatch().Length > 0)
                    {
                        EliminateSigns(ref input, regexPattern);
                    }
                    break;
                case "[+]{2,}": // If it contains more than 2 "+" signs, whatever it is positive anyways, replace them
                    if (match.Success)
                    {
                        input = regex.Replace(input, "+");
                    }
                    break;
                default:
                    throw new EquationFormatException("The pattern for sign elimination is not recognised.");
            }
		}
    }
}
