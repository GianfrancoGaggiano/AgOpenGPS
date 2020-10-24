﻿using System;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormShiftPos : Form
    {
        //class variables
        private readonly FormGPS mf;

        public FormShiftPos(Form callingForm)
        {
            //get copy of the calling main form
            Owner = mf = callingForm as FormGPS;
            InitializeComponent();

            label27.Text = String.Get("gsNorth");
            label2.Text = String.Get("gsWest");
            label3.Text = String.Get("gsEast");
            label4.Text = String.Get("gsSouth");
            this.Text = String.Get("gsShiftGPSPosition");
        }

        private void FormShiftPos_Load(object sender, EventArgs e)
        {
            nudEast.Value = (decimal)mf.pn.fixOffset.Easting * 100;
            nudNorth.Value = (decimal)mf.pn.fixOffset.Northing * 100;
        }

        private void BtnNorth_MouseDown(object sender, MouseEventArgs e)
        {
            nudNorth.UpButton();
            mf.pn.fixOffset.Northing = (double)nudNorth.Value / 100;
        }

        private void BtnSouth_MouseDown(object sender, MouseEventArgs e)
        {
            nudNorth.DownButton();
            mf.pn.fixOffset.Northing = (double)nudNorth.Value / 100;
        }

        private void BtnWest_MouseDown(object sender, MouseEventArgs e)
        {
            nudEast.DownButton();
            mf.pn.fixOffset.Easting = (double)nudEast.Value / 100;
        }

        private void BtnEast_MouseDown(object sender, MouseEventArgs e)
        {
            nudEast.UpButton();
            mf.pn.fixOffset.Easting = (double)nudEast.Value / 100;
        }

        private void NudNorth_ValueChanged(object sender, EventArgs e)
        {
            mf.pn.fixOffset.Northing = (double)nudNorth.Value / 100;
        }

        private void NudEast_ValueChanged(object sender, EventArgs e)
        {
            mf.pn.fixOffset.Easting = (double)nudEast.Value / 100;
        }

        private void BtnZero_Click(object sender, EventArgs e)
        {
            nudEast.Value = 0;
            nudNorth.Value = 0;
            mf.pn.fixOffset.Easting = 0;
            mf.pn.fixOffset.Northing = 0;
        }

        private void BntOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}