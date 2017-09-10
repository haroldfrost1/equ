using System;
using System.Collections.Generic;
using System.Linq;

using Equ.Enums;
using Equ.Exceptions;
using Equ.Utilities;

namespace Equ.Domain
{
    class Equation
    {
        private readonly string equationStr;
        private readonly LinkedList<Token> leftTokens;
        private readonly LinkedList<Token> rightTokens;
        private readonly LinkedList<Token> equationTokens;

        public List<int> Solutions { get; set; }

        public Equation(string equationStr)
        {
            if (!IsEquationValid(equationStr))
			{
				throw new EquationFormatException("The input equation is not valid.");
			}

            // Just initializing everythin we need.
            this.equationStr = equationStr;
            this.equationTokens = new LinkedList<Token>();
            this.rightTokens = new LinkedList<Token>();
            this.leftTokens = new LinkedList<Token>();
            Solutions = new List<int>();

			Tokeniser tokeniser = Tokeniser.GetTokeniser();
            this.equationTokens = tokeniser.Tokenise(this.equationStr);

            this.leftTokens = ToPostfix(GetLeft());
            this.rightTokens = ToPostfix(GetRight());

            // This calls the internal method to find a solution,
            // and add solutions to the member field Solutions
            Solve();
        }

        /// <summary>
        /// Solve this equation and add soluctions to the Solutions 
        /// </summary>
        public void Solve()
        {
            // Solve postfix expression on both sides and retrieve a value
            Operand left = Evaluate(this.leftTokens);
            Operand right = Evaluate(this.rightTokens);

            if (left.DenominatorCoefficient != 0
                || left.DenominatorQuadraticCoefficient != 0
                || right.DenominatorCoefficient != 0
                || right.DenominatorQuadraticCoefficient != 0)
            {
                throw new EvaluationException("Solving frational equation is not supported yet.");
            }

			// For quadratic equation
            if (left.QuadraticCoefficient != 0 || right.QuadraticCoefficient != 0)
            {
                // (a/b)x^2 + (c/d)x + e = (f/g)x^2 + (h/i)x + j

                // ax^2 + bx + c = 0
                int a = left.QuadraticCoefficient - right.QuadraticCoefficient;
                int b = left.Coefficient - right.Coefficient;
                int c = left.ConstantValue - right.ConstantValue;

                int delta = (int)Math.Pow(b, 2) - 4 * a * c;

                if (delta < 0) return; // no real root

                int solutionA = (-b + (int)Math.Sqrt(delta)) / (2 * a);
                int solutionB = (-b - (int)Math.Sqrt(delta)) / (2 * a);

                Solutions.Add(solutionA);
                Solutions.Add(solutionB);
            }
            // Linear equation
            else if (left.Coefficient != 0 || right.Coefficient != 0)
            {
                int solution = right.ConstantValue - left.ConstantValue;
                solution *= (left.DenominatorConstantValue * right.DenominatorConstantValue);
                solution /= (left.Coefficient * right.DenominatorConstantValue) - (left.DenominatorConstantValue * right.Coefficient);
                Solutions.Add(solution);
            }
            else
            {
                throw new EvaluationException("Unsupported equation type.");
            }
        }

        /// <summary>
        /// Evaluate the a list of tokens which is already converted to postfix notation.
        /// </summary>
        /// <returns>An operand represents the result</returns>
        /// <param name="tokens">Postfix Notation tokens</param>
        private Operand Evaluate(LinkedList<Token> tokens)
        {
            LinkedListNode<Token> currentNode = tokens.First;
            Stack<Operand> operandStack = new Stack<Operand>();

            while (currentNode != null)
            {
                Token currentToken = currentNode.Value;
                switch (currentToken.TypeEnum)
                {
                    case TokenEnum.Number:
                    case TokenEnum.Variable:
                        operandStack.Push(new Operand(currentToken));
                        break;
                    case TokenEnum.Multdiv:
                    case TokenEnum.Plusminus:
                    case TokenEnum.Raised:
                    case TokenEnum.Modulo:
                        try
                        {
                            Operand b = operandStack.Pop();
                            Operand a = operandStack.Pop();

                            Operand result = Operation.Evaluate(currentToken, a, b);
                            operandStack.Push(result);
                            break;
                        }
                        catch (InvalidOperationException exception)
                        {
                            throw new EvaluationException("Incorrect number of operands or operation symbols.", exception.Message);
                        }
                    default:
                        throw new EvaluationException("Unexpected Token while evaluating: {0}", currentToken.Sequence);
                }

                currentNode = currentNode.Next;
            }

            if (operandStack.Count != 1)
                throw new EvaluationException("Incorrect number of operands.");

            return operandStack.Peek();
        }

        # region ToPostfix
        /// <summary>
        /// Convert a list of infix tokens to postfix notation
        /// </summary>
        /// <returns>The list of tokens represents in postfix notation.</returns>
        /// <param name="tokens">Infix tokens.</param>
        private LinkedList<Token> ToPostfix(LinkedList<Token> tokens)
        {
            LinkedList<Token> postfixExpressionTokens = new LinkedList<Token>();

            LinkedListNode<Token> currentNode = tokens.First;
            Stack<Token> operationStack = new Stack<Token>();

            while (currentNode != null)
            {
                Token currentToken = currentNode.Value;

                switch (currentToken.TypeEnum)
                {
                    case TokenEnum.Variable:
                        postfixExpressionTokens.AddLast(currentToken);
                        break;
                    case TokenEnum.Number:
                        if (currentNode.Next != null)
                        {
                            Token nextToken = currentNode.Next.Value;
                            if (nextToken.TypeEnum == TokenEnum.Variable)
                            {
                                tokens.AddAfter(currentNode, new Token(TokenEnum.Multdiv, "*"));
                                postfixExpressionTokens.AddLast(currentToken);
                            }
                            else if (nextToken.TypeEnum == TokenEnum.OpenBracket)
                            {
                                postfixExpressionTokens.AddLast(currentToken);
                                operationStack.Push(new Token(TokenEnum.Multdiv, "*"));
                            }
                            else
                            {
                                postfixExpressionTokens.AddLast(currentToken);
                            }
                        }
                        else
                        {
                            postfixExpressionTokens.AddLast(currentToken);
                        }
                        break;
                    case TokenEnum.Raised:
                        if (currentNode.Previous == null)
                        {
                            throw new EquationFormatException("Invalid symbol '^' at the start of the equation.");
                        }

						if (currentNode.Next == null)
						{
							break;
						}

                        if (operationStack.Count == 0 || operationStack.Peek().TypeEnum == TokenEnum.OpenBracket)
                        {
                            operationStack.Push(currentToken);
                        }
                        else if (operationStack.Peek().TypeEnum == TokenEnum.Raised)
                        {
                            while (operationStack.Count > 0)
                            {
                                if (operationStack.Peek().TypeEnum == TokenEnum.Raised)
                                {
                                    postfixExpressionTokens.AddLast(operationStack.Pop());
                                }
                                else if (operationStack.Peek().TypeEnum == TokenEnum.OpenBracket)
                                {
                                    break;
                                }
                                else
                                {
                                    throw new EvaluationException("Unexpected operation: {0}", operationStack.Peek().Sequence);
                                }
                            }
                            operationStack.Push(currentToken);
                        }
                        else
                        {
                            operationStack.Push(currentToken);
                        }
                        break;
                    case TokenEnum.Multdiv:
                    case TokenEnum.Modulo:
                        if (currentNode.Previous == null)
                        {
                            throw new EquationFormatException("Invalid symbol '{0}' at the start of the equation.", currentToken.Sequence);
                        }

						if (currentNode.Next == null)
						{
							break;
						}

                        // Check is there is a negative term which starts with "-"
                        if (currentNode.Next != null){
                            LinkedListNode<Token> nextNode = currentNode.Next;

                            // If so, wrap around the "-" token, replace it with "(-1)*"
                            if (nextNode.Value.Sequence.Equals("-"))
                            {
                                tokens.AddAfter(currentNode, new Token(TokenEnum.OpenBracket, "("));
								tokens.AddAfter(nextNode, new Token(TokenEnum.Multdiv, "*"));
								tokens.AddAfter(nextNode, new Token(TokenEnum.Number, "1"));
                                tokens.AddAfter(nextNode.Next, new Token(TokenEnum.CloseBracket, ")"));
                            }
                            else if (nextNode.Value.Sequence.Equals("+"))
                            {
                                tokens.Remove(nextNode);
                            }
                        }

                        if (operationStack.Count == 0 || operationStack.Peek().TypeEnum == TokenEnum.OpenBracket)
                        {
                            operationStack.Push(currentToken);
                        }
                        else if (operationStack.Peek().TypeEnum == TokenEnum.Plusminus)
                        {
                            operationStack.Push(currentToken);
                        }
                        else
                        {
                            while (operationStack.Count > 0)
                            {
                                if (operationStack.Peek().TypeEnum == TokenEnum.Raised
                                    || operationStack.Peek().TypeEnum == TokenEnum.Multdiv
                                    || operationStack.Peek().TypeEnum == TokenEnum.Modulo)
                                {
                                    postfixExpressionTokens.AddLast(operationStack.Pop());
                                }
                                else if (operationStack.Peek().TypeEnum == TokenEnum.OpenBracket
                                    || operationStack.Peek().TypeEnum == TokenEnum.Plusminus)
                                {
                                    break;
                                }
                            }
                            operationStack.Push(currentToken);
                        }
                        break;
                    case TokenEnum.Plusminus:
                        if (currentNode.Previous == null)
                        {
                            postfixExpressionTokens.AddLast(new Token(TokenEnum.Number, "0"));
                        }

                        if (currentNode.Next == null)
                        {
                            break;
                        }

                        if (operationStack.Count == 0 || operationStack.Peek().TypeEnum == TokenEnum.OpenBracket)
                        {
                            operationStack.Push(currentToken);
                        }
                        else
                        {
                            while (operationStack.Count > 0)
                            {
                                if (operationStack.Peek().TypeEnum == TokenEnum.OpenBracket)
                                {
                                    break;
                                }

                                postfixExpressionTokens.AddLast(operationStack.Pop());
                            }
                            operationStack.Push(currentToken);
                        }
                        break;
                    case TokenEnum.OpenBracket:
                        operationStack.Push(currentToken);

                        if (currentNode.Next != null)
                        {
							if (currentNode.Next.Value.TypeEnum == TokenEnum.CloseBracket)
							{
								Console.WriteLine("Warning: detected brackets with no symbols inside.");
								postfixExpressionTokens.AddLast(new Token(TokenEnum.Number, "0"));
							}
                            else if (currentNode.Next.Value.Sequence.Equals("-"))
                            {
                                // handle if the first term inside the bracket starts with a minus sign
                                postfixExpressionTokens.AddLast(new Token(TokenEnum.Number, "0"));
                            }
                            else if (currentNode.Next.Value.Sequence.Equals("+"))
                            {
								// handle if the first term inside the bracket starts with a plus sign
								postfixExpressionTokens.AddLast(new Token(TokenEnum.Number, "0"));
                            }
                        }
                        break;
                    case TokenEnum.CloseBracket:
                        Token token;
                        bool openBracketFound = false;
                        while ((token = operationStack.Pop()) != null)
                        {
                            if (token.TypeEnum == TokenEnum.OpenBracket)
                            {
                                openBracketFound = true;
                                break;
                            }
                            postfixExpressionTokens.AddLast(token);
                        }

                        if (currentNode.Next != null
                            && (currentNode.Next.Value.TypeEnum == TokenEnum.OpenBracket || currentNode.Next.Value.Sequence.Equals("x")))
                        {
                            tokens.AddAfter(currentNode, new Token(TokenEnum.Multdiv, "*"));
                        }

                        if (!openBracketFound)
                        {
                            throw new EvaluationException("Unmatched parenthesis, expected open bracket.");
                        }

                        break;
                    default:
                        throw new EvaluationException("Unexpected token '{0}', while converting expression to postfix notation.", currentToken.Sequence);
                }

                currentNode = currentNode.Next;

                if (currentNode == null)
                {
                    while (operationStack.Count > 0)
                    {
                        Token token = operationStack.Pop();

                        if (token.TypeEnum == TokenEnum.OpenBracket) throw new EvaluationException("Unmatched parenthesis, expected close bracket.");

                        postfixExpressionTokens.AddLast(token);
                    }
                }
            }

            return postfixExpressionTokens;
        }
        #endregion ToPostfix

        /// <summary>
        /// Checks if the equation is valid.
        /// </summary>
        /// <returns><c>true</c>, if equation valid was valid, <c>false</c> otherwise.</returns>
        /// <param name="equation">The Equation.</param>
        private bool IsEquationValid(string equation) => true && (equation.Count(c => c == '=') <= 2 && equation.Any(c => c == 'x'));

        /// <summary>
        /// Get tokens on the left side of the equation,
        /// before the '=' equal sign
        /// </summary>
        /// <returns>The list of the left tokens.</returns>
        private LinkedList<Token> GetLeft()
        {
            return GetSideTokens(true);
        }

        /// <summary>
        /// Get tokens on the right side of the equation,
        /// before the '=' equal sign
        /// </summary>
        /// <returns>A list of the right tokens.</returns>
        private LinkedList<Token> GetRight()
        {
            return GetSideTokens(false);
        }

        /// <summary>
        /// Helper function to separate tokens from a side of equation,
        /// </summary>
        /// <returns>The side tokens.</returns>
        /// <param name="left">If set to <c>true</c>, returns the left side tokens,
        /// otherwise, returns the right side.</param>
        private LinkedList<Token> GetSideTokens(bool left)
        {
            LinkedList<Token> tokenList = new LinkedList<Token>();

            // The variable left is used as a flag here, if true, it keeps adding the token,
            // when it reads an equal sign, it flips the flag
            foreach (var token in this.equationTokens)
            {
                if (token.TypeEnum == TokenEnum.EqualSign)
                {
                    left = !left;
                    continue;
                }

                if (left)
                    tokenList.AddLast(token);
            }

            return tokenList;
        }
    }
}
