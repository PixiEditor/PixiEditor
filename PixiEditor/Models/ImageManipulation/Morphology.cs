using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.Models.ImageManipulation
{
    public class Morphology
    {

        public static Coordinates[] ApplyDilation(Coordinates[] points, int kernelSize, int[,] mask)
        {
            int kernelDim = kernelSize;

            //This is the offset of center pixel from border of the kernel
            int kernelOffset = (kernelDim - 1) / 2;
            int margin = kernelDim;

            byte[,] byteImg = GetByteArrayForPoints(points, margin);
            byte[,] outputArray = byteImg.Clone() as byte[,];
            Coordinates offset = new Coordinates(points.Min(x => x.X) - margin, points.Min(x => x.Y) - margin);

            int width = byteImg.GetLength(0);
            int height = byteImg.GetLength(1);
            for (int y = kernelOffset; y < height - kernelOffset; y++)
            {
                for (int x = kernelOffset; x < width - kernelOffset; x++)
                {
                    byte value = 0;

                    //Apply dilation
                    for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                    {
                        for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                        {
                            if (mask[xkernel + kernelOffset, ykernel + kernelOffset] == 1)
                            {
                                value = Math.Max(value, byteImg[x + xkernel, y + ykernel]);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    //Write processed data into the second array
                    outputArray[x, y] = value;
                }
            }
            return ToCoordinates(outputArray, offset).Distinct().ToArray();
        }

        private static Coordinates[] ToCoordinates(byte[,] byteArray, Coordinates offset)
        {
            List<Coordinates> output = new List<Coordinates>();
            int width = byteArray.GetLength(0);

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (byteArray[x, y] == 1)
                    {
                        output.Add(new Coordinates(x + offset.X, y + offset.Y));
                    }

                }
            }
            return output.ToArray();
        }

        private static byte[,] GetByteArrayForPoints(Coordinates[] points, int margin)
        {
            Tuple<int, int> dimensions = GetDimensionsForPoints(points);
            int minX = points.Min(x => x.X);
            int minY = points.Min(x => x.Y);
            byte[,] array = new byte[dimensions.Item1 + margin * 2, dimensions.Item2 + margin * 2];
            //Debug.Write("----------\n");

            for (int y = 0; y < dimensions.Item2 + margin; y++)
            {
                for (int x = 0; x < dimensions.Item1 + margin; x++)
                {
                    Coordinates cords = new Coordinates(x + minX, y + minY);
                    array[x + margin, y + margin] = points.Contains(cords) ? (byte)1 : (byte)0;
                }
            }

            //for (int y = 0; y < array.GetLength(1); y++)
            //{
            //    for (int x = 0; x < array.GetLength(0); x++)
            //    {
            //        Debug.Write($"{array[x, y]} ");
            //    }
            //    Debug.Write("\n");
            //}

            return array;
        }

        private static Tuple<int, int> GetDimensionsForPoints(Coordinates[] points)
        {
            int width = points.Max(x => x.X) - points.Min(x => x.X);
            int height = points.Max(x => x.Y) - points.Min(x => x.Y);
            return new Tuple<int, int>(width + 1, height + 1);
        }
    }
}
