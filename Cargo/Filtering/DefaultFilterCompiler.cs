using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo.Filtering
{
    public class FilterCompiler<TContext> : IFilterCompiler<TContext> where TContext : IContentContext
    {
        public CompiledFilter<TContext> Compile(string filterConditionText)
        {
            throw new NotImplementedException();
        }
    }
}
