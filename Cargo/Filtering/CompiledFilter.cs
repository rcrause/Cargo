using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo.Filtering
{
    public abstract class CompiledFilter<TContext> where TContext : IContentContext
    {
        public CompiledFilter(string filterCondition)
        {
            FilterCondition = filterCondition;
        }

        public string FilterCondition { get; private set; }

        public abstract bool Evaluate(TContext context);
    }
}