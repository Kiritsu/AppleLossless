namespace AppleLossless
{
    internal class AppleLosslessOptions
    {
        /// <summary>
        /// Gets or sets the path to the ffmpeg executable.
        /// </summary>
        public string FFmpegPath { get; set; } = null!;

        /// <summary>
        /// Gets or sets the path to the folder and subfolders with the different media to convert.
        /// </summary>
        public string SourcePath { get; set; } = null!;

        /// <summary>
        /// Gets or sets the path to the folder that mirrors the source path with the converted medias.
        /// </summary>
        public string DestinationPath { get; set; } = null!;

        /// <summary>
        /// Gets or sets the maximum amount of instance to be ran when converting medias.
        /// </summary>
        public int ThreadCount { get; set; } = 32;

        /// <summary>
        /// Format to convert the media to.
        /// </summary>
        public string Format { get; set; } = "m4a";
    }
}
