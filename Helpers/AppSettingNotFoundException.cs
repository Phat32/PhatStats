using System;
using System.Runtime.Serialization;

namespace Phat_Stats.Helpers
{
    [Serializable]
    internal class AppSettingNotFoundException : Exception
    {
        public AppSettingNotFoundException()
        {
        }

        public AppSettingNotFoundException(string message) : base(message)
        {
        }

        public AppSettingNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AppSettingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}