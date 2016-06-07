using System;

namespace CommonPortableLib
{
    public class OperationResult<T>
    {
        public Exception Exception { get; set; }
        public bool IsOk { get; set; }
        public T Result { get; set; }

        public string Message { get; set; }

    }
    public static class OperationResult
    {
        public static OperationResult<T> CreateOk<T>(T value)
        {
            return new OperationResult<T>
            {
                IsOk = true,
                Result = value
            };
        }

        public static OperationResult<T> CreateKo<T>(string message)
        {
            return new OperationResult<T>
            {
                IsOk = false,
                Message = message
            };
        }
        public static OperationResult<T> CreateKo<T>(Exception exception)
        {
            return new OperationResult<T>
            {
                IsOk = false,
                Exception = exception
            };
        }

    }
}