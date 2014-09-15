using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class FormulaDefinition<T> : ExpressionDefinition, ISourceDefinition<T>
    {
        readonly Expression<Func<T, T>> sourceExpression;

        public FormulaDefinition(Expression<Func<T, T>> sourceExpression, string nodeId) : 
            base(sourceExpression, NodeType.Formula, nodeId)
        {
            this.sourceExpression = sourceExpression;
            PathToParent = null;
            SourcePaths = new List<ISourceDefinition>();
        }

        public Func<T> CreateGetValueDelegate()
        {
            throw new NotSupportedException("Formulas should always be re-evaluated with a provided current value.");
        }

        public string PathToParent { get; private set; }

        public Func<T, T> CreateGetValueDelegateWithCurrentValue()
        {
            return sourceExpression.Compile();
        }

        public List<ISourceDefinition> SourcePaths { get; private set; }

        public Type SourceType { get { return typeof (T); } }
    }
}