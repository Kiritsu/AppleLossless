using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppleLossless
{
    internal class ConverterService : IHostedService
    {
        private readonly AppleLosslessOptions _options;
        private readonly ILogger<ConverterService> _logger;
        private readonly IHostEnvironment _host;

        private FFmpegHelper _ffmpeg = null!;

        public ConverterService(IOptions<AppleLosslessOptions> options, ILogger<ConverterService> logger, IHostEnvironment host)
        {
            _options = options.Value;
            _logger = logger;
            _host = host;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!FFmpegHelper.Exists(_options.FFmpegPath, out var path))
            {
                _logger.LogCritical("Couldn't find FFmpeg. Please set environment {Environment} to the path to ffmpeg or its folder", 
                    "A_LOSSLESS__FFMPEGPATH");

                return;
            }

            _options.FFmpegPath = path!;

            if (_options.SourcePath == null)
            {
                _logger.LogCritical("Couldn't find any source path. Please set environment {Environment} to the path to your media", 
                    "A_LOSSLESS__SOURCEPATH");

                return;
            }

            if (_options.DestinationPath == null)
            {
                _options.DestinationPath = Path.Combine(_host.ContentRootPath, Guid.NewGuid().ToString());
                _logger.LogWarning("Setting the destination path to {Path} because environment {Environment} was not set", 
                    _options.DestinationPath, "A_LOSSLESS__DESTINATIONPATH");
            }

            _logger.LogInformation("Checking and caching media information. Please do not modify the source folder from now");
            var files = GetFiles(_options.SourcePath);
            _logger.LogInformation("Media found: {Count}", files.Count());

            var keptFiles = new List<string>();
            var groupedFiles = files.GroupBy(x => Path.GetExtension(x));
            foreach (var group in groupedFiles)
            {
                if (!FFmpegHelper.IsSupportedMime(group.Key[1..]))
                {
                    _logger.LogWarning("- {Ext}, {Count} medias ({Percentage}%) [Unsupported]",
                        group.Key, group.Count(), Math.Round(group.Count() / (double)files.Count() * 100, 2));
                }
                else
                {
                    _logger.LogInformation("- {Ext}, {Count} medias ({Percentage}%)",
                        group.Key, group.Count(), Math.Round(group.Count() / (double)files.Count() * 100, 2));

                    keptFiles.AddRange(group);
                }
            }

            _logger.LogInformation("Kept {Count} valid and supported media files", keptFiles.Count);
            _logger.LogInformation("Starting convertion process to {Format} format on {Count} threads", _options.Format, _options.ThreadCount);

            _ffmpeg = new(new(_options.FFmpegPath!), keptFiles, _options, _logger);
            await _ffmpeg.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ffmpeg?.Dispose();
            return Task.CompletedTask;
        }

        private static IEnumerable<string> GetFiles(string directory)
        {
            var files = Directory.GetFiles(directory);
            foreach (string fileName in files)
                yield return fileName;

            var directories = Directory.GetDirectories(directory);
            foreach (string subDirectory in directories)
                foreach (var file in GetFiles(subDirectory))
                    yield return file;
        }
    }
}
