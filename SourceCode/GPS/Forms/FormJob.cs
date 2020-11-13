﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormJob : Form
    {
        //class variables
        private readonly FormGPS mf;

        public FormJob(Form callingForm)
        {
            //get copy of the calling main form
            Owner = mf = callingForm as FormGPS;

            InitializeComponent();

            btnJobOpen.Text = String.Get("gsOpen");
            btnJobNew.Text = String.Get("gsNew");
            btnJobResume.Text = String.Get("gsResume");

            label1.Text = String.Get("gsLastFieldUsed");

            this.Text = String.Get("gsStartNewField");
        }

        private void BtnJobNew_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void BtnJobResume_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FormJob_Load(object sender, EventArgs e)
        {
            //check if directory and file exists, maybe was deleted etc
            if (string.IsNullOrEmpty(mf.currentFieldDirectory)) btnJobResume.Enabled = false;
            string directoryName = mf.fieldsDirectory + mf.currentFieldDirectory + "\\";

            string fileAndDirectory = directoryName + "Field.txt";

            if (!File.Exists(fileAndDirectory))
            {
                textBox1.Text = "";
                btnJobResume.Enabled = false;
                mf.currentFieldDirectory = "";
                Properties.Settings.Default.setF_CurrentDir = "";
                Properties.Settings.Default.Save();
            }
            else
            {
                textBox1.Text = mf.currentFieldDirectory;
            }
        }

        private void BtnJobOpen_Click(object sender, EventArgs e)
        {
            mf.filePickerFileAndDirectory = "";

            using (var form = new FormFilePicker(mf))
            {
                var result = form.ShowDialog(mf);

                //returns full field.txt file dir name
                if (result == DialogResult.Yes)
                {
                    DialogResult = result;
                    Close();
                }
            }
        }

        private void BtnInField_Click(object sender, EventArgs e)
        {
            string infieldList = "";
            int numFields = 0;

            string[] dirs = Directory.GetDirectories(mf.fieldsDirectory);

            foreach (string dir in dirs)
            {
                double northingOffset = 0;
                double eastingOffset = 0;
                
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
                            for (int i = 0; i < 4; i++)
                            {
                                line = reader.ReadLine();
                            }

                            //start positions
                            if (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                string[] offs = line.Split(',');

                                eastingOffset = (double.Parse(offs[0], CultureInfo.InvariantCulture));
                                northingOffset = (double.Parse(offs[1], CultureInfo.InvariantCulture));
                            }
                        }
                        catch (Exception)
                        {
                            mf.TimedMessageBox(2000, String.Get("gsFieldFileIsCorrupt"), String.Get("gsChooseADifferentField"));
                        }
                    }

                    //grab the boundary
                    filename = dir + "\\Boundary.txt";

                    if (!File.Exists(filename))
                        filename = dir + "\\Boundary.Tmp";

                    if (File.Exists(filename))
                    {
                        List<Vec3> pointList = new List<Vec3>();

                        using (StreamReader reader = new StreamReader(filename))
                        {
                            try
                            {

                                //read header
                                line = reader.ReadLine();//Boundary

                                if (!reader.EndOfStream) //empty boundary field
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
                                    Vec2[] linePoints = new Vec2[numPoints];

                                    if (numPoints > 0)
                                    {
                                        //load the line
                                        for (int i = 0; i < numPoints; i++)
                                        {
                                            line = reader.ReadLine();
                                            string[] words = line.Split(',');
                                            Vec2 vecPt = new Vec2(
                                            double.Parse(words[1], CultureInfo.InvariantCulture) + northingOffset,
                                                double.Parse(words[0], CultureInfo.InvariantCulture) + eastingOffset);

                                            linePoints[i] = vecPt;
                                        }

                                        int j = linePoints.Length - 1;
                                        bool oddNodes = false;
                                        double x = mf.pn.actualEasting;
                                        double y = mf.pn.actualNorthing;

                                        for (int i = 0; i < linePoints.Length; i++)
                                        {
                                            if ((linePoints[i].Northing < y && linePoints[j].Northing >= y
                                            || linePoints[j].Northing < y && linePoints[i].Northing >= y)
                                            && (linePoints[i].Easting <= x || linePoints[j].Easting <= x))
                                            {
                                                oddNodes ^= (linePoints[i].Easting + (y - linePoints[i].Northing) /
                                                (linePoints[j].Northing - linePoints[i].Northing) * (linePoints[j].Easting - linePoints[i].Easting) < x);
                                            }
                                            j = i;
                                        }

                                        if (oddNodes)
                                        {
                                            numFields++;
                                            if (string.IsNullOrEmpty(infieldList))
                                                infieldList += Path.GetFileName(dir);
                                            else
                                                infieldList += "," + Path.GetFileName(dir);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(infieldList))
            {
                mf.filePickerFileAndDirectory = "";

                if (numFields > 1)
                {
                    using (var form = new FormDrivePicker(mf, this, infieldList))
                    {
                        var result = form.ShowDialog(this);
                        if (result == DialogResult.Yes)
                        {
                            DialogResult = DialogResult.Yes;
                            Close();
                        }
                    }
                }
                else // 1 field found
                {
                    mf.filePickerFileAndDirectory = mf.fieldsDirectory + infieldList + "\\Field.txt";
                    DialogResult = DialogResult.Yes;
                    Close();
                }
            }
            else //no fields found
            {
                mf.TimedMessageBox(2000, String.Get("gsNoFieldsFound"), String.Get("gsFieldNotOpen"));
            }
        }
    }
}