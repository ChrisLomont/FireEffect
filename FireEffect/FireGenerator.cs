using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hypnocube.LargeArtDriver.Model.ImageGenerator.Graphics;

namespace FireEffect
{
    // ported from here
    // http://lodev.org/cgtutor/fire.html
    // used to make fire for the Eggify art project

    class FireGenerator
    {
        public const int screenWidth = 30;
        public const int screenHeight = 100;

        // Y-coordinate first because we use horizontal scanlines
        uint[,] fire = new uint[screenHeight, screenWidth]; 

        // image buffer, RGB, width filled first, then height
        byte [] buffer = new byte[screenHeight * screenWidth * 3]; //this is the buffer to be drawn to the screen

        byte [] palette = new byte [256*3]; //this will contain the color palette

        public FireGenerator()
        {
            Initialize();
        }

        void Initialize()
        {
            var h = screenHeight;
            var w = screenWidth;

            //make sure the fire buffer is zero in the beginning
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                fire[y, x] = 0;

            //generate the palette
            for (var x = 0; x < 256; x++)
            {
                double r, g, b;
                //HSLtoRGB is used to generate colors:
                //Hue goes from 0 to 85: red to yellow
                //Saturation is always the maximum: 255
                //Lightness is 0..255 for x=0..128, and 255 for x=128..255
                var hue = x / 255.0;
                var lit = Math.Min(1.0, hue * 2);
                HslUtils.HslToRgb(hue / 3, 1.0,lit , out r, out g, out b);
                // color = HSLtoRGB(ColorHSL(x / 3, 255, std::min(255, x * 2)));
                //set the palette to the calculated RGB value
                palette[x * 3] = IntC(r);
                palette[x * 3 + 1] = IntC(g);
                palette[x * 3 + 2] = IntC(b);
                // palette[x] = RGBtoINT(color);
            }
        }

        byte IntC(double v)
        {
            var vv = (int) Math.Round(v*255);
            if (vv > 255) vv = 255;
            if (vv < 0) vv = 0;
            return (byte)vv;
        }

        Random rand = new Random();

        // call 20 times per second
        // returns rgb buffer
        public byte[] Update()
        {

            //randomize the bottom row of the fire buffer
            for (int x = 0; x < screenWidth; x++)
                fire[screenHeight - 1, x] = (uint)(Math.Abs(32768 + rand.Next()) % 256);
            //do the fire calculations for every pixel, from top to bottom
            for (int y = 0; y < screenHeight - 1; y++)
            for (int x = 0; x < screenWidth; x++)
            {
                fire[y, x] =
                ((fire[(y + 1) % screenHeight, (x - 1 + screenWidth) % screenWidth]
                  + fire[(y + 1) % screenHeight, (x) % screenWidth]
                  + fire[(y + 1) % screenHeight, (x + 1) % screenWidth]
                  + fire[(y + 2) % screenHeight, (x) % screenWidth])
                 * 32) / 129;
            }

            //set the drawing buffer to the fire buffer, using the palette colors
            for (int y = 0; y < screenHeight; y++)
            for (int x = 0; x < screenWidth; x++)
            {
                var index = (x + y * screenWidth) * 3;
                var color = fire[y, x] * 3;
                buffer[index] = palette[color];
                buffer[index+1] = palette[color+1];
                buffer[index+2] = palette[color+2];
            }
            return buffer;
        }
    }
}
