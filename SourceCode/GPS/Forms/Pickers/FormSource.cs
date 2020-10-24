﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormSource : Form
    {
        private readonly FormNtrip nt;

        private readonly List<string> dataList = new List<string>();
        private readonly double lat,lon;
        readonly string site;

        public FormSource(Form callingForm, List<string> _dataList, double _lat, double _lon, string syte)
        {
            Owner = nt = callingForm as FormNtrip;
            InitializeComponent();
            dataList = _dataList;
            lat = _lat;
            lon = _lon;
            site = syte;
        }

        private void FormSource_Load(object sender, EventArgs e)
        {
            double minDist = 999999999;
            ListViewItem itm;
            if (dataList.Count > 0)
            {
                double temp;

                for (int i = 0; i < dataList.Count; i++)
                {
                    string[] data = dataList[i].Split(',');
                    double.TryParse(data[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double cLat);
                    double.TryParse(data[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double cLon);

                    if (cLat == 0 || cLon == 0)
                    {
                        temp = 9999999.9;
                    }
                    else
                    {
                        temp = GetDistance(cLon, cLat, lon, lat);
                        temp *= .001;
                    }
                    if (temp < minDist)
                    {
                        minDist = temp;
                    }                    

                    //load up the listview
                    string[] fieldNames = { temp.ToString("#######").PadLeft(10),data[0].Trim(), data[1].Trim(), 
                                                    data[2].Trim(), data[3].Trim(), data[4].Trim() };
                    itm = new ListViewItem(fieldNames);
                    lvLines.Items.Add(itm);
                }

                //string [] dataM = dataList[place].Split(',');
                //tboxMount.Text = dataM[0];
            }
            this.chName.Width = 250;
        }

        private void BtnSite_Click(object sender, EventArgs e)
        {
             Process.Start(site);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnUseMount_Click(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                nt.tboxMount.Text = (lvLines.SelectedItems[0].SubItems[1].Text);
                
                Close();
            }

        }

        private void LvLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                nt.tboxMount.Text = (lvLines.SelectedItems[0].SubItems[1].Text);
                tboxMount.Text = (lvLines.SelectedItems[0].SubItems[1].Text);
            }
        }

        public double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = Glm.ToRadians(latitude);
            var num1 = Glm.ToRadians(longitude);
            var d2 = Glm.ToRadians(otherLatitude);
            var num2 = Glm.ToRadians(otherLongitude) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
