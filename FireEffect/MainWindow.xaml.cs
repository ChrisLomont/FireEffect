using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FireEffect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(30); // 50 fps
            timer.Tick += (o, e) => Tick();
            timer.Start();
        }

        WriteableBitmap bmp = new WriteableBitmap(FireGenerator.screenWidth, FireGenerator.screenHeight, 96.0,96.0,PixelFormats.Rgb24,null);

        DispatcherTimer timer = new DispatcherTimer();

        // eggify is 6 arms, each 3 pixels (weird spacing - make them 5), add 10 pixels in between
        WriteableBitmap eggifyBmp = new WriteableBitmap(90,170,96.0,96.0,PixelFormats.Rgb24,null);

        void Tick()
        {
            var data = fire.Update();
            bmp.WritePixels(new Int32Rect(0,0,FireGenerator.screenWidth,FireGenerator.screenHeight),data,FireGenerator.screenWidth*3,0);
            image.Source = bmp;
            MakeEggify(data, FireGenerator.screenWidth, FireGenerator.screenHeight, eggifyBmp);
            eggifyImage.Source = eggifyBmp;
        }

        void MakeEggify(byte[] data, int w, int h, WriteableBitmap bmp)
        {
            byte r, g, b;

            var buffer = new byte[bmp.PixelWidth*bmp.PixelHeight*3];

            void GetPixel(int i, int j)
            {
                var index = (i+j*w)*3;
                r = data[index++];
                g = data[index++];
                b = data[index  ];
            }

            void SetPixel(int i, int j)
            {
                var index = (i + j * bmp.PixelWidth) * 3;
                buffer[index++] = r;
                buffer[index++] = g;
                buffer[index  ] = b;
            }

            // skip bottom pixels, double rest up
            for (var arm = 0; arm < 6; ++arm)
            {
                for (var y = 0; y < 150; y+=3)
                for (var x = 0; x < 3; ++x)
                {
                    GetPixel(10 * arm + x, h - 1 - 1 - y/3);
                    SetPixel(10 * arm + x, 150 - y);
                    SetPixel(10 * arm + x, 150 - y - 1);
                    SetPixel(10 * arm + x, 150 - y - 2);
                }
            }

            bmp.WritePixels(new Int32Rect(0,0,bmp.PixelWidth,bmp.PixelHeight),buffer,bmp.PixelWidth*3,0);

        }

        FireGenerator fire = new FireGenerator(); 

    }
}
