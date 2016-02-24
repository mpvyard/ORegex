﻿using System.Collections.Generic;
using ORegex.Core.Ast;

namespace ORegex.Core.FinitieStateAutomaton
{
    public sealed class ComparePredicateEdge<TValue> : PredicateEdgeBase<TValue>
    {
        internal readonly IEqualityComparer<TValue> _comparer;
        internal readonly TValue _value;
        
        public ComparePredicateEdge(TValue value, IEqualityComparer<TValue> comparer = null) : base(true)
        {
            _comparer = comparer ?? EqualityComparer<TValue>.Default;
            _value = value;
        }

        public override bool IsFuncPredicate
        {
            get { return false; }
        }

        public override bool IsComparePredicate
        {
            get { return true; }
        }

        public override bool IsComplexPredicate
        {
            get { return false; }
        }

        public override Range Match(TValue[] sequence, int startIndex)
        {
            if (_comparer.Equals(_value, sequence[startIndex]))
            {
                return new Range(startIndex, 1);
            }
            return Range.Invalid;
        }

        public override bool IsMatch(TValue value)
        {
            return _comparer.Equals(value, _value);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode() ^ _value.GetHashCode();
        }
    }
}
