#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

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
                var sourceNormals = sourceObjMesh.Normals;
                var sourceTexCoords2D = sourceObjMesh.TexCoords2D;
                var sourceTexCoords3D = sourceObjMesh.TexCoords3D;
                var sourceIndices = sourceObjMesh.Indices;

                var sourceMesh = new Mesh(sourceVertices, sourceIndices);
                sourceMesh.Normals = sourceNormals;

                if (sourceTexCoords2D != null)
                {
                    sourceMesh.SetUVs(0, sourceTexCoords2D);
                }
                else if (sourceTexCoords3D != null)
                {
                    sourceMesh.SetUVs(0, sourceTexCoords3D);
                }

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
                var destNormals = destMesh.Normals;
                var destIndices = destMesh.Indices;

                ObjMesh destObjMesh = new ObjMesh(destVertices, destIndices);
                destObjMesh.Normals = destNormals;

                if (sourceTexCoords2D != null)
                {
                    var destUVs = destMesh.GetUVs2D(0);
                    destObjMesh.TexCoords2D = destUVs;
                }
                else if (sourceTexCoords3D != null)
                {
                    var destUVs = destMesh.GetUVs3D(0);
                    destObjMesh.TexCoords3D = destUVs;
                }

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