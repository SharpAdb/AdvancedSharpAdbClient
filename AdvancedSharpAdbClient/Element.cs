using System.Collections.Generic;

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
        /// Contains element coordinates.
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
        /// <param name="cords">Contains element coordinates .</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, Cords cords, Dictionary<string, string> attributes)
        {
            Client = client;
            Device = device;
            Cords = cords;
            Attributes = attributes;
        }

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public void Click() => Client.Click(Device, Cords);

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string text)
        {
            Click();
            Client.SendText(Device, text);
        }

        /// <summary>
        /// Clear the input text. The input should be in focus. Use el.ClearInput() if the element isn't focused.
        /// </summary>
        /// <param name="charcount"></param>
        public void ClearInput(int charcount = 0)
        {
            Click(); // focuse
            if (charcount == 0)
            {
                Client.ClearInput(Device, Attributes["text"].Length);
            }
            else
            {
                Client.ClearInput(Device, charcount);
            }
        }
    }
}
