using System;
using System.Text.RegularExpressions;
using Equ.Enums;

namespace Equ.Domain
{
    /// <summary>
    /// Token type. see TokenEnum
    /// </summary>
    public class TokenType
    {
		/// <summary>
		/// Gets or sets the rgx.
		/// </summary>
		/// <value>The regular expression used to match and identify tokens in a string</value>
		public Regex Rgx { get; set; }

        /// <summary>
        /// Gets or sets the type enum.
        /// </summary>
        /// <value>The type enum.</value>
        public TokenEnum TypeEnum { get; set; }

        public TokenType(Regex rgx, TokenEnum type)
        {
            Rgx = rgx;
            TypeEnum = type;
        }
    }
}
