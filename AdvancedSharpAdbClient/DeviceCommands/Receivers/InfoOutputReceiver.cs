// <copyright file="InfoOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers
{
    /// <summary>
    /// Processes command line output of a <c>adb</c> shell command.
    /// </summary>
    [DebuggerDisplay($"{nameof(InfoOutputReceiver)} \\{{ {nameof(Properties)} = {{{nameof(Properties)}}}, {nameof(PropertyParsers)} = {{{nameof(PropertyParsers)}}} }}")]
    public class InfoOutputReceiver : ShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoOutputReceiver"/> class.
        /// </summary>
        public InfoOutputReceiver() { }

        /// <summary>
        /// Gets or sets a dictionary with the extracted properties and their corresponding values.
        /// </summary>
        private Dictionary<string, object?> Properties { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary with all properties and their corresponding property parsers.
        /// A property parser extracts the property value out of a <see cref="string"/> if possible.
        /// </summary>
        private Dictionary<string, Func<string, object?>> PropertyParsers { get; set; } = [];

        /// <summary>
        /// Gets the value of the property out of the Properties dictionary.
        /// Returns null if the property is not present in the directory.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The received value</returns>
        public object? GetPropertyValue(string propertyName) => Properties.TryGetValue(propertyName, out object? property) ? property : null;

        /// <summary>
        /// Gets the value of the property out of the Properties dictionary.
        /// Returns null if the property is not present in the directory.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The received value.</returns>
        [return: MaybeNull]
        public T GetPropertyValue<T>(string propertyName) => Properties.TryGetValue(propertyName, out object? property) && property is T value ? value : default;

        /// <summary>
        /// Adds a new parser to this receiver.
        /// The parsers parses one received line and extracts the property value if possible.
        /// The parser should return <c>null</c> if the property value cannot be found in the line.
        /// </summary>
        /// <param name="property">The property corresponding with the parser.</param>
        /// <param name="parser">Function parsing one string and returning the property value if possible. </param>
        public void AddPropertyParser(string property, Func<string, object?> parser) => PropertyParsers[property] = parser;

        /// <inheritdoc/>
        public override bool AddOutput(string line)
        {
            if (line == null)
            {
                return true;
            }

            foreach (KeyValuePair<string, Func<string, object?>> parser in PropertyParsers)
            {
                object? propertyValue = parser.Value(line);
                if (propertyValue != null)
                {
                    Properties[parser.Key] = propertyValue;
                }
            }
            return true;
        }
    }
}
