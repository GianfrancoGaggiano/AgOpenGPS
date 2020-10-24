﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormFilePicker : Form
    {
        private readonly FormGPS mf;

        private int order;

        public List<string> FileList { get; set; } = new List<string>();

        public FormFilePicker(Form callingForm)
        {
            //get copy of the calling main form
            Owner = mf = callingForm as FormGPS;

            InitializeComponent();
            btnByDistance.Text = String.Get("gsSort");
            btnOpenExistingLv.Text = String.Get("gsUseSelected");
        }
        private void FormFilePicker_Load(object sender, EventArgs e)
        {
            order = 0;
            timer1.Enabled = true;
            ListViewItem itm;

            string[] dirs = Directory.GetDirectories(mf.fieldsDirectory);

            FileList.Clear();

            foreach (string dir in dirs)
            {
                double latStart = 0;
                double lonStart = 0;
                double distance = 0;
                string fieldDirectory = Path.GetFileName(dir);
                string filename = dir + "\\Field.txt";
                string line;

                //make sure directory has a field.txt in it
                if (File.Exists(filename))
                {
                    using (StreamReader reader = new StreamReader(filename))
                    {
                        try
                        {
                            //Date time line
                            for (int i = 0; i < 8; i++)
                            {
                                line = reader.ReadLine();
                            }

                            //start positions
                            if (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                string[] offs = line.Split(',');

                                latStart = (double.Parse(offs[0], CultureInfo.InvariantCulture));
                                lonStart = (double.Parse(offs[1], CultureInfo.InvariantCulture));


                                distance = Math.Pow((latStart - mf.pn.latitude), 2) + Math.Pow((lonStart - mf.pn.longitude), 2);
                                distance = Math.Sqrt(distance);
                                distance *= 100;

                                FileList.Add(fieldDirectory);
                                FileList.Add(distance.ToString("#####.##").PadLeft(10));
                            }
                            else
                            {
                                MessageBox.Show(fieldDirectory + " is Damaged, Please Delete This Field", String.Get("gsFileError"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                                FileList.Add(fieldDirectory);
                                FileList.Add("Error");
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(fieldDirectory + " is Damaged, Please Delete, Field.txt is Broken", String.Get("gsFileError"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                            FileList.Add(fieldDirectory);
                            FileList.Add("Error");
                        }
                    }
                }

                //grab the boundary area
                filename = dir + "\\Boundary.txt";
                if (File.Exists(filename))
                {
                    List<Vec3> pointList = new List<Vec3>();
                    double area = 0;

                    using (StreamReader reader = new StreamReader(filename))
                    {
                        try
                        {
                            //read header
                            line = reader.ReadLine();//Boundary

                            if (!reader.EndOfStream)
                            {
                                //True or False OR points from older boundary files
                                line = reader.ReadLine();

                                //Check for older boundary files, then above line string is num of points
                                if (line == "True" || line == "False")
                                {
                                    line = reader.ReadLine(); //number of points
                                }

                                //Check for latest boundary files, then above line string is num of points
                                if (line == "True" || line == "False")
                                {
                                    line = reader.ReadLine(); //number of points
                                }

                                int numPoints = int.Parse(line);

                                if (numPoints > 0)
                                {
                                    //load the line
                                    for (int i = 0; i < numPoints; i++)
                                    {
                                        line = reader.ReadLine();
                                        string[] words = line.Split(',');
                                        Vec3 vecPt = new Vec3(
                                        double.Parse(words[1], CultureInfo.InvariantCulture),
                                            double.Parse(words[0], CultureInfo.InvariantCulture),
                                            double.Parse(words[2], CultureInfo.InvariantCulture));

                                        pointList.Add(vecPt);
                                    }

                                    int ptCount = pointList.Count;
                                    if (ptCount > 5)
                                    {
                                        area = 0;         // Accumulates area in the loop
                                        int j = ptCount - 1;  // The last vertex is the 'previous' one to the first

                                        for (int i = 0; i < ptCount; j = i++)
                                        {
                                            area += (pointList[j].Easting + pointList[i].Easting) * (pointList[j].Northing - pointList[i].Northing);
                                        }
                                        if (mf.isMetric)
                                        {
                                            area = (Math.Abs(area / 2)) * 0.0001;
                                        }
                                        else
                                        {
                                            area = (Math.Abs(area / 2)) * 0.00024711;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            area = 0;
                        }
                    }
                    if (area ==0) FileList.Add("No Bndry");
                    else FileList.Add(area.ToString("####.##").PadLeft(10));
                }

                else
                {
                    FileList.Add("Error");
                    MessageBox.Show(fieldDirectory + " is Damaged, Missing Boundary.Txt " +
                        "               \r\n Delete Field or Fix ", String.Get("gsFileError"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                filename = dir + "\\Field.txt";
            }

            for (int i = 0; i < FileList.Count-2; i += 3)
            {
                string[] fieldNames = { FileList[i], FileList[i + 1], FileList[i+2] };
                itm = new ListViewItem(fieldNames);
                lvLines.Items.Add(itm);
            }

            //string fieldName = Path.GetDirectoryName(dir).ToString(CultureInfo.InvariantCulture);

            if (lvLines.Items.Count > 0)
            {
                this.chName.Text = "Field Name";
                this.chName.Width = 680;

                this.chDistance.Text = "Distance";
                this.chDistance.Width = 140;

                this.chArea.Text = "Area";
                this.chArea.Width = 140;
            }
        }

        private void BtnByDistance_Click(object sender, EventArgs e)
        {
            ListViewItem itm;

            lvLines.Items.Clear();
            order += 1;
            if (order == 3) order = 0;


            for (int i = 0; i < FileList.Count-2; i += 3)
            {
                if (order == 0)
                {
                    string[] fieldNames = { FileList[i], FileList[i + 1], FileList[i + 2] };
                    itm = new ListViewItem(fieldNames);
                }
                else if (order == 1)
                {
                    string[] fieldNames = { FileList[i + 1], FileList[i], FileList[i + 2] };
                    itm = new ListViewItem(fieldNames);
                }
                else
                {
                    string[] fieldNames = { FileList[i + 2], FileList[i], FileList[i + 1] };
                    itm = new ListViewItem(fieldNames);
                }

                lvLines.Items.Add(itm);
            }

            if (lvLines.Items.Count > 0)
            {
                if (order == 0)
                {
                    this.chName.Text = "Field Name";
                    this.chName.Width = 680;

                    this.chDistance.Text = "Distance";
                    this.chDistance.Width = 140;

                    this.chArea.Text = "Area";
                    this.chArea.Width = 140;
                }
                else if (order == 1)
                {
                    this.chName.Text = "Distance";
                    this.chName.Width = 140;

                    this.chDistance.Text = "Field Name";
                    this.chDistance.Width = 680;

                    this.chArea.Text = "Area";
                    this.chArea.Width = 140;
                }

                else
                {
                    this.chName.Text = "Area";
                    this.chName.Width = 140;

                    this.chDistance.Text = "Field Name";
                    this.chDistance.Width = 680;

                    this.chArea.Text = "Distance";
                    this.chArea.Width = 140;
                }


            }
        }

        private void BtnOpenExistingLv_Click(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                if (lvLines.SelectedItems[0].SubItems[0].Text == "Error" ||
                    lvLines.SelectedItems[0].SubItems[1].Text == "Error" ||
                    lvLines.SelectedItems[0].SubItems[2].Text == "Error")
                {
                    MessageBox.Show("This Field is Damaged, Please Delete \r\n ALREADY TOLD YOU THAT :)", String.Get("gsFileError"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else
                {
                    if (order == 0) mf.filePickerFileAndDirectory = (mf.fieldsDirectory + lvLines.SelectedItems[0].SubItems[0].Text + "\\Field.txt");
                    else mf.filePickerFileAndDirectory = (mf.fieldsDirectory + lvLines.SelectedItems[0].SubItems[1].Text + "\\Field.txt");
                    Close();
                }
            }
        }

        private void BtnDeleteAB_Click(object sender, EventArgs e)
        {
            mf.filePickerFileAndDirectory = "";
        }

    }
}