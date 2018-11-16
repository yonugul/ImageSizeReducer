﻿using nQuant;
using PnnQuant;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Minimage
{    public class PngQuant : Compressor
    {

        public PngQuant() : base(new string[] { "image/png" }) { }

        public override Task<byte[]> CompressImplementation(byte[] stream)
        {
            var quantizer = new WuQuantizer();
            Bitmap bmp;
            using (var ms = new MemoryStream(stream))
            {
                bmp = new Bitmap(ms);
                using (var quantized = quantizer.QuantizeImage(bmp, 10, 70, null, 256))
                {
                    using (var compressed = new MemoryStream())
                    {
                        quantized.Save(compressed, ImageFormat.Png);
                        return Task.FromResult(compressed.ToArray());
                    }
                }
            }
        }
       
    }
}
