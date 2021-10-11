using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AppleLossless
{
    internal class FFmpegHelper
    {
        private static readonly string[] _mimes = new string[] {
            "flac", "m3u", "m3u8", "m4a", "m4b", "mp3", "ogg",
            "opus", "pls", "wav", "aac", "webm", "wma", "xspf"
        };

        private readonly FileInfo _ffmpeg;
        private readonly IReadOnlyList<string> _files;
        private readonly AppleLosslessOptions _options;
        private readonly ILogger<ConverterService> _logger;
        private readonly int _threadCount;

        public FFmpegHelper(FileInfo ffmpeg, IReadOnlyList<string> files, AppleLosslessOptions options, ILogger<ConverterService> logger)
        {
            _ffmpeg = ffmpeg;
            _files = files;
            _options = options;
            _logger = logger;
            _threadCount = _options.ThreadCount <= 0 ? _options.ThreadCount = 1 : _options.ThreadCount;
        }

        public async Task StartAsync(CancellationToken token = default)
        {
            var chunkSize = (_files.Count - 1) / _threadCount + 1;

            var chunks = _files.Chunk(chunkSize);
            _logger.LogInformation("Splitting job into {Count} chunks of {Size} size", chunks.Count(), chunkSize);

            var sw = Stopwatch.StartNew();
            var tasks = chunks.Select((chunk, index) => ProcessChunkAsync(chunk, index, token));
            await Task.WhenAll(tasks);

            sw.Stop();
            _logger.LogInformation("Every chunk has been successfully completed in {Ms} ms", sw.ElapsedMilliseconds);
        }

        private async Task ProcessChunkAsync(string[] chunk, int chunkIndex, CancellationToken token = default)
        {
            _logger.LogInformation("Processing chunk #{Count}. {FileCount} files to convert", chunkIndex, chunk.Length);

            foreach (var file in chunk)
            {
                var fileInfo = new FileInfo(file);
                if (!fileInfo.Exists)
                {
                    _logger.LogError("The file at path {Path} doesn't exist anymore. Ignoring it", fileInfo.FullName);
                    continue;
                }

                _logger.LogInformation("Converting {File} to format {Format}", fileInfo.Name, _options.Format);

                await ConvertFileAsync(fileInfo, token);
            }
        }

        private async Task ConvertFileAsync(FileInfo fileInfo, CancellationToken token = default)
        {
            var destinationName = Path.Combine(_options.DestinationPath, fileInfo.FullName[(_options.SourcePath.Length + 1)..]);
            var currentExtension = Path.GetExtension(destinationName);
            var destinationExtension = _options.Format;
            if (!destinationExtension.StartsWith('.'))
            {
                destinationExtension = destinationExtension.Insert(0, ".");
            }

            destinationName = destinationName.Replace(currentExtension, destinationExtension);

            if (File.Exists(destinationName))
{
                _logger.LogWarning("The file at path {Path} already exist. Ignoring it", fileInfo.FullName);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationName)!);

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = _ffmpeg.FullName,
                Arguments = $"-hide_banner -loglevel error -i \"{fileInfo.FullName}\" -map a:0 -acodec alac \"{destinationName}\""
            });

            await process!.WaitForExitAsync(token);

            _logger.LogInformation("Converting {File} succeeded!", Path.GetFileName(destinationName));
        }

        public static bool Exists(string? path, out string? truePath)
        {
            truePath = null;

            if (path == null)
            {
                return false;
            }

            if (Directory.Exists(path))
            {
                var ffmpegWin = Path.Combine(path, "ffmpeg.exe");
                var ffmpegUnix = Path.Combine(path, "ffmpeg");

                if (File.Exists(ffmpegWin))
                {
                    truePath = ffmpegWin;
                    return true;
                }
                else if (File.Exists(ffmpegUnix))
                {
                    truePath = ffmpegUnix;
                    return true;
                }
            }

            if (File.Exists(path))
            {
                truePath = path;
                return true;
            }

            return false;
        }

        public static bool IsSupportedMime(string extension)
        {
            return _mimes.Contains(extension);
        }
    }
}
