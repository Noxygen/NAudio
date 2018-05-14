using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable once CheckNamespace
namespace NAudio.Wave
{
    /// <summary>
    /// Creates a <see cref="IWaveProvider"/> that offers precise looping abilities.
    /// </summary>
    public class LoopWaveProvider : IWaveProvider
    {
        private readonly object sourceLock = new object();

        #region Properties

        /// <inheritdoc />
        public WaveFormat WaveFormat => Source.WaveFormat;

        /// <summary>
        /// Absolute playback position within the source.
        /// </summary>
        public int Position
        {
            get { lock (sourceLock) return (int) Source.Position; }
            set { lock (sourceLock) Source.Position = value; }
        }

        /// <summary>
        /// Position of the first byte in the loop.
        /// </summary>
        public int LoopStart { get; set; }

        /// <summary>
        /// Position of the last byte in the loop.
        /// </summary>
        public int LoopEnd { get; set; }

        /// <summary>
        /// Is looping enabled.
        /// Setting this to false will cause the provider to continue playing until it runs out of data.
        /// </summary>
        public bool EnableLooping { get; set; } = true;

        /// <summary>
        /// Playback position relative to <see cref="LoopStart"/> within the source.
        /// </summary>
        public int LoopPosition => (int) (Source.Position - LoopStart);

        /// <summary>
        /// Total count of bytes in the loop.
        /// </summary>
        public int LoopLength => LoopEnd - LoopStart + 1;

        /// <summary>
        /// If this setting is true, the provider will start at the beginning and run into the loop.
        /// Setting it to false causes the playback to start at <see cref="LoopStart"/>.
        /// </summary>
        public bool CatchUpMode { get; set; }

        /// <summary>
        /// The source wave provider.
        /// </summary>
        public MemoryWaveProvider Source { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopWaveProvider"/> class based on the given source.
        /// </summary>
        /// <param name="source">The wave source for this instance.</param>
        /// <param name="catchUp">
        /// Specifies whether this provider will start at the beginning of the source and run into the loop.
        /// Setting this to false causes the playback to start at <see cref="LoopStart"/>.
        /// </param>
        public LoopWaveProvider(MemoryWaveProvider source, bool catchUp = false)
        {
            Source = source;
            CatchUpMode = catchUp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopWaveProvider"/> class based on the given source and loop dimensions.
        /// </summary>
        /// <param name="source">The wave source for this instance.</param>
        /// <param name="loopStart">Position of the first byte in the loop.</param>
        /// <param name="loopEnd">Position of the last byte in the loop.</param>
        /// <param name="catchUp">
        /// Specifies whether this provider will start at the beginning of the source and run into the loop.
        /// Setting this to false causes the playback to start at <see cref="LoopStart"/>.
        /// </param>
        public LoopWaveProvider(MemoryWaveProvider source, int loopStart, int loopEnd, bool catchUp = false)
        {
            Source = source;

            CatchUpMode = catchUp;
            LoopStart = loopStart;
            LoopEnd = loopEnd;
        }

        /// <summary>
        /// Sets <see cref="LoopStart"/> and <see cref="LoopEnd"/> according to the given frame-positions by aligning them to the number of channels and bits defined in <see cref="WaveFormat"/>.
        /// </summary>
        /// <param name="loopStart">First frame of the loop.</param>
        /// <param name="loopEnd">Last frame of the loop.</param>
        public void SetLoopFromFrames(int loopStart, int loopEnd)
        {
            var bytesPerSample = WaveFormat.BitsPerSample / 8;
            LoopStart = loopStart * WaveFormat.Channels * bytesPerSample;
            LoopEnd = (WaveFormat.Channels * loopEnd * bytesPerSample) + ((WaveFormat.Channels * bytesPerSample) - 1);
        }

        /// <inheritdoc />
        public int Read(byte[] buffer, int offset, int count)
        {
            lock (sourceLock)
            {
                if (!EnableLooping) return Source.Read(buffer, offset, count);

                // Validate loop parameters
                if (LoopEnd <= 0 ||
                    LoopEnd >= Source.Length ||
                    LoopStart < 0 ||
                    LoopStart >= Source.Length ||
                    LoopEnd <= LoopStart) throw new ArgumentException("Invalid loop parameters.");

                // Make sure playback is within loop when not in catchup mode
                if (!CatchUpMode && (Position > LoopEnd || Position < LoopStart))
                    Position = LoopStart;

                var totalDataCaptured = 0;

                while (totalDataCaptured < count)
                {
                    var dataTilLoopEnd = LoopEnd - Position + 1;
                    if (dataTilLoopEnd < 0) // Overshot?
                        dataTilLoopEnd = 0;

                    var dataToRead = count - totalDataCaptured;
                    var readNow = Math.Min(dataToRead, dataTilLoopEnd);

                    var readcount = Source.Read(buffer, totalDataCaptured + offset, readNow);
                    if (readcount == 0) break;

                    totalDataCaptured += readcount;

                    if (Position > LoopEnd)
                        Position = LoopStart;
                }

                return totalDataCaptured;
            }
        }
    }
}
