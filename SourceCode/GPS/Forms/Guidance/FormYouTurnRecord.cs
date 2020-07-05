﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormYouTurnRecord : Form
    {
        //properties
        private readonly FormGPS mf;

        public FormYouTurnRecord(Form callingForm)
        {
            mf = callingForm as FormGPS;
            InitializeComponent();
            btnStop.Text = gStr.gsDone;
            btnRecord.Text = gStr.gsRecord;
            label1.Text = gStr.gsTurnRIGHTwhilerecording;
            this.Text = gStr.gsYouTurnRecorder;
        }

        private void BtnRecord_Click(object sender, EventArgs e)
        {
            btnRecord.ForeColor = Color.Red;
            if (mf.yt.youFileList.Count > 0) mf.yt.youFileList.Clear();
            mf.yt.isRecordingCustomYouTurn = true;
            btnRecord.Enabled = false;
            btnStop.Enabled = true;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            //go make the list
            mf.yt.isRecordingCustomYouTurn = false;

            //first one is the reference the rest are subtracted from, remove it.
            mf.yt.youFileList.RemoveAt(0);

            int numShapePoints = mf.yt.youFileList.Count;
            int i;
            Vec2[] pt = new Vec2[numShapePoints];

            //put the driven list into an array
            for (i = 0; i < numShapePoints; i++)
            {
                pt[i].easting = mf.yt.youFileList[i].easting;
                pt[i].northing = mf.yt.youFileList[i].northing;
            }

            //empty out the youFileList
            mf.yt.youFileList.Clear();

            //rotate pattern to match AB Line heading
            for (i = 0; i < pt.Length; i++)
            {
                //since we want to unwind the heading, we go not negative for heading unlike GPS circle
                double xr, yr;
                xr = (Math.Cos(mf.ABLine.abHeading) * pt[i].easting) - (Math.Sin(mf.ABLine.abHeading) * pt[i].northing);
                yr = (Math.Sin(mf.ABLine.abHeading) * pt[i].easting) + (Math.Cos(mf.ABLine.abHeading) * pt[i].northing);

                //update the array
                pt[i].easting = xr;
                pt[i].northing = yr;
            }

            //scale the drawing to match exactly the ABLine width
            double adjustFactor = pt[pt.Length - 1].easting;
            adjustFactor = (mf.Tools[0].ToolWidth - mf.Tools[0].ToolOverlap + mf.Tools[0].ToolOffset) / adjustFactor;
            for (i = 0; i < pt.Length; i++)
            {
                pt[i].easting *= adjustFactor;
                pt[i].northing *= adjustFactor;
            }

            // 2nd pass scale it so coords are based on 10m
            //last point is the width
            adjustFactor = pt[pt.Length - 1].easting;
            adjustFactor = 10.0 / adjustFactor;
            for (i = 0; i < pt.Length; i++)
            {
                pt[i].easting *= adjustFactor;
                pt[i].northing *= adjustFactor;
                mf.yt.youFileList.Add(pt[i]);
            }

            //create the file.
            string dir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dir = System.IO.Path.GetDirectoryName(dir);
            dir += @"\Dependencies\YouTurnShapes\Custom.txt";
            using (StreamWriter writer = new StreamWriter(dir))
            {
                writer.WriteLine(pt.Length);
                for (i = 0; i < mf.yt.youFileList.Count; i++)
                    writer.WriteLine(mf.yt.youFileList[i].easting + "," + mf.yt.youFileList[i].northing);
            }

            Close();
        }

        private void FormYouTurnRecord_Load(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
        }
    }
}