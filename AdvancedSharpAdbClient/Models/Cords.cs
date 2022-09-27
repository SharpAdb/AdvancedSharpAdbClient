namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Contains element coordinates
    /// </summary>
    public struct Cords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cords"/> class.
        /// </summary>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        public Cords(int cx, int cy)
        {
            x = cx;
            y = cy;
        }

        /// <summary>
        /// Gets or sets the horizontal "X" coordinate
        /// </summary>
        public int x { get; set; }

        /// <summary>
        /// Gets or sets the vertical "Y" coordinate
        /// </summary>
        public int y { get; set; }
    }
}
