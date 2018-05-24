using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

// http://localhost:6549/WcfService.svc/web/getdata/?data=123
namespace Docker.Tools
{
    [ServiceContract]
    public interface IWcfService
    {
        [OperationContract]
        [WebInvoke(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "GetData/?data={someData}",
            Method = "GET")]
        string GetData(string someData);
    }
}
