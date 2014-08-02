using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Internals;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly DirectedGraph<INodeInfo> graph;
        private readonly ExpressionParser expressionParser;
        private readonly NodeRepository nodeRepository;
        private bool isExecuting;

        public DependencyEngine()
        {
            graph = new DirectedGraph<INodeInfo>();
            nodeRepository = new NodeRepository(this);
            expressionParser = new ExpressionParser(nodeRepository);
        }

        public bool ValueHasChanged(object instance, string key)
        {
            if (!nodeRepository.Contains(instance, key)) return false;
            var node = nodeRepository.Get(instance, key);
            
            if (isExecuting) return false;
            try
            {
                isExecuting = true;
                var orderToReeval = graph.TopologicalSort(node).ToArray();
                orderToReeval.First().Data.ValueChanged();
                foreach (var vertex in orderToReeval.Skip(1))
                {
                    vertex.Data.Reevaluate();
                }
            }
            finally
            {
                isExecuting = false;
            }

            return true;
        }

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty, Expression<Func<TProp>> sourceFunction)
        {
            var targetVertex = expressionParser.GetNodeInfo(targetProperty);
            var valueSink = targetVertex as IValueSink<TProp>;
            var formulaNode = expressionParser.GetNodeInfo(sourceFunction);

            if (valueSink == null)
                throw new Exception("Target expression cannot be written to");

            // TODO We probably need another interface or a base type here to remove cast
            valueSink.SetSource((IValueSource<TProp>)formulaNode);

            graph.AddEdge(formulaNode, targetVertex);
            AddDependenciesToGraph(formulaNode);
        }

        private void AddDependenciesToGraph(INodeInfo formulaNode)
        {
            foreach (var dependency in formulaNode.Dependencies)
            {
                graph.AddEdge(dependency, formulaNode);
                AddDependenciesToGraph(dependency);
            }
        }

        public override string ToString()
        {
            return graph.ToDotLanguage("DependencyGraph");
        }
    }
}