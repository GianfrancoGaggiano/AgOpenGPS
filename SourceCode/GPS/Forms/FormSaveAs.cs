﻿using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormSaveAs : Form
    {
        //class variables
        private readonly FormGPS mf;

        public FormSaveAs(Form _callingForm)
        {
            //get copy of the calling main form
            Owner = mf = _callingForm as FormGPS;

            InitializeComponent();

            label1.Text = String.Get("gsEnterFieldName");
            label2.Text = String.Get("gsDateWillBeAdded");
            label3.Text = String.Get("gsBasedOnField");
            label4.Text = String.Get("gsEnterTask");
            label5.Text = String.Get("gsEnterVehicleUsed");

            chkHeadland.Text = String.Get("gsHeadland");
            chkFlags.Text = String.Get("gsFlags");
            chkGuidanceLines.Text = String.Get("gsGuidance");
            chkApplied.Text = String.Get("gsMapping");

            this.Text = String.Get("gsSaveAs");
            lblTemplateChosen.Text = String.Get("gsNoneUsed");
        }

        private void FormSaveAs_Load(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            lblTemplateChosen.Text = Properties.Settings.Default.setF_CurrentDir;
            //tboxVehicle.Text = mf.vehicleFileName + " " + mf.toolFileName;
            lblFilename.Text = "";
        }

        private void TboxFieldName_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, Glm.fileRegex, "");
            textboxSender.SelectionStart = cursorPosition;

            if (string.IsNullOrEmpty(tboxFieldName.Text.Trim()))
            {
                btnSave.Enabled = false;
            }
            else
            {
                btnSave.Enabled = true;
            }

            lblFilename.Text = tboxFieldName.Text.Trim() + " " + tboxTask.Text.Trim()
                + " " + tboxVehicle.Text.Trim() + " " + DateTime.Now.ToString("yyyy.MMM.dd HH_mm", CultureInfo.InvariantCulture);
        }

        private void TboxTask_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, Glm.fileRegex, "");
            textboxSender.SelectionStart = cursorPosition;

            lblFilename.Text = tboxFieldName.Text.Trim() + " " + tboxTask.Text.Trim()
                + " " + tboxVehicle.Text.Trim() + " " + DateTime.Now.ToString("yyyy.MMM.dd HH_mm", CultureInfo.InvariantCulture);
        }

        private void TboxVehicle_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, Glm.fileRegex, "");
            textboxSender.SelectionStart = cursorPosition;

            lblFilename.Text = tboxFieldName.Text.Trim() + " " + tboxTask.Text.Trim()
                + " " + tboxVehicle.Text.Trim() + " " + DateTime.Now.ToString("yyyy.MMM.dd HH_mm", CultureInfo.InvariantCulture);
        }

        private void BtnSerialCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            //fill something in
            if (string.IsNullOrEmpty(tboxFieldName.Text.Trim()))
            {
                Close();
                return;
            }

            //append date time to name

            mf.currentFieldDirectory = tboxFieldName.Text.Trim() + " ";

            //task
            if (!string.IsNullOrEmpty(tboxTask.Text.Trim())) mf.currentFieldDirectory += tboxTask.Text.Trim() + " ";

            //vehicle
            if (!string.IsNullOrEmpty(tboxVehicle.Text.Trim())) mf.currentFieldDirectory += tboxVehicle.Text.Trim() + " ";

            //date
            mf.currentFieldDirectory += string.Format("{0}", DateTime.Now.ToString("yyyy.MMM.dd HH_mm", CultureInfo.InvariantCulture));

            //get the directory and make sure it exists, create if not
            string dirNewField = mf.fieldsDirectory + mf.currentFieldDirectory + "\\";


            // create from template
            string directoryName = Path.GetDirectoryName(dirNewField);

            if ((!string.IsNullOrEmpty(directoryName)) && (Directory.Exists(directoryName)))
            {
                MessageBox.Show(String.Get("gsChooseADifferentName"), String.Get("gsDirectoryExists"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            else
            {
                //create the new directory
                if ((!string.IsNullOrEmpty(directoryName)) && (!Directory.Exists(directoryName)))
                { Directory.CreateDirectory(directoryName); }
            }

            string line;
            string offsets, convergence, startFix;

            using (StreamReader reader = new StreamReader(mf.fieldsDirectory + lblTemplateChosen.Text + "\\Field.txt"))
            {
                try
                {
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    line = reader.ReadLine();

                    //read the Offsets  - all we really need from template field file
                    offsets = reader.ReadLine();

                    line = reader.ReadLine();
                    convergence = reader.ReadLine();

                    line = reader.ReadLine();
                    startFix = reader.ReadLine();
                }
                catch (Exception ex)
                {
                    mf.WriteErrorLog("While Opening Field" + ex);

                        mf.TimedMessageBox(2000, String.Get("gsFieldFileIsCorrupt"), String.Get("gsChooseADifferentField"));
                        mf.JobClose();
                        return;
                    }

                const string myFileName = "Field.txt";

                using (StreamWriter writer = new StreamWriter(dirNewField + myFileName))
                {
                    //Write out the date
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MMMM-dd hh:mm:ss tt", CultureInfo.InvariantCulture));

                    writer.WriteLine("$FieldDir");
                    writer.WriteLine(mf.currentFieldDirectory.ToString(CultureInfo.InvariantCulture));

                    //write out the easting and northing Offsets
                    writer.WriteLine("$Offsets");
                    writer.WriteLine(offsets);

                    writer.WriteLine("$Convergence");
                    writer.WriteLine(convergence);

                    writer.WriteLine("StartFix");
                    writer.WriteLine(startFix);
                }

                //create txt file copies
                string templateDirectoryName = (mf.fieldsDirectory + lblTemplateChosen.Text);
                string fileToCopy = "";
                string destinationDirectory = "";

                if (chkApplied.Checked)
                {
                     fileToCopy = templateDirectoryName + "\\Contour.txt";
                     destinationDirectory = directoryName + "\\Contour.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);

                     fileToCopy = templateDirectoryName + "\\Sections.txt";
                     destinationDirectory = directoryName + "\\Sections.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }

                else
                {
                    //create blank Contour and Section files
                    mf.FileCreateSections();
                    mf.FileCreateContour();
                    //mf.FileCreateElevation();
                }

                 fileToCopy = templateDirectoryName + "\\Boundary.txt";
                 destinationDirectory = directoryName + "\\Boundary.txt";
                if (File.Exists(fileToCopy))
                    File.Copy(fileToCopy, destinationDirectory);

                if (chkFlags.Checked)
                {
                    fileToCopy = templateDirectoryName + "\\Flags.txt";
                    destinationDirectory = directoryName + "\\Flags.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }
                else
                {
                    mf.FileSaveFlags();
                }

                if (chkGuidanceLines.Checked)
                {
                    fileToCopy = templateDirectoryName + "\\ABLines.txt";
                    destinationDirectory = directoryName + "\\ABLines.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);

                    fileToCopy = templateDirectoryName + "\\RecPath.txt";
                    destinationDirectory = directoryName + "\\RecPath.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);

                    fileToCopy = templateDirectoryName + "\\CurveLines.txt";
                    destinationDirectory = directoryName + "\\CurveLines.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }
                else
                {
                    mf.FileSaveABLines();
                    mf.FileSaveCurveLines();
                    mf.FileSaveRecPath();                    
                }

                if (chkHeadland.Checked)
                {
                    fileToCopy = templateDirectoryName + "\\Headland.txt";
                    destinationDirectory = directoryName + "\\Headland.txt";
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }
                else
                    mf.FileSaveHeadland();

                //fileToCopy = templateDirectoryName + "\\Elevation.txt";
                //destinationDirectory = directoryName + "\\Elevation.txt";
                //if (File.Exists(fileToCopy))
                //    File.Copy(fileToCopy, destinationDirectory);

                //now open the newly cloned field
                mf.FileOpenField(dirNewField + myFileName);
                mf.Text = "AgOpenGPS - " + mf.currentFieldDirectory;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void TboxFieldName_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void TboxTask_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void TboxVehicle_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }
    }
}
