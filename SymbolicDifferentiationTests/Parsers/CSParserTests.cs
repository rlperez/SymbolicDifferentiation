﻿#region License

/* ****************************************************************************
 * Copyright (c) Edmondo Pentangelo. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. 
 * A copy of the license can be found in the License.html file at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 * ***************************************************************************/

#endregion

using System.Collections.Generic;
using SymbolicDifferentiation.Core.AST;
using SymbolicDifferentiation.Core.Tokens;
using SymbolicDifferentiation.Extensions;
using System.Linq;

namespace SymbolicDifferentiation.Tests.Parsers
{
    public class CSParserTests : ParserTests
    {
        protected override Expression Parse(IEnumerable<Token> tokens)
        {
            return ParseMultiple(tokens).Single();
        }

        protected override IEnumerable<Expression> ParseMultiple(IEnumerable<Token> tokens)
        {
            return tokens.CSParser();
        }
    }
}