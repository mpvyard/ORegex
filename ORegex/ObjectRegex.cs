﻿using ORegex.Core.Parse;
using System;
using System.Collections.Generic;
using ORegex.Core;
using ORegex.Core.Ast;
using ORegex.Core.FinitieStateAutomaton;

namespace ORegex
{
    /// <summary>
    /// Objected regex.  Very useful for simple tasks with a little amount of predicates.
    /// For use just type {number} instead of character like you usualliy write regex.
    /// Number can be from 0 to MaxPredicatesCount-1.
    /// Example of pattern: {0}(.{0})* where {0} -> isPrime(x) on sequence: 1 2 3 4 5 6 7 8 9 10 11 12 13
    /// Syntax to write patterns is RegularExpressions.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class ObjectRegex<TValue>
    {
        private readonly ORegexCompiler<TValue> _compiler = new ORegexCompiler<TValue>();
        private readonly CFSA<TValue> _cfsa;

        public ObjectRegex(string pattern, ORegexOptions options, params Func<TValue, bool>[] predicates) : this(pattern, options, CreatePredicateTable(predicates))
        {
        }

        public ObjectRegex(string pattern, ORegexOptions options, PredicateTable<TValue> table)
        {
            _cfsa = _compiler.Build(pattern, table);
        }

        private static PredicateTable<TValue> CreatePredicateTable(Func<TValue, bool>[] predicates)
        {
            var table = new PredicateTable<TValue>();
            for (int i = 0; i < predicates.Length; i++)
            {
                table.AddPredicate(i.ToString(), predicates[i]);
            }
            return table;
        }

        public IEnumerable<ObjectMatch<TValue>> Matches(TValue[] values)
        {
            var captureTable = new CaptureTable<TValue>();
            for (int i = 0; i < values.Length; i++)
            {
                var capture = _cfsa.Run(values, i, captureTable);
                if (!capture.Equals(Range.Invalid))
                {
                    var match = new ObjectMatch<TValue>(values, captureTable, capture);
                    i += capture.Length - 1;
                    yield return match;
                }
            }
        }
    }
}
