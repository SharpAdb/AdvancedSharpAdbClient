// <copyright file="Element.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient.DeviceCommands.Models
{
    /// <summary>
    /// Implement of screen element, likes Selenium.
    /// </summary>
    public class Element : IEquatable<Element>
    {
        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that separate the coordinates of the element.
        /// </summary>
        private static readonly char[] separator = ['[', ']', ',', ' '];

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="rectangle">The coordinates and size of the element.</param>
        /// <param name="attributes">Gets or sets element attributes.</param>
        public Element(IAdbClient client, DeviceData device, Rectangle rectangle, Dictionary<string, string?>? attributes = null)
        {
            Client = client;
            Device = device;
            Bounds = rectangle;
            Attributes = attributes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="XmlNode"/> of the element.</param>
        public Element(IAdbClient client, DeviceData device, XmlNode? xmlNode)
        {
            Client = client;
            Device = device;
            Node = xmlNode;

            if (xmlNode?.Attributes != null)
            {
                bool foundBounds = false;
                Attributes = new(xmlNode.Attributes.Count);
                foreach (XmlAttribute at in xmlNode.Attributes.OfType<XmlAttribute>())
                {
                    if (!foundBounds && at.Name == "bounds" && at.Value is string bounds)
                    {
                        string[] cords = bounds.Split(separator, StringSplitOptions.RemoveEmptyEntries); // x1, y1, x2, y2
                        Bounds = Rectangle.FromLTRB(int.Parse(cords[0]), int.Parse(cords[1]), int.Parse(cords[2]), int.Parse(cords[3]));
                        foundBounds = true;
                    }
                    Attributes[at.Name] = at.Value;
                }
            }

            static IEnumerable<Element> FindElements(IAdbClient client, DeviceData device, XmlNode? xmlNode)
            {
                XmlNodeList? childNodes = xmlNode?.ChildNodes;
                if (childNodes != null)
                {
                    for (int i = 0; i < childNodes!.Count; i++)
                    {
                        Element? element = FromXmlNode(client, device, childNodes?[i]);
                        if (element != null)
                        {
                            yield return element;
                        }
                    }
                }
            }
            Children = FindElements(client, device, xmlNode);
        }

#if HAS_WINRT
        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="Windows.Data.Xml.Dom.IXmlNode"/> of the element.</param>
#if NET
        [SupportedOSPlatform("Windows10.0.10240.0")]
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
#endif
        public Element(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode xmlNode)
        {
            Client = client;
            Device = device;

            ExceptionExtensions.ThrowIfNull(xmlNode);
            XmlDocument doc = new();
            doc.LoadXml(xmlNode.GetXml());
            Node = doc.FirstChild;

            if (xmlNode.Attributes != null)
            {
                bool foundBounds = false;
                Attributes = new(xmlNode.Attributes.Count);
                foreach (Windows.Data.Xml.Dom.IXmlNode at in xmlNode.Attributes)
                {
                    if (!foundBounds && at.NodeName == "bounds" && at.NodeValue is string bounds)
                    {
                        string[] cords = bounds.Split(separator, StringSplitOptions.RemoveEmptyEntries); // x1, y1, x2, y2
                        Bounds = Rectangle.FromLTRB(int.Parse(cords[0]), int.Parse(cords[1]), int.Parse(cords[2]), int.Parse(cords[3]));
                        foundBounds = true;
                    }
                    Attributes[at.NodeName] = at.NodeValue.ToString();
                }
            }

            static IEnumerable<Element> FindElements(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode xmlNode)
            {
                Windows.Data.Xml.Dom.XmlNodeList childNodes = xmlNode.ChildNodes;
                if (childNodes != null)
                {
                    foreach (Windows.Data.Xml.Dom.IXmlNode childNode in childNodes)
                    {
                        Element? element = FromIXmlNode(client, device, childNode);
                        if (element != null)
                        {
                            yield return element;
                        }
                    }
                }
            }
            Children = FindElements(client, device, xmlNode);
        }
#endif

        /// <summary>
        /// Gets or sets the current ADB client that manages the connection.
        /// </summary>
        public IAdbClient Client { get; init; }

        /// <summary>
        /// Gets the current device containing the element.
        /// </summary>
        public DeviceData Device { get; init; }

        /// <summary>
        /// Gets the coordinates and size of the element.
        /// </summary>
        public Rectangle Bounds { get; init; }

        /// <summary>
        /// Gets the children of this element.
        /// </summary>
        public IEnumerable<Element>? Children { get; init; }

        /// <summary>
        /// Gets the element attributes.
        /// </summary>
        public Dictionary<string, string?>? Attributes { get; init; }

        /// <summary>
        /// Gets the <see cref="XmlNode"/> of this element.
        /// </summary>
        public XmlNode? Node { get; init; }

        /// <summary>
        /// Gets the coordinates of the the center of the element.
        /// </summary>
        public Point Center => unchecked(new(Bounds.X + (Bounds.Width / 2), Bounds.Y + (Bounds.Height / 2)));

        /// <summary>
        /// Gets the text of the element.
        /// </summary>
        public string? Text => Attributes?.TryGetValue("text", out string? text) == true ? text : string.Empty;

        /// <summary>
        /// Gets the class name of the element.
        /// </summary>
        public string? Class => Attributes?.TryGetValue("class", out string? @class) == true ? @class : string.Empty;

        /// <summary>
        /// Gets the package name of the element.
        /// </summary>
        public string? Package => Attributes?.TryGetValue("package", out string? package) == true ? package : string.Empty;

        /// <summary>
        /// Gets the resource ID of the element.
        /// </summary>
        public string? ResourceID => Attributes?.TryGetValue("resource-id", out string? resource_id) == true ? resource_id : string.Empty;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <remarks>The index method is index by <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)"/>.</remarks>
        public Element? this[int index] => Children?.ElementAt(index);

        /// <summary>
        /// Creates a new <see cref='Element'/> with the specified <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="XmlNode"/> of the element.</param>
        /// <returns>The new <see cref="Element"/> that this method creates.</returns>
        public static Element? FromXmlNode(IAdbClient client, DeviceData device, XmlNode? xmlNode) =>
            xmlNode?.Attributes?["bounds"] != null ? new Element(client, device, xmlNode) : null;

#if HAS_WINRT
        /// <summary>
        /// Creates a new <see cref='Element'/> with the specified <see cref="Windows.Data.Xml.Dom.IXmlNode"/>.
        /// </summary>
        /// <param name="client">The current ADB client that manages the connection.</param>
        /// <param name="device">The current device containing the element.</param>
        /// <param name="xmlNode">The <see cref="Windows.Data.Xml.Dom.IXmlNode"/> of the element.</param>
        /// <returns>The new <see cref="Element"/> that this method creates.</returns>
#if NET
        [SupportedOSPlatform("Windows10.0.10240.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public static Element? FromIXmlNode(IAdbClient client, DeviceData device, Windows.Data.Xml.Dom.IXmlNode xmlNode) =>
            xmlNode.Attributes?.GetNamedItem("bounds") != null ? new Element(client, device, xmlNode) : null;
#endif

        /// <summary>
        /// Gets the count of <see cref="Children"/> in this element.
        /// </summary>
        public virtual int GetChildCount() => Children == null ? 0 : Children.Count() + Children.Select(x => x.GetChildCount()).Sum();

        /// <summary>
        /// Clicks on this coordinates.
        /// </summary>
        public void Click()
        {
            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            Client.ExecuteShellCommand(Device, $"input tap {Center.X} {Center.Y}", receiver);

            string result = receiver.ToString();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        public void SendText(string text)
        {
            Click();

            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            Client.ExecuteShellCommand(Device, $"input text {text}", receiver);

            string result = receiver.ToString();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new InvalidTextException();
            }
        }

        /// <summary>
        /// Clear the input text. Use <see cref="DeviceClient.ClearInput(int)"/> if the element is focused.
        /// </summary>
        [MemberNotNull(nameof(Text))]
        public void ClearInput() => ClearInput(Text!.Length);

        /// <summary>
        /// Clear the input text. Use <see cref="DeviceClient.ClearInput(int)"/> if the element is focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        public void ClearInput(int charCount)
        {
            Click(); // focuses
            Client.ClearInput(Device, charCount);
        }

#if HAS_TASK
        /// <summary>
        /// Asynchronously clicks on this coordinates.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        public async Task ClickAsync(CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            await Client.ExecuteShellCommandAsync(Device, $"input tap {Center.X} {Center.Y}", receiver, cancellationToken).ConfigureAwait(false);

            string result = receiver.ToString();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Asynchronously send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SendTextAsync(string text, CancellationToken cancellationToken = default)
        {
            await ClickAsync(cancellationToken).ConfigureAwait(false);

            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            await Client.ExecuteShellCommandAsync(Device, $"input text {text}", receiver, cancellationToken).ConfigureAwait(false);

            string result = receiver.ToString();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new InvalidTextException();
            }
        }

        /// <summary>
        /// Asynchronously clear the input text. Use <see cref="DeviceClient.ClearInputAsync(int, CancellationToken)"/> if the element is focused.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        [MemberNotNull(nameof(Text))]
        public Task ClearInputAsync(CancellationToken cancellationToken = default) =>
            ClearInputAsync(Text!.Length, cancellationToken);

        /// <summary>
        /// Asynchronously clear the input text. Use <see cref="DeviceClient.ClearInputAsync(int, CancellationToken)"/> if the element is focused.
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
        public Element? FindDescendant(Func<Element, bool> predicate)
        {
            if (Children == null) { return null; }

            foreach (Element child in Children)
            {
                if (predicate(child))
                {
                    return child;
                }

                Element? descendant = child.FindDescendant(predicate);

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
        /// <param name="predicate">The predicate to use to match the descendant nodes.</param>
        /// <returns>The descendant (or self) that was found, or <see langword="null"/>.</returns>
        public Element? FindDescendantOrSelf(Func<Element, bool> predicate) =>
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
            if (Children == null) { yield break; }
            foreach (Element child in Children)
            {
                yield return child;

                foreach (Element childOfChild in child.FindDescendants())
                {
                    yield return childOfChild;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="FindDescendants"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="Element"/>.</returns>
        public IEnumerator<Element> GetEnumerator() => FindDescendants().GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Element);

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] Element? other) =>
            (object?)this == other ||
                (other != (object?)null
                && EqualityComparer<IAdbClient>.Default.Equals(Client, other.Client)
                && EqualityComparer<DeviceData>.Default.Equals(Device, other.Device)
                && (Node == null
                    ? Bounds == other.Bounds
                        && other.Attributes == null
                            ? Attributes == null
                            : Attributes?.SequenceEqual(other.Attributes!) == true
                    : other.Node != null
                        && Node.OuterXml == other.Node.OuterXml));

        /// <summary>
        /// Tests whether two <see cref='Element'/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref='Element'/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref='Element'/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="Element"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(Element? left, Element? right) => (object?)left == right || (left?.Equals(right) ?? false);

        /// <summary>
        /// Tests whether two <see cref='Element'/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref='Element'/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref='Element'/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="Element"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Element? left, Element? right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            Node == null
                ? HashCode.Combine(Client, Device, Bounds, Attributes)
                : HashCode.Combine(Client, Device, Node);

        /// <inheritdoc/>
        public override string? ToString() =>
            string.IsNullOrEmpty(Text) ? string.IsNullOrEmpty(Class) ? base.ToString() : Class : Text;
    }
}
