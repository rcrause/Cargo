using Cargo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoExample
{
    public class MyCargoEngine : CargoEngine
    {
        public override bool AuthenticateRequest(IDictionary<string, object> environment)
        {
            return true;
        }

        public override ICargoDataSource CreateDataSource()
        {
            return new EntityFrameworkCargoDataSource(new MyDataContext());
        }
    }
}