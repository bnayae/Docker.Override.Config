using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json;
using System.Web;
using System.Diagnostics;
using System.ServiceModel.Activation;
using System.Configuration;
using System;

namespace Docker.Tools
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WcfService : IWcfService
    {
        public string GetData(string someData)
        {
            string cfg = "undefined";
            string conn = "undefined";
            string envAbc = "undefined";
            string envXyz = "undefined";
            string error = "";
            try
            {
                cfg = ConfigurationManager.AppSettings["xyz"] ?? "undefined";
                conn = ConfigurationManager.ConnectionStrings["abc"]?.ConnectionString ?? "undefined";
                envAbc = Environment.GetEnvironmentVariable("abc") ?? "undefined";
                envXyz = Environment.GetEnvironmentVariable("xyz") ?? "undefined";
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            return $"@ {someData}: app-setting: {envXyz} -> {cfg}, Connection string: {envAbc} -> {conn}, error = {error}";
        }
    }
}
