using OCP.ApiModel;
using System.Web.Http;

namespace OCP.Controllers
{
    public class FirstExecutionController : ApiController
    {
        public Acknowledgement FirstExecution(TrialInfo data)
        {
            data.Save();

            return new Acknowledgement { Message = "OK" };
        }
    }
}
