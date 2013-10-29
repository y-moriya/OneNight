using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneNightWerewolf
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            //DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();
            //dmc.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = TimeSpan.FromMinutes(2);
            //dmc.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Critical;

            //DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmc);

            return base.OnStart();
        }
    }
}