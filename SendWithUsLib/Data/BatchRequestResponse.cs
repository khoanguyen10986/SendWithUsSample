using Sendwithus;

namespace SendWithUsLib
{
    public class BatchRequestResponse : GenericApiCallStatus
    {
        public string message { get; set; }
        public string jsonData { get; set; }

        public BatchRequestResponse(bool success, string status, string jsonData = "", string message = "")
        {
            base.success = success;
            base.status = status;
            this.message = message;
            this.jsonData = jsonData;
        }
    }
}
