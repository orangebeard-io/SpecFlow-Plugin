using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orangebeard.SpecFlowPlugin.ClientExecution
{
    //NOTE: Originally, LaunchContext implemented ILaunchContext, which was an empty extension of ILogContext ... 

    class LaunchContext : ILogContext
    {
        public ILogScope Log { get; set; }
    }
}
