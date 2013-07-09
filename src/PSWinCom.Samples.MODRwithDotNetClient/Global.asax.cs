using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using log4net;

namespace PSWinCom.Samples.MODRwithDotNetClient
{
    public class Global : System.Web.HttpApplication
    {
        private static ILog log = LogManager.GetLogger("Default");

        protected void Application_Start(object sender, EventArgs e)
        {
            log.Info("Application starting");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();
            log.Error("Exception when handling request!", exc);
            Server.ClearError();
        }


        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}