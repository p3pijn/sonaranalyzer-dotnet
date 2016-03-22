﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015-2016 SonarSource SA
 * mailto:contact@sonarsource.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarLint.Rules.CSharp;

namespace SonarLint.UnitTest.Rules
{
    [TestClass]
    public class EmptyMethodTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void EmptyMethod()
        {
            Verifier.VerifyAnalyzer(@"TestCases\EmptyMethod.cs", new EmptyMethod());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void EmptyMethod_CodeFix_Throw()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Throw.Fixed.cs",
                new EmptyMethod(),
                new EmptyMethodCodeFixProvider(),
                EmptyMethodCodeFixProvider.TitleThrow);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void EmptyMethod_CodeFix_Comment()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Comment.Fixed.cs",
                new EmptyMethod(),
                new EmptyMethodCodeFixProvider(),
                EmptyMethodCodeFixProvider.TitleComment);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void EmptyMethod_CodeFix_Constructor()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Constructor.Fixed.cs",
                new EmptyMethod(),
                new EmptyMethodCodeFixProvider(),
                EmptyMethodCodeFixProvider.TitleRemoveConstructor);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void EmptyMethod_CodeFix_Destructor()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Destructor.Fixed.cs",
                new EmptyMethod(),
                new EmptyMethodCodeFixProvider(),
                EmptyMethodCodeFixProvider.TitleRemoveDestructor);
        }
    }
}
