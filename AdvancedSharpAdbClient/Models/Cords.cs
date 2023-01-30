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
            X = cx;
            Y = cy;
        }

        /// <summary>
        /// Gets or sets the horizontal "X" coordinate
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the vertical "Y" coordinate
        /// </summary>
        public int Y { get; set; }
    }
}
