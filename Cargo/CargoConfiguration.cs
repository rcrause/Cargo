using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Contains the configuration for a <see cref="CargoEngine"/>.
    /// </summary>
    public class CargoConfiguration
    {
        /// <summary>
        /// The route prefix for the Cargo API.
        /// </summary>
        public string CargoRoutePrefix { get; set; } = "/cargo";
    }
}
