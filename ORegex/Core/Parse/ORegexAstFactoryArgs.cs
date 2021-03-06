﻿using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Eocron.Core.FinitieStateAutomaton.Predicates;

namespace Eocron.Core.Parse
{
    public sealed class ORegexAstFactoryArgs<TValue>
    {
        private readonly PredicateTable<TValue> _predicateTable;
        private readonly RegexGrammarParser _parser;

        public List<string> CaptureGroupNames;

        public int CurrentCaptureGroupId = 0;

        public ORegexAstFactoryArgs(PredicateTable<TValue> predicateTable, RegexGrammarParser parser)
        {
            _predicateTable = new PredicateTable<TValue>(predicateTable.ThrowIfNull());
            _parser = parser.ThrowIfNull();
            CaptureGroupNames = new List<string>();
        }

        public bool IsName(IParseTree node, string name)
        {
            return name == GetName(node);
        }

        public string GetName(IParseTree node)
        {
            var rule = (RuleContext)node;
            var name = _parser.RuleNames[rule.RuleIndex];
            return name;
        }

        public PredicateEdgeBase<TValue> GetPredicate(string atomName)
        {
            return _predicateTable.GetPredicate(atomName);
        }

        public void GetInvertedPredicate(IEnumerable<string> names, out PredicateEdgeBase<TValue> predicate, out string invertedName)
        {
            names = names.Distinct().OrderByDescending(x => x);
            invertedName = string.Format("#not in: {0}",string.Join(",", names));
            if (!_predicateTable.Contains(invertedName))
            {
                var predicates = names.Select(GetPredicate).Distinct().ToArray();

                Func<SequenceHandler<TValue>, int, bool> invertedPredicate = (v,i) =>
                {
                    return !predicates.Any(p => p.IsMatch(v,i));
                };
                _predicateTable.AddPredicate(invertedName, invertedPredicate);
            }

            predicate = GetPredicate(invertedName);
        }
    }
}
