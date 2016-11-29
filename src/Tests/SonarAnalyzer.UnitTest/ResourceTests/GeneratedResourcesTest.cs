﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.UnitTest.ResourceTests
{
    [TestClass]
    public class GeneratedResourcesTest
    {
        [TestMethod]
        public void AnalyzersHaveCorrespondingResource_CS()
        {
            var rulesFromResources = GetRulesFromResources(@"..\..\..\..\..\rspec\cs");

            var rulesFromClasses = GetRulesFromClasses(typeof(SyntaxHelper).Assembly);

            CollectionAssert.AreEqual(rulesFromClasses, rulesFromResources);
        }

        [TestMethod]
        public void AnalyzersHaveCorrespondingResource_VB()
        {
            var rulesFromResources = GetRulesFromResources(@"..\..\..\..\..\rspec\vbnet");

            var rulesFromClasses = GetRulesFromClasses(typeof(SonarAnalyzer.Helpers.VisualBasic.SyntaxHelper).Assembly);

            CollectionAssert.AreEqual(rulesFromClasses, rulesFromResources);
        }

        private static string[] GetRulesFromClasses(Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(typeof(SonarDiagnosticAnalyzer).IsAssignableFrom)
                           .Where(t => !t.IsAbstract)
                           .Where(IsNotUtilityAnalyzer)
                           .Select(GetRuleNameFromAttributes)
                           .OrderBy(name => name)
                           .ToArray();
        }

        private static bool IsNotUtilityAnalyzer(Type arg)
        {
            return !typeof(UtilityAnalyzerBase).IsAssignableFrom(arg);
        }

        private static string[] GetRulesFromResources(string relativePath)
        {
            var resourcesRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));

            var resources = Directory.GetFiles(resourcesRoot);

            var ruleNames = resources.Where(IsHtmlFile)
                                     .Select(Path.GetFileNameWithoutExtension)
                                     .Select(GetRuleFromFileName)
                                     .OrderBy(name => name)
                                     .ToArray();

            return ruleNames;
        }

        private static string GetRuleNameFromAttributes(Type analyzerType)
        {
            var name = analyzerType.GetCustomAttributes(typeof(RuleAttribute), true)
                                .OfType<RuleAttribute>()
                                .FirstOrDefault()
                                ?.Key;
            return name;
        }

        private static string GetRuleFromFileName(string fileName)
        {
            // S1234_c# or S1234_vb.net
            var match = Regex.Match(fileName, @"(S\d+)_.*");

            return match.Success ? match.Groups[1].Value : null;
        }

        private static bool IsHtmlFile(string name)
        {
            return Path.GetExtension(name).Equals(".html", StringComparison.OrdinalIgnoreCase);
        }
    }
}