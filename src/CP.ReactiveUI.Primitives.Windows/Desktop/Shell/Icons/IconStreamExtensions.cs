// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Extension methods for icon streams.</summary>
public static class IconStreamExtensions
{
    /// <summary>The byte offset of an icon directory entry's image offset field.</summary>
    private const int IconDirectoryEntryOffset = 12;

    /// <summary>The icon directory header size, in bytes.</summary>
    private const int IconDirectoryHeaderSize = 6;

    /// <summary>The byte offset of an icon directory entry's image size field.</summary>
    private const int IconDirectoryImageSizeOffset = 8;

    /// <summary>The icon directory entry size, in bytes.</summary>
    private const int IconDirectorySize = 16;

    /// <summary>The byte offset of the icon count field.</summary>
    private const int IconCountOffset = 4;

    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="iconStream">The extended instance.</param>
    extension(Stream iconStream)
    {
        /// <summary>Extracts a Vista PNG icon from an icon stream.</summary>
        /// <returns>The extracted bitmap, or null when no Vista icon is found.</returns>
        public Bitmap ExtractVistaIcon()
        {
            Bitmap extractedBitmap = null;
            checked
            {
                try
                {
                    var sourceBuffer = new byte[iconStream.Length];
                    _ = iconStream.Read(sourceBuffer, 0, (int)iconStream.Length);
                    var count = BitConverter.ToInt16(sourceBuffer, IconCountOffset);
                    for (var index = 0; index < count; index++)
                    {
                        var entryOffset = IconDirectoryHeaderSize + (IconDirectorySize * index);
                        var num = sourceBuffer[entryOffset];
                        var height = sourceBuffer[entryOffset + 1];
                        if (num == 0 && height == 0)
                        {
                            var imageSize =
                                BitConverter.ToInt32(sourceBuffer, entryOffset + IconDirectoryImageSizeOffset);
                            var imageOffset =
                                BitConverter.ToInt32(sourceBuffer, entryOffset + IconDirectoryEntryOffset);
                            using (MemoryStream destinationStream = new())
                            {
                                destinationStream.Write(sourceBuffer, imageOffset, imageSize);
                                _ = destinationStream.Seek(0L, SeekOrigin.Begin);
                                extractedBitmap = new(destinationStream);
                            }

                            break;
                        }
                    }
                }
                catch (IOException)
                {
                    return null;
                }
                catch (ArgumentException)
                {
                    return null;
                }

                return extractedBitmap;
            }
        }
    }
}
