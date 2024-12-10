////////////////////////////////////////////////////////////////
//                                                            //
//  Neoforce Controls                                         //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//         File: ContentReaders.cs                            //
//                                                            //
//      Version: 0.7                                          //
//                                                            //
//         Date: 11/09/2010                                   //
//                                                            //
//       Author: Tom Shane                                    //
//                                                            //
////////////////////////////////////////////////////////////////
//                                                            //
//  Copyright (c) by Tom Shane                                //
//                                                            //
////////////////////////////////////////////////////////////////

#region //// Using /////////////

//////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;


//////////////////////////////////////////////////////////////////////////////

#endregion

namespace TomShane.Neoforce.Controls
{
    ////////////////////////////////////////////////////////////////////////////
    public class LayoutXmlDocument : XmlDocument { }

    public class SkinXmlDocument : XmlDocument { }
    ////////////////////////////////////////////////////////////////////////////


    public class SkinReader : ContentTypeReader<SkinXmlDocument>
    {
        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////
        protected override SkinXmlDocument Read(ContentReader input, SkinXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                SkinXmlDocument doc = new SkinXmlDocument();
                doc.LoadXml(input.ReadString());
                return doc;
            }
            else
            {
                existingInstance.LoadXml(input.ReadString());
            }

            return existingInstance;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }

    public class LayoutReader : ContentTypeReader<LayoutXmlDocument>
    {
        #region //// Methods ///////////

        ////////////////////////////////////////////////////////////////////////////
        protected override LayoutXmlDocument Read(ContentReader input, LayoutXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                LayoutXmlDocument doc = new LayoutXmlDocument();
                doc.LoadXml(input.ReadString());
                return doc;
            }
            else
            {
                existingInstance.LoadXml(input.ReadString());
            }

            return existingInstance;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }

#if (!XBOX && !XBOX_FAKE)

    public class CursorReader : ContentTypeReader<Cursor>
    {
        #region //// Methods ///////////

        // Todo: Don't talk about this....
        private static Cursor _LastReadCursor = null;

        ////////////////////////////////////////////////////////////////////////////
        protected override Cursor Read(ContentReader input, Cursor existingInstance)
        {
            if (existingInstance != null) return existingInstance;
            int xnbSize = input.ReadInt32();
            Span<byte> curBytes = input.ReadBytes(xnbSize);

            var postHeader = curBytes.Read<CurHeader>(out var curHeader);

            if (curHeader.Reserved != 0 || curHeader.Type != 2 || curHeader.Count == 0)
            {
                return _LastReadCursor;
                //throw new Exception("Invalid CUR file");
            }

            var iconHeader = postHeader.Read<IconDirEntry>();
            var imageData = curBytes.Slice((int)iconHeader.ImageOffset, (int)iconHeader.ImageSize);


            var imagePixelDataBytes = imageData.Read<BitmapInfoHeader>(out var bmpHeader);
            var postBMPRemainderPixels = MemoryMarshal.Cast<byte, Color>(imagePixelDataBytes);
            var pixelCount = (bmpHeader.Height / 2) * bmpHeader.Width;

            var colorHalf = postBMPRemainderPixels[..pixelCount];
            var transparencyBytes = imagePixelDataBytes[(pixelCount * 4)..];
            var colorArr = new Color[pixelCount];

            for (int i = 0; i < colorArr.Length; i++)
            {
                var col = colorHalf[i];
                var transByteIndex = i / 8;
                var transByte = transparencyBytes[transByteIndex];
                var bit = i % 8;
                var mask = 1 << bit;
                var isAlpha = (transByte & mask) == 0;
                var result = new Color();
                result.R = col.B;
                result.G = col.G;
                result.B = col.R;

                result.A = isAlpha ? (byte)0 : byte.MaxValue;
            }

            if (bmpHeader.BitCount != 32)
                throw new Exception("Only 32bpp images supported");

            _LastReadCursor = new Cursor(colorArr, iconHeader.Width, iconHeader.Height, iconHeader.HotspotX, iconHeader.HotspotY);
            return _LastReadCursor;
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CurHeader
    {
        public ushort Reserved; // Must be 0
        public ushort Type; // Type 2 for CUR
        public ushort Count; // Number of images
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IconDirEntry
    {
        public byte Width;
        public byte Height;
        public byte ColorCount;
        public byte Reserved;
        public ushort HotspotX;
        public ushort HotspotY;
        public uint ImageSize;
        public uint ImageOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInfoHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ClrUsed;
        public uint ClrImportant;
    }

    public static class WhyTheFuck
    {
        public static ref T Read<T>(this Span<byte> bytes) where T : struct
        {
            var hSpan = MemoryMarshal.Cast<byte, T>(bytes);
            return ref hSpan[0];
        }


        public static Span<byte> Read<T>(this Span<byte> bytes, out T value) where T : struct
        {
            value = MemoryMarshal.Read<T>(bytes);
            var length = Unsafe.SizeOf<T>();
            return bytes[length..];
        }
    }

#endif
}
