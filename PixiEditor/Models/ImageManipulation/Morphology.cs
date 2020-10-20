using System;
using System.Collections.Generic;
using System.Linq;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.ImageManipulation
{
    public class Morphology
    {
        public static IEnumerable<Coordinates> ApplyDilation(Coordinates[] points, int kernelSize, int[,] mask)
        {
            var kernelDim = kernelSize;

            //This is the offset of center pixel from border of the kernel
            var kernelOffset = (kernelDim - 1) / 2;
            var margin = kernelDim;

            var byteImg = GetByteArrayForPoints(points, margin);
            var outputArray = byteImg.Clone() as byte[,];
            var offset = new Coordinates(points.Min(x => x.X) - margin, points.Min(x => x.Y) - margin);

            var width = byteImg.GetLength(0);
            var height = byteImg.GetLength(1);
            for (var y = kernelOffset; y < height - kernelOffset; y++)
            for (var x = kernelOffset; x < width - kernelOffset; x++)
            {
                byte value = 0;

                //Apply dilation
                for (var ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                for (var xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                    if (mask[xkernel + kernelOffset, ykernel + kernelOffset] == 1)
                        value = Math.Max(value, byteImg[x + xkernel, y + ykernel]);
                    else
                        continue;
                //Write processed data into the second array
                outputArray[x, y] = value;
            }

            return ToCoordinates(outputArray, offset).Distinct();
        }

        private static IEnumerable<Coordinates> ToCoordinates(byte[,] byteArray, Coordinates offset)
        {
            var output = new List<Coordinates>();
            var width = byteArray.GetLength(0);

            for (var y = 0; y < byteArray.GetLength(1); y++)
            for (var x = 0; x < width; x++)
                if (byteArray[x, y] == 1)
                    output.Add(new Coordinates(x + offset.X, y + offset.Y));
            return output;
        }

        private static byte[,] GetByteArrayForPoints(Coordinates[] points, int margin)
        {
            var dimensions = GetDimensionsForPoints(points);
            var minX = points.Min(x => x.X);
            var minY = points.Min(x => x.Y);
            var array = new byte[dimensions.Item1 + margin * 2, dimensions.Item2 + margin * 2];

            for (var y = 0; y < dimensions.Item2 + margin; y++)
            for (var x = 0; x < dimensions.Item1 + margin; x++)
            {
                var cords = new Coordinates(x + minX, y + minY);
                array[x + margin, y + margin] = points.Contains(cords) ? (byte) 1 : (byte) 0;
            }

            return array;
        }

        private static Tuple<int, int> GetDimensionsForPoints(Coordinates[] points)
        {
            var width = points.Max(x => x.X) - points.Min(x => x.X);
            var height = points.Max(x => x.Y) - points.Min(x => x.Y);
            return new Tuple<int, int>(width + 1, height + 1);
        }
    }
}