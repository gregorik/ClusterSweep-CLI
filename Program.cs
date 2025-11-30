using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ClusterSweep_CLI
{
    // USAGE: ClusterSweep-CLI.exe input.png -o output.png --clean 3 --snap 25
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ClusterSweep-CLI v1.0 [GitHub Edition]");
            Console.WriteLine("Core Logic by GregOrigin");
            Console.ResetColor();

            if (args.Length == 0)
            {
                Console.WriteLine("\nUsage:");
                Console.WriteLine("  ClusterSweep-CLI.exe <input> [-o output] [--clean <passes>] [--snap <threshold>]");
                return;
            }

            string inputPath = args[0];
            string outputPath = "result.png";
            int cleanPasses = 0;
            int snapThreshold = -1;

            // Simple Args Parser
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length) outputPath = args[++i];
                if (args[i] == "--clean" && i + 1 < args.Length) int.TryParse(args[++i], out cleanPasses);
                if (args[i] == "--snap" && i + 1 < args.Length) int.TryParse(args[++i], out snapThreshold);
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: File not found '{inputPath}'");
                return;
            }

            try
            {
                Bitmap bmp = new Bitmap(inputPath);
                Console.WriteLine($"loaded: {Path.GetFileName(inputPath)} ({bmp.Width}x{bmp.Height})");

                // Execute Snap
                if (snapThreshold > 0)
                {
                    Console.Write($"Snapping Palette (T:{snapThreshold})... ");
                    SnapToLocalPalette(bmp, snapThreshold);
                    Console.WriteLine("Done.");
                }

                // Execute Clean
                if (cleanPasses > 0)
                {
                    Console.Write($"Removing Orphans ({cleanPasses} passes)... ");
                    for (int i = 0; i < cleanPasses; i++) RemoveOrphans(bmp);
                    Console.WriteLine("Done.");
                }

                bmp.Save(outputPath, ImageFormat.Png);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ResetColor();
        }

        // --- CORE ALGORITHMS (Ported from GUI) ---

        static unsafe void RemoveOrphans(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int h = bmp.Height;
            int w = bmp.Width;
            byte* ptr = (byte*)data.Scan0;
            int stride = data.Stride;

            // Note: In CLI version, we do strictly in-place for memory efficiency
            // ideally we clone a buffer for reads, but this is "Lite" version.

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int off = y * stride + x * 4;
                    byte b = ptr[off], g = ptr[off + 1], r = ptr[off + 2];

                    int[] n = {
                        (y - 1) * stride + x * 4,
                        (y + 1) * stride + x * 4,
                        y * stride + (x - 1) * 4,
                        y * stride + (x + 1) * 4
                    };

                    bool isOrphan = true;
                    foreach (var no in n)
                    {
                        if (ptr[no] == b && ptr[no + 1] == g && ptr[no + 2] == r)
                        {
                            isOrphan = false; break;
                        }
                    }

                    if (isOrphan)
                    {
                        // Swap with Upper
                        int t = n[0];
                        ptr[off] = ptr[t]; ptr[off + 1] = ptr[t + 1]; ptr[off + 2] = ptr[t + 2]; ptr[off + 3] = ptr[t + 3];
                    }
                }
            }
            bmp.UnlockBits(data);
        }

        static unsafe void SnapToLocalPalette(Bitmap bmp, int threshold)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptr = (byte*)data.Scan0;
            int bytes = Math.Abs(data.Stride) * bmp.Height;

            List<(byte b, byte g, byte r)> palette = new List<(byte, byte, byte)>();

            for (int i = 0; i < bytes; i += 4)
            {
                byte b = ptr[i], g = ptr[i + 1], r = ptr[i + 2], a = ptr[i + 3];
                if (a == 0) continue;

                bool merged = false;
                for (int p = 0; p < palette.Count; p++)
                {
                    var c = palette[p];
                    double dist = Math.Sqrt(Math.Pow(c.b - b, 2) + Math.Pow(c.g - g, 2) + Math.Pow(c.r - r, 2));
                    if (dist < threshold)
                    {
                        ptr[i] = c.b; ptr[i + 1] = c.g; ptr[i + 2] = c.r;
                        merged = true; break;
                    }
                }
                if (!merged && palette.Count < 256) palette.Add((b, g, r));
            }
            bmp.UnlockBits(data);
        }
    }
}