using System.Net;

namespace IvaFlashSaleEngine.Exceptions
{
    public class ServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; }

        public ServiceException(string message, string errorCode, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}