using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;

namespace NES
{
    public class GameRender
    {
        NES NES;
        Bitmap textureScreenBuffer;

        // Setup background color to use with Alpha
        Color colorBG;

        Graphics graphics;
        
        public void InitializeGame()
        {
            textureScreenBuffer = new Bitmap(256, 240);

            graphics = Graphics.FromImage(textureScreenBuffer);

            // Set the initial background color
            colorBG = Color.DarkMagenta;
        }

        public void WriteBitmap(byte[] byteToWrite, Color XColor)
        {
            graphics.Clear(XColor);

            int w = 0;
            int h = 0;

            for(int i = 0; i < byteToWrite.Length; i+=4) 
            {
                Color color = Color.FromArgb(byteToWrite[i + 3], byteToWrite[i+2], byteToWrite[i + 1], byteToWrite[i + 0]);
                if (color.A != 0)
                {
                    textureScreenBuffer.SetPixel(w, h, color);
                }
                //
                w++;
                //256*240
                if(w == 256) 
                {
                    w = 0;
                    h++;
                }
            }

            Program.NES.Present(textureScreenBuffer);
        }

        public GameRender(NES formObject)
        {
            NES = formObject;
            InitializeGame();
        }

        public void DrawPoint(Graphics graphics,int X,int Y,Color color) 
        {
            graphics.FillRectangle(new SolidBrush(color), X, Y, 1, 1);
        }
    }
}
