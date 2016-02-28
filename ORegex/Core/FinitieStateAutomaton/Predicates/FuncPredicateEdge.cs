﻿using System;
using System.Diagnostics;
using Eocron.Core.Ast;

namespace Eocron.Core.FinitieStateAutomaton.Predicates
{
    [DebuggerDisplay("(Predicate, {_condition.GetHashCode()})")]
    public sealed class FuncPredicateEdge<TValue> : PredicateEdgeBase<TValue>
    {
        public static readonly FuncPredicateEdge<TValue> Epsilon = new FuncPredicateEdge<TValue>(x => { throw new NotImplementedException("Epsilon condition."); });

        public static readonly FuncPredicateEdge<TValue> AlwaysTrue = new FuncPredicateEdge<TValue>(x => true);

        internal Func<TValue, bool> _condition { get;private set; }

        public FuncPredicateEdge(Func<TValue, bool> condition)
        {
            _condition = condition;
        }

        public override bool IsFuncPredicate
        {
            get { return true; }
        }

        public override bool IsComparePredicate
        {
            get { return false; }
        }

        public override bool IsComplexPredicate
        {
            get { return false; }
        }

        public override Range Match(TValue[] sequence, int startIndex, out OCaptureTable<TValue> table)
        {
            table = null;
            if (_condition(sequence[startIndex]))
            {
                return new Range(startIndex, 1);
            }
            return Range.Invalid;
        }

        public override int GetHashCode()
        {
            return _condition.GetHashCode();
        }

        public override bool IsMatch(TValue value)
        {
            return _condition(value);
        }
    }
}
