using System;
using System.Runtime.Serialization;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    /// <summary>
    /// Tests the <see cref="Exception"/> class.
    /// </summary>
    internal static class ExceptionTester<T> where T : Exception
    {
        public static void EmptyConstructorTest(Func<T> constructor) => _ = constructor();

        public static void MessageConstructorTest(Func<string, T> constructor)
        {
            string message = "Hello, World";
            T ex = constructor(message);

            Assert.Equal(message, ex.Message);
            Assert.Null(ex.InnerException);
        }

        public static void MessageAndInnerConstructorTest(Func<string, Exception, T> constructor)
        {
            string message = "Hello, World";
            Exception inner = new();
            T ex = constructor(message, inner);

            Assert.Equal(message, ex.Message);
            Assert.Equal(inner, ex.InnerException);
        }

        [Obsolete]
        public static void SerializationConstructorTest(Func<SerializationInfo, StreamingContext, T> constructor)
        {
            SerializationInfo info = new(typeof(T), new FormatterConverter());
            StreamingContext context = new();

            info.AddValue("ClassName", string.Empty);
            info.AddValue("Message", string.Empty);
            info.AddValue("InnerException", new ArgumentException());
            info.AddValue("HelpURL", string.Empty);
            info.AddValue("StackTraceString", string.Empty);
            info.AddValue("RemoteStackTraceString", string.Empty);
            info.AddValue("RemoteStackIndex", 0);
            info.AddValue("ExceptionMethod", string.Empty);
            info.AddValue("HResult", 1);
            info.AddValue("Source", string.Empty);

            _ = constructor(info, context);
        }
    }
}
