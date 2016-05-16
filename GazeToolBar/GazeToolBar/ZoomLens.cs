﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GazeToolBar
{
    public partial class ZoomLens : Form
    {

        const int LENSSIZE = 600;// how big the actual lens is
        const int ZOOMLEVEL = 30;// this is controls how far the lens will zoom in
        int x, y;
        Graphics graphics;
        Zoomer zoomer;
        Bitmap bmpScreenshot;
        delegate void SetFormDelegate(int x, int y);
        FixationDetection fixationWorker;
        public ZoomLens(FixationDetection fixationWorker)
        {
            InitializeComponent();
            this.fixationWorker = fixationWorker;
            //this.x = x;
            //this.y = y;
            this.Width = LENSSIZE;//setting the lens size
            this.Height = LENSSIZE;

            bmpScreenshot = new Bitmap(this.Width, this.Height);//set bitmap to same size as the lens
            graphics = this.CreateGraphics();
            graphics = Graphics.FromImage(bmpScreenshot);



            pictureBox1.Width = this.Width;//set picturebox to same size as form
            pictureBox1.Height = this.Height;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;//make the image stretch to the bounds of the picturebox

            this.FormBorderStyle = FormBorderStyle.None;
            zoomer = new Zoomer(graphics);

        }
        public void CreateZoomLens(int x, int y)
        {
            if (this.InvokeRequired)
            {
                SetFormDelegate sfd = new SetFormDelegate(SetForm);
                this.Invoke(sfd, new Object[] { x, y });
            }
            //perform click here @ the center of the form
        }
        public void SetForm(int x, int y)
        {
            this.DesktopLocation = new Point(x - (this.Width / 2), y - (this.Height / 2));//set the position of the lens and offset it by it's size /2 to center the lens on the location of the current event
            this.Show();//make lens visible
            Point lensPoint = new Point();
            Point empty = new Point(0, 0);
            lensPoint.X = x - (this.Width / 2);//this sets the position on the screen which is being zoomed in. 
            lensPoint.Y = y - (this.Height / 2);
            graphics.CopyFromScreen(lensPoint.X, lensPoint.Y, empty.X, empty.Y, this.Size, CopyPixelOperation.SourceCopy);

            bmpScreenshot = zoomer.Zoom(bmpScreenshot);
            pictureBox1.Image = bmpScreenshot;
            Application.DoEvents();
            //somehow check for when another fixation happens on the form?
            System.Threading.Thread.Sleep(2000);

            int xMiddleofForm = this.Top + this.Height / 2;
            int yMiddleofForm = this.Left + this.Width / 2;
            VirtualMouse.LeftMouseClick(xMiddleofForm, yMiddleofForm);
            //fixationWorker.SetupSelectedFixationAction(VirtualMouse.LeftMouseClick);//this needs to change based on what event is being called. also the x,y needs to be 

            this.Dispose();
        }
    }
}
