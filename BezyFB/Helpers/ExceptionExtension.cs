using System;

namespace BezyFB.Helpers
{
    public static class ExceptionExtension
    {
        public static Exception GetInner(this Exception exception)
        {
            if (null != exception.InnerException)
            {
                return exception.InnerException.GetInner();
            }
            return exception;
        }
    }
}