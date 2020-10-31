using Google.Protobuf;
using HiveMind;
using Microsoft.AspNetCore.Mvc;
using SC2APIProtocol;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Sys = System.IO;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BitmapController : ControllerBase
    {
        [Route("[action]")]
        public FileContentResult PlacementGrid()
        {
            return File(AddHeader(Game.ResponseGameInfo.StartRaw.PlacementGrid), "image/bmp", "bitmap.bmp");
        }

        [Route("[action]")]
        public FileResult PlacementGrid2()
        {
            var bitmap = new Bitmap(Game.ResponseGameInfo.StartRaw.PlacementGrid.Size.X,Game.ResponseGameInfo.StartRaw.PlacementGrid.Size.Y, PixelFormat.Format1bppIndexed);

            //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.unlockbits?view=dotnet-plat-ext-3.1#System_Drawing_Bitmap_UnlockBits_System_Drawing_Imaging_BitmapData_

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            var ptr = bmpData.Scan0;
            var bytes = AddPadding(Game.ResponseGameInfo.StartRaw.PlacementGrid);
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, ptr, bytes.Length);
            bitmap.UnlockBits(bmpData);
            
            var stream = new MemoryStream();
            bitmap.Save(stream,ImageFormat.Bmp);
            return File(stream, "image/bmp", "bitmap.bmp");
        }

        [Route("[action]")]
        public ImageData PlacementGridRaw()
        {
            return Game.ResponseGameInfo.StartRaw.PlacementGrid;
        }

        [Route("[action]")]
        public FileContentResult PathingGrid()
        {
            return File(AddHeader(Game.ResponseGameInfo.StartRaw.PathingGrid), "image/bmp", "bitmap.bmp");
        }

        [Route("[action]")]
        public RectangleI PlayableArea()
        {
            return Game.ResponseGameInfo.StartRaw.PlayableArea;
        }

        private byte[] AddHeader(ImageData imageData)
        {
            var paddedData = AddPadding(imageData);

            return AddHeader(paddedData, paddedData.Length, imageData.Size.X, imageData.Size.Y);
        }


        //TODO: Check orientation of source bits ((0,0) is bottom left)
        private static byte[] AddPadding(ImageData imageData)
        {
            var pixelData = imageData.Data.ToByteArray();

            // Bitmaps: Each row in the Pixel array is padded to a multiple of 4 bytes in size
            var numOfPaddedBytes = (imageData.Size.X / 8) % 4;
            int bytesPerRow = imageData.Size.X / 8;
            var paddedData = new byte[(bytesPerRow + numOfPaddedBytes) * imageData.Size.Y];
            var newByteIndex = 0;
            for (int y = 0; y < imageData.Size.Y; y++)
            {
                for (int i = 0; i < bytesPerRow; i++)
                {
                    paddedData[newByteIndex] = pixelData[y * bytesPerRow + i];
                    newByteIndex++;
                }
                newByteIndex += numOfPaddedBytes;
            }

            return paddedData;
        }

        // https://stackoverflow.com/questions/11452246/add-a-bitmap-header-to-a-byte-array-then-create-a-bitmap-file
        // BmpBufferSize : a pure length of raw bitmap data without the header.
        public byte[] AddHeader(byte[] pixelData, int bmpBufferSize, int width, int height)
        {
            // the value here is the length of bitmap header.
            const int headerSize = 62;

            byte[] bitmapBytes = new byte[bmpBufferSize + headerSize];

            #region Bitmap Header
            // 0~2 "BM"
            bitmapBytes[0] = 0x42;
            bitmapBytes[1] = 0x4d;

            // 2~6 Size of the BMP file - Bit cound + Header 54
            Array.Copy(BitConverter.GetBytes(bmpBufferSize + headerSize), 0, bitmapBytes, 2, 4);

            // 6~8 Application Specific : normally, set zero
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 6, 2);

            // 8~10 Application Specific : normally, set zero
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 8, 2);

            // 10~14 Offset where the pixel array can be found - 24bit bitmap data always starts at 54 offset.
            Array.Copy(BitConverter.GetBytes(headerSize), 0, bitmapBytes, 10, 4);
            #endregion

            #region DIB Header
            // 14~18 Number of bytes in the DIB header. 40 bytes constant.
            Array.Copy(BitConverter.GetBytes(40), 0, bitmapBytes, 14, 4);

            // 18~22 Width of the bitmap.
            Array.Copy(BitConverter.GetBytes(width), 0, bitmapBytes, 18, 4);

            // 22~26 Height of the bitmap.
            Array.Copy(BitConverter.GetBytes(height), 0, bitmapBytes, 22, 4);

            // 26~28 Number of color planes being used
            Array.Copy(BitConverter.GetBytes(1), 0, bitmapBytes, 26, 2);

            // 28~30 Number of bits per pixel. If you don't know the pixel format, trying to calculate it with the quality of the video/image source.
            //if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            //{
            Array.Copy(BitConverter.GetBytes(1), 0, bitmapBytes, 28, 2);
            //}

            // 30~34 BI_RGB no pixel array compression used : most of the time, just set zero if it is raw data.
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 30, 4);

            // 34~38 Size of the raw bitmap data ( including padding )
            Array.Copy(BitConverter.GetBytes(bmpBufferSize), 0, bitmapBytes, 34, 4);

            // 38~46 Print resolution of the image, 72 DPI x 39.3701 inches per meter yields
            //if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            //{
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 38, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 42, 4);
            //}

            // 46~50 Number of colors in the palette
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 46, 4);

            // 50~54 means all colors are important
            Array.Copy(BitConverter.GetBytes(0), 0, bitmapBytes, 50, 4);

            // 1bbp, RGB colour of non-black colour
            Array.Copy(BitConverter.GetBytes(255), 0, bitmapBytes, 58, 1);
            Array.Copy(BitConverter.GetBytes(255), 0, bitmapBytes, 59, 1);
            Array.Copy(BitConverter.GetBytes(255), 0, bitmapBytes, 60, 1);

            // 54~end : Pixel Data : Finally, time to combine your raw data, BmpBuffer in this code, with a bitmap header you've just created.
            Array.Copy(pixelData, 0, bitmapBytes, headerSize, bmpBufferSize);
            #endregion - bitmap header process

            return bitmapBytes;
        }
    }
}
