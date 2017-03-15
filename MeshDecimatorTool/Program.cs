using System;
using System.IO;
using System.Globalization;
using MeshDecimator;
using MeshDecimator.Math;

namespace MeshDecimatorTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            try
            {
                string sourcePath = Path.GetFullPath(args[0]);
                string destPath = Path.GetFullPath(args[1]);

                float quality = 0.5f;
                if (args.Length > 2)
                {
                    if (!float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out quality))
                    {
                        Console.Error.WriteLine("Unable to convert '{0}' into a float.", args[2]);
                        return;
                    }
                }

                quality = MathHelper.Clamp01(quality);
                ObjMesh sourceObjMesh = new ObjMesh();
                sourceObjMesh.ReadFile(sourcePath);
                var sourceVertices = sourceObjMesh.Vertices;
                var sourceIndices = sourceObjMesh.Indices;
                var sourceMesh = new Mesh(sourceVertices, sourceIndices);

                int currentTriangleCount = sourceIndices.Length / 3;
                int targetTriangleCount = (int)Math.Ceiling(currentTriangleCount * quality);
                Console.WriteLine("Input: {0} vertices, {1} triangles (target {2})",
                    sourceVertices.Length, currentTriangleCount, targetTriangleCount);

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Reset();
                stopwatch.Start();

                var algorithm = MeshDecimation.CreateAlgorithm(Algorithm.Default);
                algorithm.Verbose = true;
                Mesh destMesh = MeshDecimation.DecimateMesh(algorithm, sourceMesh, targetTriangleCount);
                stopwatch.Stop();

                var destVertices = destMesh.Vertices;
                var destIndices = destMesh.Indices;
                ObjMesh destObjMesh = new ObjMesh(destVertices, destIndices);
                destObjMesh.WriteFile(destPath);

                float reduction = (float)(destIndices.Length / 3) / (float)currentTriangleCount;
                float timeTaken = (float)stopwatch.Elapsed.TotalSeconds;
                Console.WriteLine("Output: {0} vertices, {1} triangles ({2} reduction; {3:0.0000} sec)",
                    destVertices.Length, destIndices.Length / 3, reduction, timeTaken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error decimating mesh. Reason: {0}", ex.Message);
            }
        }

        private static void PrintUsage()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.Error.WriteLine("Needs at least two arguments.");
            Console.WriteLine("Usage: {0} <source> <dest> (quality)", processName);
        }
    }
}