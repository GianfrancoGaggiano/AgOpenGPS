﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormToolPicker : Form
    {
        //class variables
        private readonly FormGPS mf;

        public FormToolPicker(Form callingForm)
        {
            //get copy of the calling main form
            Owner = mf = callingForm as FormGPS;
            InitializeComponent();

            this.Text = String.Get("gsLoadTool");
        }

        private void FormFlags_Load(object sender, EventArgs e)
        {
            lblLast.Text = String.Get("gsCurrent") + mf.toolFileName;

            DirectoryInfo dinfo = new DirectoryInfo(mf.toolsDirectory);
            FileInfo[] Files = dinfo.GetFiles("*.txt");
            if (Files.Length == 0)
            {
                Close();
                mf.TimedMessageBox(2000, String.Get("gsNoToolSaved"), String.Get("gsSaveAToolFirst"));

            }

            foreach (FileInfo file in Files)
            {
                cboxTool.Items.Add(Path.GetFileNameWithoutExtension(file.Name));
            }
        }

        private void CboxVeh_SelectedIndexChanged(object sender, EventArgs e)
        {
            mf.FileOpenTool(mf.toolsDirectory + cboxTool.SelectedItem.ToString() + ".txt");
            Close();
        }
    }
}