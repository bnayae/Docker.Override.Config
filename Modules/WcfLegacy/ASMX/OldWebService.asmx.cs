using System;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;

namespace Bnaya.Samples.OldWebServiceServices
{
    [WebServiceBinding(ConformsTo = WsiProfiles.None)]
    [WebService(Name = "LegacyWebService", Namespace = "http://www.sample.com/default")]
    [SoapDocumentService(ParameterStyle = SoapParameterStyle.Default, RoutingStyle = SoapServiceRoutingStyle.SoapAction, Use = SoapBindingUse.Encoded)]
    public class OldWebService : WebService
    {
        [SoapDocumentMethod(Action = @"LegacyWebService#Send", Use = SoapBindingUse.Encoded, ParameterStyle = SoapParameterStyle.Default)]
        [WebMethod]
        public string Send(string data)
        {
            string cfg = "undefined";
            string conn = "undefined";
            string envAbc ="undefined";
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
            return $"@ {data}: app-setting: {envXyz} -> {cfg}, Connection string: {envAbc} -> {conn}, error = {error}";
        }
    }
}
