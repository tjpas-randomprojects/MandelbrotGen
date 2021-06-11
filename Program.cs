/* Mandelbrot Fractal Generator Without Tutorial
 * 
 * Version 1.0 - 2:28 AM 6-07-2021
 *   - Currently generates fractals and zooms at good perceptive rate
 *   - Iteration control needs more work
 *   - SINGLE THREADED
 *   - No anti-aliasing
 *   - Only single-spectrum color management
 *   
 * Version 1.1 - 11:57 AM 6-08-2021
 *   - Multi-threaded
 *     - Can set limit on maximum thread count
 *     - File stream is locked so only one file can be written at a time, no overwrites
 *   - Iteration control is better, needs improvement
 *   - Color spectrum control needed, makes iteration control irrelevant
 *   - No errors in the for loop skipping numbers now
 * 
 * *Desired Features* for Version 1.2
 *   - Add Diagnostic Timer to time operation length
 *     - Possibly add a log file that logs frame number, iteration count, resolution, and time elapsed.
 *   - Benchmarked testing for finding optimal thread counts
 *   - Multi-color spectrum, test zooms for noisy flashes
 *   - Anti-Aliasing or Denoising if possible (Likely not possible)
 */

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FastMandelbrotGenTesting
{
    class Functioning
    {
        private static readonly object locker = new object();
        static void Main(string[] args)

        {
            Stopwatch timmy = new Stopwatch();
            Stopwatch jimmy = new Stopwatch();

            /* Use this to add prompts for resolution at program start. Make sure to remove the xsize and ysize variables below. */
            // Console.WriteLine("X Dimension: ");
            // int xsize = Convert.ToInt32(Console.ReadLine());
            // Console.WriteLine("Y Dimension ");
            // int ysize = Convert.ToInt32(Console.ReadLine());

            int xsize = 640;
            int ysize = 360;

            // Initialize current thread count, limit to 4 threads max
            int threaders = 0;
            int max_threaders = 8;


            // Console.WriteLine("How Many Frames? ");
            // int framecount = Convert.ToInt32(Console.ReadLine());
            int framecount = 120;

            jimmy.Start();
            for (int i = 0; i <= framecount; i++)
            {
            checker:
                if (threaders < max_threaders)
                {
                    Task.Factory.StartNew(() =>
                    {
                        // timmy.Start();
                        threaders++;
                        string name = i.ToString();

                        double zeem = 5.0 / Math.Pow(1.05, i);
                        // float it = (float) Math.Round(8.0 * Math.Pow(1.015, i), 0);

                        Mandelbrot(xsize, ysize, 0.251, 0.00005, 32, zeem, i);
                        // timmy.Stop();
                        // int ts = (int) timmy.ElapsedMilliseconds;

                        Console.WriteLine("Frame " + name + " Complete."); // + " RunTime " + ts.ToString() + " ms." );
                        // timmy.Reset();
                        threaders--;
                    });
                    Thread.Sleep(0003);
                }
                else
                {
                    goto checker;
                }
            }
            jimmy.Stop();
            int js = (int) jimmy.ElapsedMilliseconds;

            Console.WriteLine("Total Time Elaspsed: " + js.ToString());

            Console.ReadKey();

        }
        static void Mandelbrot(int resx, int resy, double originx, double originy, float max_iter, double zoom, int frame)
        {
            using (Image<Rgba32> image = new Image<Rgba32>(resx, resy))
            {
                //Define Other Variables
                double x0;      // Initial x coord
                double y0;      // Iniital y coord
                double a;       // "x" in equations
                double b;       // "y" in equations
                double a2;
                double b2;
                double aCol;
                double bCol;
                float iter;     // Current iteration count. It's a float for now due to the variability needed. Will be an int again in the future


                // Goes through every pixel in render resolution, calculates the output and assigns it a color.
                for (int y = 0; y < image.Height; y++)
                {
                    Span<Rgba32> pixelRowSpan = image.GetPixelRowSpan(y);
                    for (int x = 0; x < image.Width; x++)
                    {
                        // Set initial values using current pixel, scale and transform to custom coordinate system.
                        x0 = (x - (resx / 2.0)) * (zoom / resy) + originx;
                        y0 = (y - (resy / 2.0)) * (zoom / resy) + originy;

                        // Initialize values for new pixel.
                        a = 0.0;
                        b = 0.0;
                        iter = 0.0f;

                        a2 = 0.0;
                        b2 = 0.0;

                        // While the iteration result is within the 2r circle, and the iterations are below the limit.
                        while (a2 + b2 <= 1 << 16 & iter <= max_iter)
                        {
                            // Funky Math Stuff
                            aCol = a2;
                            bCol = b2;

                            b = (a + a) * b + y0;
                            a = a2 - b2 + x0;

                            a2 = a * a;
                            b2 = b * b;
                            iter++;
                        }


                        // COLOR MANAGEMENT. NEEDS FUTURE WORK FOR MULTI-COLOR, ITERATION-INDEPENDENT RENDERS
                        // Make center set black, vary color for all else
                        if (iter <= max_iter)
                        {
                            double log_zn = Math.Log(a2 + b2) / 2;
                            float nu = (float) Math.Log(log_zn / Math.Log(2)) / (float) Math.Log(2);

                            iter = iter + 1 - nu;
                            /* Old. Keeping as a backup.
                            float shade = iter / max_iter;
                            pixelRowSpan[x] = new Rgba32(shade, shade, shade, 255.0f);
                            */
                        }
                        else
                        {
                            pixelRowSpan[x] = new Rgba32(0.0f, 0.0f, 0.0f, 255.0f);
                        }
                    }       
                }
                lock(locker)
                {
                    image.Save("frame_" + frame + ".png");
                }
            }
        }
    }
}