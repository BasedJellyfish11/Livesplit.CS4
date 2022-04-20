﻿using System.ComponentModel;
 using System.Linq;
 using System.Windows.Forms;

 namespace Livesplit.CS4
{
    partial class Settings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SplitsCollection = new System.Windows.Forms.CheckedListBox();
            this.splitsGroupBox = new System.Windows.Forms.GroupBox();
            this.splitsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SplitsCollection
            // 
            this.SplitsCollection.CheckOnClick = true;
            this.SplitsCollection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitsCollection.FormattingEnabled = true;
            this.SplitsCollection.Location = new System.Drawing.Point(3, 16);
            this.SplitsCollection.Name = "SplitsCollection";
            this.SplitsCollection.ScrollAlwaysVisible = true;
            this.SplitsCollection.Size = new System.Drawing.Size(403, 527);
            this.SplitsCollection.TabIndex = 1;
            this.SplitsCollection.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.SplitsCollection_ItemCheckChanged);
            // 
            // splitsGroupBox
            // 
            this.splitsGroupBox.Controls.Add(this.SplitsCollection);
            this.splitsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.splitsGroupBox.Name = "splitsGroupBox";
            this.splitsGroupBox.Size = new System.Drawing.Size(409, 546);
            this.splitsGroupBox.TabIndex = 2;
            this.splitsGroupBox.TabStop = false;
            this.splitsGroupBox.Text = "Settings";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.splitsGroupBox);
            this.Name = "Settings";
            this.Size = new System.Drawing.Size(409, 546);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.splitsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.CheckedListBox SplitsCollection;

        #endregion

        private GroupBox splitsGroupBox;
    }
}