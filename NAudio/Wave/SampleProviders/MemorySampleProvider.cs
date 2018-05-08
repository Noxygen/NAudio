using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable once CheckNamespace
namespace NAudio.Wave
{
    /// <summary>
    /// A read-only, random-access <see cref="ISampleProvider"/> whose backing store is memory.
    /// </summary>
    public class MemorySampleProvider : ISampleProvider
    {
        private readonly object sourceLock = new object();
        private readonly float[] source;

        #region Properties

        private int position;

        /// <inheritdoc />
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// Gets the length of the data source in samples.
        /// </summary>
        public int Length => source.Length;

        /// <summary>
        /// Gets or sets the current position within the data source.
        /// </summary>
        public int Position
        {
            get { lock (sourceLock) return position; }
            set { lock (sourceLock) position = value; }
        }

        #endregion

        /// <summary>Initializes a new instance of the <see cref="MemorySampleProvider" /> class based on the specified float array.</summary>
        /// <param name="data">The array of floats from which to create the current provider.</param>
        /// <param name="format">The WaveFormat of this sample provider.
        /// It needs to specify a format of <see cref="WaveFormatEncoding.IeeeFloat"/> type.</param>
        public MemorySampleProvider(float[] data, WaveFormat format)
        {
            WaveFormat = format;
            source = data;
        }

        /// <summary>Initializes a new instance of the <see cref="MemorySampleProvider" /> class based on the specified float array.</summary>
        /// <param name="data">The array of floats from which to create the current provider. </param>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="numChannels">Number of channels</param>
        public MemorySampleProvider(float[] data, int sampleRate, int numChannels)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, numChannels);
            source = data;
        }

        /// <inheritdoc />
        public int Read(float[] buffer, int offset, int count)
        {
            lock (sourceLock)
            {
                var len = Math.Min(count, Length - Position);

                Buffer.BlockCopy(source, Position * 4, buffer, offset * 4, len * 4);

                Position += len;
                return len;
            }
        }
    }
}
