// <copyright file="Element.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.WinRT.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Foundation;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Implement of screen element, likes Selenium.
    /// </summary>
    public sealed class Element
    {
        internal readonly AdvancedSharpAdbClient.Element element;

        /// <summary>
        /// Contains element coordinates.
        /// </summary>
        public Cords Cords
        {
            get => Cords.GetCords( element.Cords);
            set => element.Cords = value.cords;
        }

        /// <summary>
        /// Gets or sets element attributes.
        /// </summary>
        public IDictionary<string, string> Attributes
        {
            get => element.Attributes;
            set => element.Attributes = value.GetDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="cords">Contains element coordinates .</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(AdbClient client, DeviceData device, Cords cords, IDictionary<string, string> attributes) =>
            element = new(client.adbClient, device.deviceData, cords.cords, attributes.GetDictionary());

        internal Element(AdvancedSharpAdbClient.Element element) => this.element = element;

        internal static Element GetElement(AdvancedSharpAdbClient.Element element) => new(element);

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public void Click() => element.Click();

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public IAsyncAction ClickAsync() => element.ClickAsync().AsAsyncAction();

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        /// <param name="timeout">A <see cref="System.TimeSpan"/> which can be used to cancel the asynchronous task.</param>
        public IAsyncAction ClickAsync(TimeSpan timeout) => element.ClickAsync(timeout.GetCancellationToken()).AsAsyncAction();

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string text) => element.SendText(text);


        /// <summary>
        /// Clear the input text. The input should be in focus. Use el.ClearInput() if the element isn't focused.
        /// </summary>
        public void ClearInput() => element.ClearInput();

        /// <summary>
        /// Clear the input text. The input should be in focus. Use el.ClearInput() if the element isn't focused.
        /// </summary>
        /// <param name="charCount"></param>
        public void ClearInput(int charCount) => element.ClearInput(charCount);
    }
}
