﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eocron.Core.Ast;
using Eocron.Core.FinitieStateAutomaton.Predicates;

namespace Eocron.Core.FinitieStateAutomaton
{
    /// <summary>
    /// Compiled FSA
    /// </summary>
    public sealed class CFSA<TValue> : IFSA<TValue>
    {
        public bool ExactBegin { get; set; }
        public bool ExactEnd { get; set; }

        public string Name { get; private set; }

        private readonly IFSATransition<TValue>[][] _transitionMatrix;

        private readonly int _startState;

        private readonly bool[] _finalsLookup;

        public IEnumerable<IFSATransition<TValue>> Transitions
        {
            get { return _transitionMatrix.Where(x=> x!= null).SelectMany(x => x); }
        }

        public CFSA(FSA<TValue> fsa)
        {
            ExactBegin = fsa.ExactBegin;
            ExactEnd = fsa.ExactEnd;
            Name = fsa.Name;
            _transitionMatrix = new IFSATransition<TValue>[fsa.StateCount][];
            foreach (var look in fsa.Transitions.ToLookup(x => x.From, x => x))
            {
                _transitionMatrix[look.Key] = look.ToArray();
            }

            _startState = fsa.Q0.First();
            _finalsLookup = new bool[_transitionMatrix.Length];
            foreach (var f in fsa.F)
            {
                _finalsLookup[f] = true;
            }
        }

        private sealed class FSMState
        {
            public int CurrentIndex;
            public int CurrentPredicateIndex;
            public IFSATransition<TValue>[] Transitions;
            public bool IsFinal;
        }

        [ThreadStatic]
        private readonly Stack<FSMState> _globalStack = new Stack<FSMState>(); 

        public bool TryRun(TValue[] values, int startIndex, out Range range)
        {
            range = default(Range);
            var stack = _globalStack;
            stack.Clear();

            FSMState state = CreateState(_startState, startIndex);
            stack.Push(state);
            while (stack.Count > 0)
            {
                state = stack.Peek();

                FSMState next;
                if (TryGetNextState(state, values, out next))
                {
                    stack.Push(next);
                }
                else if (state.IsFinal)
                {
                    break;
                }
                else
                {
                    state = stack.Pop();
                }
            }

            if (stack.Count != 0 && state.IsFinal)
            {
                range = new Range(startIndex, state.CurrentIndex - startIndex);
                return true;
            }
            return false;
        }

        public bool IsFinal(int state)
        {
            return _finalsLookup[state];
        }

        private FSMState CreateState(int state, int index)
        {
            return new FSMState
            {
                CurrentIndex = index,
                CurrentPredicateIndex = 0,
                Transitions = _transitionMatrix[state],
                IsFinal = _finalsLookup[state],
            };
        }

        private bool TryGetNextState(FSMState current, TValue[] values, out FSMState nextState)
        {
            nextState = default(FSMState);
            if (current.Transitions != null && 
                current.Transitions.Length > 0 &&
                current.CurrentPredicateIndex != current.Transitions.Length &&
                current.CurrentIndex <= values.Length)
            {
                for (int i = current.CurrentPredicateIndex; i < current.Transitions.Length; i++)
                {
                    var trans = current.Transitions[i];
                    current.CurrentPredicateIndex++;

                    var isMatch = trans.Condition.IsMatch(values, current.CurrentIndex);
                    if (isMatch)
                    {
                        var isEps = PredicateEdgeBase<TValue>.IsEpsilon(trans.Condition);
                        nextState = CreateState(trans.To, current.CurrentIndex + (isEps ? 0 : 1));
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
