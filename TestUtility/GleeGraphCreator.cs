﻿using Microsoft.Glee.Drawing;
using ORegex;
using ORegex.Core.FinitieStateAutomaton;

namespace TestUtility
{
    public sealed class GleeGraphCreator
    {
        public Graph Create<TValue>(FSA<TValue> fsa, PredicateTable<TValue> table)
        {
            var graph = new Graph(fsa.Name);
            FillGraph(graph, fsa, table);
            return graph;
        }

        public Graph Create<TValue>(CFSA<TValue> fsa, PredicateTable<TValue> table)
        {
            var graph = new Graph(fsa.Name);
            FillGraph(graph, fsa, table);
            return graph;
        }

        private static void FillGraph<TValue>(Graph graph, FSA<TValue> fsm, PredicateTable<TValue> table)
        {
            foreach (var t in fsm.Transitions)
            {
                Edge edge = null;
                if (t.Info is FSAPredicateEdge<TValue>)
                {
                    var info = (FSAPredicateEdge<TValue>)t.Info;
                    edge = graph.AddEdge("q" + t.StartState, table.GetName(info.Predicate), "q" + t.EndState);
                }
                else if (t.Info is FSACaptureEdge<TValue>)
                {
                    var info = (FSACaptureEdge<TValue>)t.Info;
                    edge = graph.AddEdge("q" + t.StartState, info.InnerFsa.Name, "q" + t.EndState);
                }
                if (fsm.F.Contains(t.EndState))
                {
                    edge.TargetNode.Attr.Fillcolor = Color.Gray;
                    edge.TargetNode.Attr.Shape = Shape.DoubleCircle;
                }
            }
        }

        private static void FillGraph<TValue>(Graph graph, CFSA<TValue> fsa, PredicateTable<TValue> table)
        {
            foreach (var t in fsa.Transitions)
            {
                Edge edge = null;
                if (t.Condition != null)
                {
                    edge = graph.AddEdge("q" + t.StartState, table.GetName(t.Condition), "q" + t.EndState);
                }
                else if (t.InnerFSA != null)
                {
                    edge = graph.AddEdge("q" + t.StartState, t.InnerFSA.Name, "q" + t.EndState);
                }
                if (fsa.IsFinal(t.EndState))
                {
                    edge.TargetNode.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.Gray;
                    edge.TargetNode.Attr.Shape = Shape.DoubleCircle;
                }
            }
        }
    }
}