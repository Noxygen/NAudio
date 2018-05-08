using System.IO;

// ReSharper disable UnusedMember.Global
// ReSharper disable once CheckNamespace
namespace NAudio.Wave
{
    /// <summary>
    /// Creates an expandable read-write, random-access <see cref="IWaveProvider"/> based on <see cref="MemoryStream" />.
    /// </summary>
    public class MemoryWaveProvider : MemoryStream, IWaveProvider
    {
        /// <inheritdoc />
        public WaveFormat WaveFormat { get; }

        /// <summary>Initializes a new instance of the <see cref="MemoryWaveProvider" /> class with an expandable capacity initialized to zero.</summary>
        /// <param name="waveFormat">The WaveFormat of this provider.</param>
        public MemoryWaveProvider(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }

        /// <summary>Initializes a new non-resizable instance of the <see cref="MemoryWaveProvider" /> class based on the specified byte array.</summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        /// <param name="waveFormat">The WaveFormat of this provider.</param>
        public MemoryWaveProvider(WaveFormat waveFormat, byte[] buffer) : base(buffer)
        {
            WaveFormat = waveFormat;
        }

        /// <summary>Initializes a new non-resizable instance of the <see cref="MemoryWaveProvider" /> class based on the specified byte array.</summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        /// <param name="writable">The setting of the <see cref="MemoryStream.CanWrite" /> property, which determines whether the stream supports writing.</param>
        /// <param name="waveFormat">The WaveFormat of this provider.</param>
        public MemoryWaveProvider(WaveFormat waveFormat, byte[] buffer, bool writable) : base(buffer, writable)
        {
            WaveFormat = waveFormat;
        }
    }
}
