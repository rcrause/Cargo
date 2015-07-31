using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo.Filtering
{
    public interface IFilterCompiler<TContext> where TContext : IContentContext
    {
        CompiledFilter<TContext> Compile(string filterConditionText);
    }
}
