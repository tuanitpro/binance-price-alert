using System;

namespace WindowsFormsApp1.Core
{
    public class JsonConversionException : Exception
    {
        public string Json { get; }
        public Type TargetType { get; }

        public JsonConversionException(string message, Exception innerException, string json, Type targetType) : base(message, innerException)
        {
            this.Json = json;
            this.TargetType = targetType;
        }
    }
}