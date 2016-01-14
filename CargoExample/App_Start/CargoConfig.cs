using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cargo;

namespace CargoExample
{
    public class CargoConfig : DefaultCargoConfiguration
    {
        public CargoConfig()
        {
        }

        public override bool AuthenticateRequest(IDictionary<string, object> environment)
        {
            return true;
        }

        public override ICargoDataSource GetDataSource()
        {
            return new EntityFrameworkCargoDataSource(new MyDataContext());
        }
    }
}