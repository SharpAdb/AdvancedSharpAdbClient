﻿using System.Collections.Generic;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implement of screen element, likes Selenium
    /// </summary>
    public class Element
    {
        /// <summary>
        /// The current ADB client that manages the connection.
        /// </summary>
        private IAdbClient client { get; set; }

        /// <summary>
        /// The current device containing the element
        /// </summary>
        private DeviceData device { get; set; }

        /// <summary>
        /// Contains element coordinates 
        /// </summary>
        public Cords cords { get; set; }

        /// <summary>
        /// Gets or sets element attributes
        /// </summary>
        public Dictionary<string, string> attributes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="device"></param>
        /// <param name="cords"></param>
        /// <param name="attributes"></param>
        public Element(IAdbClient client, DeviceData device, Cords cords, Dictionary<string, string> attributes)
        {
            this.client = client;
            this.device = device;
            this.cords = cords;
            this.attributes = attributes;
        }

        /// <inheritdoc/>
        public void Click()
        {
            client.Click(device, cords);
        }

        /// <inheritdoc/>
        public void SendText(string text)
        {
            Click();
            client.SendText(device, text);
        }

        /// <inheritdoc/>
        public void ClearInput(int charcount = 0)
        {
            Click(); // focuse
            if (charcount == 0)
            {
                client.ClearInput(device, attributes["text"].Length);
            }
            else
            {
                client.ClearInput(device, charcount);
            }
        }
    }
}
