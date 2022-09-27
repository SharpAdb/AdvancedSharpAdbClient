﻿using System;
using System.Runtime.Serialization;
using Xunit;

namespace AdvancedSharpAdbClient.Tests.Exceptions
{
    /// <summary>
    /// Tests the <see cref="Exception"/> class.
    /// </summary>
    internal static class ExceptionTester<T> where T : Exception
    {
        public static void TestEmptyConstructor(Func<T> constructor)
        {
            T ex = constructor();
        }

        public static void TestMessageConstructor(Func<string, T> constructor)
        {
            string message = "Hello, World";
            T ex = constructor(message);

            Assert.Equal(message, ex.Message);
            Assert.Null(ex.InnerException);
        }

        public static void TestMessageAndInnerConstructor(Func<string, Exception, T> constructor)
        {
            string message = "Hello, World";
            Exception inner = new Exception();
            T ex = constructor(message, inner);

            Assert.Equal(message, ex.Message);
            Assert.Equal(inner, ex.InnerException);
        }

#if !NETCOREAPP1_1
        public static void TestSerializationConstructor(Func<SerializationInfo, StreamingContext, T> constructor)
        {
            SerializationInfo info = new SerializationInfo(typeof(T), new FormatterConverter());
            StreamingContext context = new StreamingContext();

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

            T ex = constructor(info, context);
        }
#endif
    }
}
