using Minimage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSizeReducer
{
    internal class Program
    {
        private static object _syncLock = new();
        private static List<string> _list = new();
        private static string from = AppDomain.CurrentDomain.BaseDirectory;
        private static string convertedlist = $"{from}converted.txt";
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AsyncMain(null).GetAwaiter().GetResult();
        }
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            lock (_syncLock)
            {
                WriteToFile();
            }
            Console.WriteLine("exit");
        }

        private static async Task AsyncMain(string[] args)
        {

            Compressor pngQuant = new PngQuant(new PngQuantOptions()
            {
                QualityMinMax = (5, 10)
            });
            var allPngs = Directory.GetFiles(Path.Combine(from), "*.png", SearchOption.AllDirectories);
            if (!File.Exists(convertedlist))
            {
                await File.WriteAllTextAsync(convertedlist, "");
            }
            var existconvert = await File.ReadAllLinesAsync(convertedlist);
            if (existconvert.Any())
            {
                Console.WriteLine("{0} - onceden donusmus", existconvert.Length);
            }
            allPngs = allPngs.Except(existconvert).ToArray();
            var c = 1;
            var all = allPngs.Length;
            Console.WriteLine("{0} - donusecek", all);
            var max = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75));
            Console.WriteLine("{0} - thread at same time", max);

            Parallel.ForEach(allPngs, new ParallelOptions { MaxDegreeOfParallelism = max }, s =>
            {
                try
                {
                    var r = File.ReadAllBytes(s);
                    var compressedbytes = pngQuant.Compress(r).Result;
                    File.WriteAllBytesAsync(s, compressedbytes);
                    Console.WriteLine($"{s} - converted... ({c}/{all})");
                    _list.Add(s);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{s} - notconverted... ({c}/{all}-{e.Message})");
                    Console.ResetColor();
                }
                c++;
            });
            WriteToFile();
            Console.WriteLine("DONE!");

            Console.ReadLine();

            await AsyncMain(args);
        }

        private static void WriteToFile()
        {
            if (!File.Exists(convertedlist))
            {
                File.WriteAllText(convertedlist, "");
            }
            var existconvert = File.ReadAllLines(convertedlist);
            var toWrite = _list.Except(existconvert);
            using (var sw = File.AppendText(convertedlist))
            {
                foreach (var rec in toWrite)
                {
                    sw.WriteLine(rec);
                }
            }
        }
    }
}
