using System;

namespace BezyFB_UWP.Lib.T411
{
    public class ErrorCodeException : Exception
    {
        public int ErrorCode { get; protected set; }

        public ErrorCodeException()
        {
        }

        public ErrorCodeException(string message)
            : base(message)
        {
        }

        public ErrorCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ErrorCodeException(string message, int code)
            : base(message)
        {
            ErrorCode = code;
        }

        public static ErrorCodeException CreateFromErrorCode(ErrorResult errorResult)
        {
            if (errorResult == null)
                throw new ArgumentNullException("errorResult");

            return new ErrorCodeException(errorResult.Error, errorResult.Code);
        }
    }
}