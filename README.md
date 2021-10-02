# AppleLossless

Short project that aims to easily convert my FLAC library into apple-compatible lossless audio format (m4a; alac)

# Configuration

You need the following environment variable to use the app:

- A_LOSSLESS__FFMPEGPATH: a valid path to ffmpeg or a folder that contains ffmpeg.\
- A_LOSSLESS__SOURCEPATH: a valid path to your media files.\
- A_LOSSLESS__DESTINATIONPATH: a valid path to a folder where all your media files will be converted.\
- A_LOSSLESS__THREADCOUNT: the maximum amount of ffmpeg instances to spawn at the same time. It should be the amount of logical core you have. Defaults to 32.\
- A_LOSSLESS__FORMAT: the file format that you want to convert your media files to.