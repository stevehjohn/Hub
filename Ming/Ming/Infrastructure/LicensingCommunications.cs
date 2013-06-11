using ServiceStack.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Ming.Infrastructure
{
    internal class LicensingCommunications
    {
        private static string _server = "http://localhost";
        //private static string _server = "http://outsidecontextproblem.com";

        public static TResponse SendMessage<TResponse, TRequest>(string path, TRequest parms)
        {
            var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}{1}", _server, path));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            
            var reqData = ObjectToJsonByteArray<TRequest>(parms);
            request.ContentLength = reqData.Length;

            var reqStream = request.GetRequestStream();

            reqStream.Write(reqData, 0, reqData.Length);
            reqStream.Close();

            var response = (HttpWebResponse)request.GetResponse();

            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

            var json = reader.ReadToEnd();
            reader.Close();

            return json.FromJson<TResponse>();
        }

        private static byte[] ObjectToJsonByteArray<T>(T obj) 
        {
            var json = obj.ToJson<T>();

            return json.Select(c => (byte)c).ToArray();
        }
    }
}
