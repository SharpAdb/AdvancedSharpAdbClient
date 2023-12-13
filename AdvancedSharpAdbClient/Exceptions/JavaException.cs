using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with the Java exception output.
    /// </summary>
    [Serializable]
    public partial class JavaException : Exception
    {
        /// <summary>
        /// Unknown error message.
        /// </summary>
        private const string UnknownError = "An error occurred in Java";

        /// <summary>
        /// The output of the Java exception.
        /// </summary>
        private const string ExceptionOutput = "java.lang.";

        /// <summary>
        /// The pattern of the Java exception.
        /// </summary>
        private const string ExceptionPattern = @"java.lang.(\w+Exception):\s+(.*)?";

        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that represent a new line.
        /// </summary>
        private static readonly char[] separator = Extensions.NewLineSeparator;

        /// <summary>
        /// Gets the name of Java exception.
        /// </summary>
        public string? JavaName { get; init; }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack of Java exception.
        /// </summary>
        public string? JavaStackTrace { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaException"/> class.
        /// </summary>
        /// <param name="name">The name of Java exception.</param>
        /// <param name="stackTrace">The stackTrace of Java exception.</param>
        public JavaException(string? name, string? stackTrace) : base(UnknownError)
        {
            JavaName = name;
            JavaStackTrace = stackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaException"/> class.
        /// </summary>
        /// <param name="name">The name of Java exception.</param>
        /// <param name="message">The message of Java exception.</param>
        /// <param name="stackTrace">The stackTrace of Java exception.</param>
        public JavaException(string? name, string? message, string? stackTrace) : base(message)
        {
            JavaName = name;
            JavaStackTrace = stackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaException"/> class.
        /// </summary>
        /// <param name="name">The name of Java exception.</param>
        /// <param name="message">The message of Java exception.</param>
        /// <param name="stackTrace">The stackTrace of Java exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public JavaException(string? name, string? message, string? stackTrace, Exception? innerException) : base(message, innerException)
        {
            JavaName = name;
            JavaStackTrace = stackTrace;
        }

#if HAS_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
#if NET8_0_OR_GREATER
        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
        public JavaException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
#endif

        /// <summary>
        /// Creates a <see cref="JavaException"/> from it <see cref="string"/> representation.
        /// </summary>
        /// <param name="line">A <see cref="string"/> which represents a <see cref="JavaException"/>.</param>
        /// <returns>The equivalent <see cref="JavaException"/>.</returns>
        public static JavaException Parse(string line) => Parse(line.Split(separator, StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Creates a <see cref="JavaException"/> from it <see cref="string"/> representation.
        /// </summary>
        /// <param name="lines">A <see cref="IEnumerable{String}"/> which represents a <see cref="JavaException"/>.</param>
        /// <returns>The equivalent <see cref="JavaException"/>.</returns>
        public static JavaException Parse(IEnumerable<string> lines)
        {
            string exception = string.Empty, message = string.Empty;
            StringBuilder stackTrace = new();
            Regex exceptionRegex = ExceptionRegex();

            foreach (string line in lines)
            {
                if (line.Length > 0)
                {
                    if (line.StartsWith(ExceptionOutput))
                    {
                        Match m = exceptionRegex.Match(line);
                        if (m.Success)
                        {
                            exception = m.Groups[1].Value;

                            message = m.Groups[2].Value;
                            message = StringExtensions.IsNullOrWhiteSpace(message) ? UnknownError : message;
                        }
                    }
                    else if (!StringExtensions.IsNullOrWhiteSpace(line))
                    {
                        stackTrace.AppendLine(line.TrimEnd());
                    }
                }
            }

            return new JavaException(exception, message, stackTrace.ToString().TrimEnd());
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(ExceptionPattern, RegexOptions.IgnoreCase)]
        private static partial Regex ExceptionRegex();
#else
        /// <summary>
        /// Gets a <see cref="Regex"/> that can be used to parse the Java exception.
        /// </summary>
        /// <returns>The <see cref="Regex"/> that can be used to parse the Java exception.</returns>
        private static Regex ExceptionRegex() => new(ExceptionPattern, RegexOptions.IgnoreCase);
#endif
    }
}
