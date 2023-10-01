// <copyright file="Element.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implement of screen element, likes Selenium.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Gets or sets the current ADB client that manages the connection.
        /// </summary>
        protected IAdbClient Client { get; set; }

        /// <summary>
        /// Gets the current device containing the element.
        /// </summary>
        protected DeviceData Device { get; }

        /// <summary>
        /// Gets the coordinates and size of the element.
        /// </summary>
        public Area Area { get; }

        /// <summary>
        /// Gets or sets the coordinates of the element to click. Default is the center of area.
        /// </summary>
        public Cords Cords { get; set; }

        /// <summary>
        /// Gets the children of this element.
        /// </summary>
        public List<Element> Children { get; }

        /// <summary>
        /// Gets the element attributes.
        /// </summary>
        public Dictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the <see cref="XmlNode"/> of this element.
        /// </summary>
        public XmlNode Node { get; }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public Element this[int index] => Children[index];

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
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="node">The <see cref="XmlNode"/> of the element.</param>
        /// <param name="children">The children of the element.</param>
        /// <param name="area">The coordinates and size of the element.</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, XmlNode node, List<Element> children, Area area, Dictionary<string, string> attributes)
        {
            Client = client;
            Device = device;
            Node = node;
            Children = children;
            Area = area;
            Attributes = attributes;
            Cords = area.Center; // Average x1, y1, x2, y2
        }

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="node">The <see cref="Windows.Data.Xml.Dom.IXmlNode"/> of the element.</param>
        /// <param name="children">The children of the element.</param>
        /// <param name="area">The coordinates and size of the element.</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode node, List<Element> children, Area area, Dictionary<string, string> attributes)
        {
            XmlDocument doc = new();
            doc.LoadXml(node.GetXml());

            Client = client;
            Device = device;
            Node = doc.FirstChild;
            Children = children;
            Area = area;
            Attributes = attributes;
            Cords = area.Center; // Average x1, y1, x2, y2
        }
#endif

        /// <summary>
        /// Creates a new <see cref='Element'/> with the specified <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="XmlNode"/> of the element.</param>
        /// <returns>The new <see cref="Element"/> that this method creates.</returns>
        public static Element FromXmlNode(IAdbClient client, DeviceData device, XmlNode xmlNode)
        {
            string bounds = xmlNode.Attributes["bounds"].Value;
            if (bounds != null)
            {
                int[] cords = bounds.Replace("][", ",").Replace("[", "").Replace("]", "").Split(',').Select(int.Parse).ToArray(); // x1, y1, x2, y2
                Dictionary<string, string> attributes = new(xmlNode.Attributes.Count);
                foreach (XmlAttribute at in xmlNode.Attributes)
                {
                    attributes[at.Name] = at.Value;
                }
                Area area = Area.FromLTRB(cords[0], cords[1], cords[2], cords[3]);
                XmlNodeList childNodes = xmlNode.ChildNodes;
                List<Element> elements = new(childNodes?.Count ?? 0);
                if (childNodes != null)
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        Element element = FromXmlNode(client, device, childNodes[i]);
                        if (element != null)
                        {
                            elements.Add(element);
                        }
                    }
                }
                return new Element(client, device, xmlNode, elements, area, attributes);
            }
            return null;
        }

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Creates a new <see cref='Element'/> with the specified <see cref="Windows.Data.Xml.Dom.IXmlNode"/>.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="Windows.Data.Xml.Dom.IXmlNode"/> of the element.</param>
        /// <returns>The new <see cref="Element"/> that this method creates.</returns>
        public static Element FromIXmlNode(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode xmlNode)
        {
            string bounds = xmlNode.Attributes?.GetNamedItem("bounds")?.NodeValue?.ToString();
            if (bounds != null)
            {
                int[] cords = bounds.Replace("][", ",").Replace("[", "").Replace("]", "").Split(',').Select(int.Parse).ToArray(); // x1, y1, x2, y2
                Dictionary<string, string> attributes = new(xmlNode.Attributes.Count);
                foreach (Windows.Data.Xml.Dom.IXmlNode at in xmlNode.Attributes)
                {
                    attributes[at.NodeName] = at.NodeValue.ToString();
                }
                Area area = Area.FromLTRB(cords[0], cords[1], cords[2], cords[3]);
                Windows.Data.Xml.Dom.XmlNodeList childNodes = xmlNode.ChildNodes;
                List<Element> elements = new(childNodes?.Count ?? 0);
                if (childNodes != null)
                {
                    foreach (Windows.Data.Xml.Dom.IXmlNode childNode in childNodes)
                    {
                        Element element = FromIXmlNode(client, device, childNode);
                        if (element != null)
                        {
                            elements.Add(element);
                        }
                    }
                }
                return new Element(client, device, xmlNode, elements, area, attributes);
            }
            return null;
        }
#endif

        /// <summary>
        /// Gets the count of <see cref="Children"/> in this element.
        /// </summary>
        public virtual int GetChildCount() => Children.Count + Children.Select(x => x.GetChildCount()).Sum();

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public void Click() => Client.Click(Device, Cords);

#if HAS_TASK
        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        public Task ClickAsync(CancellationToken cancellationToken = default) =>
            Client.ClickAsync(Device, Cords, cancellationToken);
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
            await ClickAsync(cancellationToken).ConfigureAwait(false);
            await Client.SendTextAsync(Device, text, cancellationToken).ConfigureAwait(false);
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
            await ClickAsync(cancellationToken).ConfigureAwait(false); // focuses
            if (charCount == 0)
            {
                await Client.ClearInputAsync(Device, Attributes["text"].Length, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await Client.ClearInputAsync(Device, charCount, cancellationToken).ConfigureAwait(false);
            }
        }
#endif
    }
}
