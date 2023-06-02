// <copyright file="Element.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implement of screen element, likes Selenium.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// The current ADB client that manages the connection.
        /// </summary>
        private IAdbClient Client { get; set; }

        /// <summary>
        /// The current device containing the element.
        /// </summary>
        private DeviceData Device { get; set; }

        /// <summary>
        /// The coordinates and size of the element.
        /// </summary>
        public Area Area { get; set; }

        /// <summary>
        /// The coordinates of the element to click. Default is the center of area.
        /// </summary>
        public Cords Cords { get; set; }

        /// <summary>
        /// Gets or sets element attributes.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="cords">The coordinates of the element to click.</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, Cords cords, Dictionary<string, string> attributes)
        {
            Client = client;
            Device = device;
            Cords = cords;
            Attributes = attributes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="area">The coordinates and size of the element.</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, Area area, Dictionary<string, string> attributes)
        {
            Client = client;
            Device = device;
            Area = area;
            Attributes = attributes;
            Cords = area.Center; // Average x1, y1, x2, y2
        }

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public void Click() => Client.Click(Device, Cords);

#if HAS_TASK
        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        public async Task ClickAsync(CancellationToken cancellationToken = default) =>
            await Client.ClickAsync(Device, Cords, cancellationToken);
#endif

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        public void SendText(string text)
        {
            Click();
            Client.SendText(Device, text);
        }

#if HAS_TASK
        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SendTextAsync(string text, CancellationToken cancellationToken = default)
        {
            await ClickAsync(cancellationToken);
            await Client.SendTextAsync(Device, text, cancellationToken);
        }
#endif

        /// <summary>
        /// Clear the input text. Use <see cref="IAdbClient.ClearInput(DeviceData, int)"/> if the element is focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        public void ClearInput(int charCount = 0)
        {
            Click(); // focuses
            if (charCount == 0)
            {
                Client.ClearInput(Device, Attributes["text"].Length);
            }
            else
            {
                Client.ClearInput(Device, charCount);
            }
        }

#if HAS_TASK
        /// <summary>
        /// Clear the input text. Use <see cref="IAdbClient.ClearInputAsync(DeviceData, int, CancellationToken)"/> if the element is focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task ClearInputAsync(int charCount = 0, CancellationToken cancellationToken = default)
        {
            await ClickAsync(cancellationToken); // focuses
            if (charCount == 0)
            {
                await Client.ClearInputAsync(Device, Attributes["text"].Length, cancellationToken);
            }
            else
            {
                await Client.ClearInputAsync(Device, charCount, cancellationToken);
            }
        }
#endif
    }
}
