// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Helper class for creating icon files using the proper ICO file format structures.</summary>
public static class IconFileWriter
{
    /// <summary>Defines the modern PNG icon bit depth.</summary>
    private const ushort PngIconBitCount = 32;

    /// <summary>Writes icon images to a stream using the ICO file format with proper structures.</summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="images">Collection of images to include in the icon.</param>
    public static void WriteIconFile(Stream stream, IEnumerable<Image> images)
    {
        Throw.IfNull(stream);
        Throw.IfNull(images);
        var imageList = MaterializeImages(images);
        if (imageList.Count == 0)
        {
            throw new ArgumentException("At least one image is required", nameof(images));
        }

        checked
        {
            using BinaryWriter binaryWriter = new(stream, Encoding.Default, leaveOpen: true);
            List<EncodedIconImage> encodedImages = new();
            try
            {
                foreach (var image in imageList)
                {
                    MemoryStream imageStream = new();
                    image.Save(imageStream, ImageFormat.Png);
                    _ = imageStream.Seek(0L, SeekOrigin.Begin);
                    encodedImages.Add(new(image.Size, imageStream));
                }

                WriteIconDir(binaryWriter, IconDir.CreateIcon((ushort)encodedImages.Count));
                var offset = (uint)(IconDir.Size + (encodedImages.Count * IconDirEntry.Size));
                foreach (var encodedImage in encodedImages)
                {
                    IconDirEntry entry = IconDirEntry.CreateForIcon(
                        encodedImage.Size.Width,
                        encodedImage.Size.Height,
                        PngIconBitCount,
                        (uint)encodedImage.Data.Length,
                        offset);
                    WriteIconDirEntry(binaryWriter, entry);
                    offset += (uint)encodedImage.Data.Length;
                }

                foreach (var item in encodedImages)
                {
                    item.Data.WriteTo(stream);
                }
            }
            finally
            {
                DisposeEncodedImages(encodedImages);
            }
        }
    }

    /// <summary>Writes icon images to a file using the ICO file format.</summary>
    /// <param name="filePath">Path to the output icon file.</param>
    /// <param name="images">Collection of images to include in the icon.</param>
    public static void WriteIconFile(string filePath, IEnumerable<Image> images)
    {
        Throw.IfNull(filePath);
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        WriteIconFile(fileStream, images);
    }

    /// <summary>Writes cursor images to a stream using the CUR file format.</summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="images">Collection of images with hotspot information.</param>
    public static void WriteCursorFile(Stream stream, IEnumerable<(Image Image, Point Hotspot)> images)
    {
        Throw.IfNull(stream);
        Throw.IfNull(images);
        var imageList = MaterializeCursorImages(images);
        if (imageList.Count == 0)
        {
            throw new ArgumentException("At least one image is required", nameof(images));
        }

        checked
        {
            using BinaryWriter binaryWriter = new(stream, Encoding.Default, leaveOpen: true);
            List<EncodedCursorImage> encodedImages = new();
            try
            {
                foreach (var cursorImage in imageList)
                {
                    MemoryStream imageStream = new();
                    cursorImage.Image.Save(imageStream, ImageFormat.Png);
                    _ = imageStream.Seek(0L, SeekOrigin.Begin);
                    encodedImages.Add(new(cursorImage.Image.Size, cursorImage.Hotspot, imageStream));
                }

                WriteIconDir(binaryWriter, IconDir.CreateCursor((ushort)encodedImages.Count));
                var offset = (uint)(IconDir.Size + (encodedImages.Count * IconDirEntry.Size));
                foreach (var encodedImage in encodedImages)
                {
                    IconDirEntry entry = IconDirEntry.CreateForCursor(
                        encodedImage.Size.Width,
                        encodedImage.Size.Height,
                        (ushort)encodedImage.Hotspot.X,
                        (ushort)encodedImage.Hotspot.Y,
                        (uint)encodedImage.Data.Length,
                        offset);
                    WriteIconDirEntry(binaryWriter, entry);
                    offset += (uint)encodedImage.Data.Length;
                }

                foreach (var item in encodedImages)
                {
                    item.Data.WriteTo(stream);
                }
            }
            finally
            {
                DisposeEncodedImages(encodedImages);
            }
        }
    }

    /// <summary>Writes cursor images to a file using the CUR file format.</summary>
    /// <param name="filePath">Path to the output cursor file.</param>
    /// <param name="images">Collection of images with hotspot information.</param>
    public static void WriteCursorFile(string filePath, IEnumerable<(Image Image, Point Hotspot)> images)
    {
        Throw.IfNull(filePath);
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        WriteCursorFile(fileStream, images);
    }

    /// <summary>Writes a GRPICONDIR structure to a binary writer.</summary>
    /// <param name="writer">Binary writer.</param>
    /// <param name="grpIconDir">GrpIconDir structure to write.</param>
    public static void WriteGrpIconDir(BinaryWriter writer, GrpIconDir grpIconDir)
    {
        Throw.IfNull(writer);
        writer.Write(grpIconDir.Reserved);
        writer.Write(grpIconDir.Type);
        writer.Write(grpIconDir.Count);
    }

    /// <summary>Writes a GRPICONDIRENTRY structure to a binary writer.</summary>
    /// <param name="writer">Binary writer.</param>
    /// <param name="entry">GrpIconDirEntry structure to write.</param>
    public static void WriteGrpIconDirEntry(BinaryWriter writer, GrpIconDirEntry entry)
    {
        Throw.IfNull(writer);
        writer.Write(entry.Width);
        writer.Write(entry.Height);
        writer.Write(entry.ColorCount);
        writer.Write(entry.Reserved);
        writer.Write(entry.Planes);
        writer.Write(entry.BitCount);
        writer.Write(entry.BytesInRes);
        writer.Write(entry.Id);
    }

    /// <summary>Writes an ICONDIR structure.</summary>
    /// <param name="writer">Binary writer.</param>
    /// <param name="iconDir">Icon directory structure.</param>
    private static void WriteIconDir(BinaryWriter writer, IconDir iconDir)
    {
        writer.Write(iconDir.Reserved);
        writer.Write(iconDir.Type);
        writer.Write(iconDir.Count);
    }

    /// <summary>Writes an ICONDIRENTRY structure.</summary>
    /// <param name="writer">Binary writer.</param>
    /// <param name="entry">Icon directory entry.</param>
    private static void WriteIconDirEntry(BinaryWriter writer, IconDirEntry entry)
    {
        writer.Write(entry.Width);
        writer.Write(entry.Height);
        writer.Write(entry.ColorCount);
        writer.Write(entry.Reserved);
        writer.Write(entry.Planes);
        writer.Write(entry.BitCount);
        writer.Write(entry.BytesInRes);
        writer.Write(entry.ImageOffset);
    }

    /// <summary>Copies image enumeration into a list.</summary>
    /// <param name="images">Images to materialize.</param>
    /// <returns>The materialized images.</returns>
    private static List<Image> MaterializeImages(IEnumerable<Image> images)
    {
        List<Image> imageList = new();
        foreach (var image in images)
        {
            imageList.Add(image);
        }

        return imageList;
    }

    /// <summary>Copies cursor image enumeration into a list.</summary>
    /// <param name="images">Cursor images to materialize.</param>
    /// <returns>The materialized cursor images.</returns>
    private static List<CursorImage> MaterializeCursorImages(IEnumerable<(Image Image, Point Hotspot)> images)
    {
        List<CursorImage> imageList = new();
        foreach (var image in images)
        {
            imageList.Add(new(image.Image, image.Hotspot));
        }

        return imageList;
    }

    /// <summary>Disposes encoded icon image streams.</summary>
    /// <param name="encodedImages">Encoded icon images.</param>
    private static void DisposeEncodedImages(IEnumerable<EncodedIconImage> encodedImages)
    {
        foreach (var encodedImage2 in encodedImages)
        {
            encodedImage2.Data.Dispose();
        }
    }

    /// <summary>Disposes encoded cursor image streams.</summary>
    /// <param name="encodedImages">Encoded cursor images.</param>
    private static void DisposeEncodedImages(IEnumerable<EncodedCursorImage> encodedImages)
    {
        foreach (var encodedImage2 in encodedImages)
        {
            encodedImage2.Data.Dispose();
        }
    }

    /// <summary>Stores a cursor image and hotspot pair.</summary>
    /// <param name="Image">The cursor image.</param>
    /// <param name="Hotspot">The cursor hotspot.</param>
    private readonly record struct CursorImage(Image Image, Point Hotspot);

    /// <summary>Stores an encoded icon image.</summary>
    /// <param name="Size">The source image size.</param>
    /// <param name="Data">The encoded image data.</param>
    private readonly record struct EncodedIconImage(Size Size, MemoryStream Data);

    /// <summary>Stores an encoded cursor image.</summary>
    /// <param name="Size">The source image size.</param>
    /// <param name="Hotspot">The cursor hotspot.</param>
    /// <param name="Data">The encoded image data.</param>
    private readonly record struct EncodedCursorImage(Size Size, Point Hotspot, MemoryStream Data);
}
