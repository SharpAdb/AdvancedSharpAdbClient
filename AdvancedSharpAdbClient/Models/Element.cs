// <copyright file="Element.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implement of screen element, likes Selenium.
    /// </summary>
    public class Element : IEquatable<Element>
    {
        private static readonly char[] separator = ['[', ']', ',', ' '];

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="area">The coordinates and size of the element.</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, Area area, Dictionary<string, string> attributes = null)
        {
            Client = client;
            Device = device;
            Bounds = area;
            Attributes = attributes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="XmlNode"/> of the element.</param>
        public Element(IAdbClient client, DeviceData device, XmlNode xmlNode)
        {
            Client = client;
            Device = device;
            Node = xmlNode;

            if (xmlNode.Attributes["bounds"]?.Value is string bounds)
            {
                string[] cords = bounds.Split(separator, StringSplitOptions.RemoveEmptyEntries); // x1, y1, x2, y2
                Bounds = Area.FromLTRB(int.Parse(cords[0]), int.Parse(cords[1]), int.Parse(cords[2]), int.Parse(cords[3]));
            }
            
            Attributes = new(xmlNode.Attributes.Count);
            foreach (XmlAttribute at in xmlNode.Attributes)
            {
                Attributes[at.Name] = at.Value;
            }

            IEnumerable<Element> FindElements()
            {
                XmlNodeList childNodes = xmlNode.ChildNodes;
                if (childNodes != null)
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        Element element = FromXmlNode(client, device, childNodes[i]);
                        if (element != null)
                        {
                            yield return element;
                        }
                    }
                }
            }
            Children = FindElements();
        }

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="Windows.Data.Xml.Dom.IXmlNode"/> of the element.</param>
        public Element(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode xmlNode)
        {
            XmlDocument doc = new();
            doc.LoadXml(xmlNode.GetXml());

            Client = client;
            Device = device;
            Node = doc.FirstChild;

            if (xmlNode.Attributes?.GetNamedItem("bounds")?.NodeValue?.ToString() is string bounds)
            {
                string[] cords = bounds.Split(separator, StringSplitOptions.RemoveEmptyEntries); // x1, y1, x2, y2
                Bounds = Area.FromLTRB(int.Parse(cords[0]), int.Parse(cords[1]), int.Parse(cords[2]), int.Parse(cords[3]));
            }
            
            Attributes = new(xmlNode.Attributes.Count);
            foreach (Windows.Data.Xml.Dom.IXmlNode at in xmlNode.Attributes)
            {
                Attributes[at.NodeName] = at.NodeValue?.ToString();
            }

            IEnumerable<Element> FindElements()
            {
                Windows.Data.Xml.Dom.XmlNodeList childNodes = xmlNode.ChildNodes;
                if (childNodes != null)
                {
                    foreach (Windows.Data.Xml.Dom.IXmlNode childNode in childNodes)
                    {
                        Element element = FromIXmlNode(client, device, childNode);
                        if (element != null)
                        {
                            yield return element;
                        }
                    }
                }
            }
            Children = FindElements();
        }
#endif

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
        public Area Bounds { get; }

        /// <summary>
        /// Gets the children of this element.
        /// </summary>
        public IEnumerable<Element> Children { get; }

        /// <summary>
        /// Gets the element attributes.
        /// </summary>
        public Dictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the <see cref="XmlNode"/> of this element.
        /// </summary>
        public XmlNode Node { get; }

        /// <summary>
        /// Gets the coordinates of the the center of the element.
        /// </summary>
        public Cords Center => Bounds.Center;

        /// <summary>
        /// Gets the text of the element.
        /// </summary>
        public string Text => Attributes.TryGetValue("text", out string text) ? text : string.Empty;

        /// <summary>
        /// Gets the class name of the element.
        /// </summary>
        public string Class => Attributes.TryGetValue("class", out string @class) ? @class : string.Empty;

        /// <summary>
        /// Gets the package name of the element.
        /// </summary>
        public string Package => Attributes.TryGetValue("package", out string package) ? package : string.Empty;

        /// <summary>
        /// Gets the resource ID of the element.
        /// </summary>
        public string ResourceID => Attributes.TryGetValue("resource-id", out string resource_id) ? resource_id : string.Empty;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <remarks>The index method is index by <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)"/>.</remarks>
        public Element this[int index] => Children.ElementAt(index);

        /// <summary>
        /// Creates a new <see cref='Element'/> with the specified <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="XmlNode"/> of the element.</param>
        /// <returns>The new <see cref="Element"/> that this method creates.</returns>
        public static Element FromXmlNode(IAdbClient client, DeviceData device, XmlNode xmlNode) =>
            xmlNode.Attributes["bounds"] != null ? new Element(client, device, xmlNode) : null;

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Creates a new <see cref='Element'/> with the specified <see cref="Windows.Data.Xml.Dom.IXmlNode"/>.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="Windows.Data.Xml.Dom.IXmlNode"/> of the element.</param>
        /// <returns>The new <see cref="Element"/> that this method creates.</returns>
        public static Element FromIXmlNode(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode xmlNode) =>
            xmlNode.Attributes?.GetNamedItem("bounds") != null ? new Element(client, device, xmlNode) : null;
#endif

        /// <summary>
        /// Gets the count of <see cref="Children"/> in this element.
        /// </summary>
        public virtual int GetChildCount() => Children.Count() + Children.Select(x => x.GetChildCount()).Sum();

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public void Click() => Client.Click(Device, Center);

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        public void SendText(string text)
        {
            Click();
            Client.SendText(Device, text);
        }

        /// <summary>
        /// Clear the input text. Use <see cref="AdbClientExtensions.ClearInput(IAdbClient, DeviceData, int)"/> if the element is focused.
        /// </summary>
        public void ClearInput() => ClearInput(Text.Length);

        /// <summary>
        /// Clear the input text. Use <see cref="AdbClientExtensions.ClearInput(IAdbClient, DeviceData, int)"/> if the element is focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        public void ClearInput(int charCount)
        {
            Click(); // focuses
            Client.ClearInput(Device, charCount);
        }

#if HAS_TASK
        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        public Task ClickAsync(CancellationToken cancellationToken = default) =>
            Client.ClickAsync(Device, Center, cancellationToken);

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

        /// <summary>
        /// Clear the input text. Use <see cref="AdbClientExtensions.ClearInputAsync(IAdbClient, DeviceData, int, CancellationToken)"/> if the element is focused.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task ClearInputAsync(CancellationToken cancellationToken = default) =>
            ClearInputAsync(Text.Length, cancellationToken);

        /// <summary>
        /// Clear the input text. Use <see cref="AdbClientExtensions.ClearInputAsync(IAdbClient, DeviceData, int, CancellationToken)"/> if the element is focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task ClearInputAsync(int charCount, CancellationToken cancellationToken = default)
        {
            await ClickAsync(cancellationToken).ConfigureAwait(false); // focuses
            await Client.ClearInputAsync(Device, charCount, cancellationToken).ConfigureAwait(false);
        }
#endif

        /// <summary>
        /// Find the first descendant element matching a given predicate, using a depth-first search.
        /// </summary>
        /// <param name="predicate">The predicate to use to match the descendant nodes.</param>
        /// <returns>The descendant that was found, or <see langword="null"/>.</returns>
        public Element FindDescendant(Func<Element, bool> predicate)
        {
            foreach (Element child in Children)
            {
                if (predicate(child))
                {
                    return child;
                }

                Element descendant = child.FindDescendant(predicate);

                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first descendant (or self) element matching a given predicate, using a depth-first search.
        /// </summary>
        /// <param name="predicate">The predicatee to use to match the descendant nodes.</param>
        /// <returns>The descendant (or self) that was found, or <see langword="null"/>.</returns>
        public Element FindDescendantOrSelf(Func<Element, bool> predicate) =>
            predicate(this) ? this : FindDescendant(predicate);

        /// <summary>
        /// Find all descendant elements of the specified element. This method can be chained with
        /// LINQ calls to add additional filters or projections on top of the returned results.
        /// <para>
        /// This method is meant to provide extra flexibility in specific scenarios and it should not
        /// be used when only the first item is being looked for. In those cases, use one of the
        /// available <see cref="FindDescendant(Func{Element, bool})"/> overloads instead, which will
        /// offer a more compact syntax as well as better performance in those cases.
        /// </para>
        /// </summary>
        /// <returns>All the descendant <see cref="Element"/> instance from <see cref="Children"/>.</returns>
        public IEnumerable<Element> FindDescendants()
        {
            foreach (Element child in Children)
            {
                yield return child;

                foreach (Element childOfChild in child.FindDescendants())
                {
                    yield return childOfChild;
                }
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as Element);

        /// <inheritdoc/>
        public bool Equals(Element other) =>
            other is not null
                && Client == other.Client
                && Device == other.Device
                && Node == null
                    ? Bounds == other.Bounds
                        && Attributes == other.Attributes
                    : Node == other.Node;

        /// <inheritdoc/>
        public override int GetHashCode() =>
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            HashCode.Combine(Client, Device, Node == null ? HashCode.Combine(Bounds, Attributes) : Node.GetHashCode());
#else
            Client.GetHashCode()
            ^ Device.GetHashCode()
            ^ (Node == null
                ? Bounds.GetHashCode()
                    ^ Attributes.GetHashCode()
                : Node.GetHashCode());
#endif

        /// <inheritdoc/>
        public override string ToString() =>
            string.IsNullOrEmpty(Text) ? string.IsNullOrEmpty(Class) ? base.ToString() : Class : Text;
    }
}
