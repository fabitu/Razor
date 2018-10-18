﻿namespace Assistant.UI
{
    partial class ContainerLabels
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cancelContainerLabels = new System.Windows.Forms.Button();
            this.saveContainerLabels = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.containerLabelFormat = new System.Windows.Forms.TextBox();
            this.removeOverheadMessage = new System.Windows.Forms.Button();
            this.addContainLabel = new System.Windows.Forms.Button();
            this.containerView = new System.Windows.Forms.ListView();
            this.containerId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.containerType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.containerLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.setExHue = new System.Windows.Forms.Button();
            this.lblContainerHue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancelContainerLabels
            // 
            this.cancelContainerLabels.Location = new System.Drawing.Point(334, 238);
            this.cancelContainerLabels.Name = "cancelContainerLabels";
            this.cancelContainerLabels.Size = new System.Drawing.Size(54, 28);
            this.cancelContainerLabels.TabIndex = 20;
            this.cancelContainerLabels.Text = "Cancel";
            this.cancelContainerLabels.UseVisualStyleBackColor = true;
            this.cancelContainerLabels.Click += new System.EventHandler(this.cancelOverheadMessages_Click);
            // 
            // saveContainerLabels
            // 
            this.saveContainerLabels.Location = new System.Drawing.Point(394, 238);
            this.saveContainerLabels.Name = "saveContainerLabels";
            this.saveContainerLabels.Size = new System.Drawing.Size(54, 28);
            this.saveContainerLabels.TabIndex = 19;
            this.saveContainerLabels.Text = "OK";
            this.saveContainerLabels.UseVisualStyleBackColor = true;
            this.saveContainerLabels.Click += new System.EventHandler(this.saveContainerLabels_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 214);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 15);
            this.label2.TabIndex = 30;
            this.label2.Text = "Label Format:";
            // 
            // containerLabelFormat
            // 
            this.containerLabelFormat.Location = new System.Drawing.Point(97, 211);
            this.containerLabelFormat.Name = "containerLabelFormat";
            this.containerLabelFormat.Size = new System.Drawing.Size(105, 23);
            this.containerLabelFormat.TabIndex = 29;
            this.containerLabelFormat.Text = "[{label}]";
            this.containerLabelFormat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // removeOverheadMessage
            // 
            this.removeOverheadMessage.Location = new System.Drawing.Point(155, 176);
            this.removeOverheadMessage.Name = "removeOverheadMessage";
            this.removeOverheadMessage.Size = new System.Drawing.Size(121, 28);
            this.removeOverheadMessage.TabIndex = 28;
            this.removeOverheadMessage.Text = "Remove Selected";
            this.removeOverheadMessage.UseVisualStyleBackColor = true;
            this.removeOverheadMessage.Click += new System.EventHandler(this.removeOverheadMessage_Click);
            // 
            // addContainLabel
            // 
            this.addContainLabel.Location = new System.Drawing.Point(15, 176);
            this.addContainLabel.Name = "addContainLabel";
            this.addContainLabel.Size = new System.Drawing.Size(134, 28);
            this.addContainLabel.TabIndex = 27;
            this.addContainLabel.Text = "Add Container Label";
            this.addContainLabel.UseVisualStyleBackColor = true;
            this.addContainLabel.Click += new System.EventHandler(this.addContainLabel_Click);
            // 
            // containerView
            // 
            this.containerView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.containerId,
            this.containerType,
            this.containerLabel});
            this.containerView.Location = new System.Drawing.Point(15, 12);
            this.containerView.Name = "containerView";
            this.containerView.Size = new System.Drawing.Size(433, 158);
            this.containerView.TabIndex = 22;
            this.containerView.UseCompatibleStateImageBehavior = false;
            this.containerView.View = System.Windows.Forms.View.Details;
            // 
            // containerId
            // 
            this.containerId.Text = "ID";
            this.containerId.Width = 88;
            // 
            // containerType
            // 
            this.containerType.Text = "Type";
            this.containerType.Width = 100;
            // 
            // containerLabel
            // 
            this.containerLabel.Text = "Label";
            this.containerLabel.Width = 180;
            // 
            // setExHue
            // 
            this.setExHue.Location = new System.Drawing.Point(155, 240);
            this.setExHue.Name = "setExHue";
            this.setExHue.Size = new System.Drawing.Size(47, 26);
            this.setExHue.TabIndex = 39;
            this.setExHue.Text = "Set";
            this.setExHue.Click += new System.EventHandler(this.setExHue_Click);
            // 
            // lblContainerHue
            // 
            this.lblContainerHue.Location = new System.Drawing.Point(12, 240);
            this.lblContainerHue.Name = "lblContainerHue";
            this.lblContainerHue.Size = new System.Drawing.Size(190, 26);
            this.lblContainerHue.TabIndex = 38;
            this.lblContainerHue.Text = "Container Label Hue:";
            this.lblContainerHue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ContainerLabels
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 275);
            this.Controls.Add(this.setExHue);
            this.Controls.Add(this.lblContainerHue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.containerLabelFormat);
            this.Controls.Add(this.removeOverheadMessage);
            this.Controls.Add(this.addContainLabel);
            this.Controls.Add(this.containerView);
            this.Controls.Add(this.cancelContainerLabels);
            this.Controls.Add(this.saveContainerLabels);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ContainerLabels";
            this.Text = "Container Labels";
            this.Load += new System.EventHandler(this.ContainerLabels_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cancelContainerLabels;
        private System.Windows.Forms.Button saveContainerLabels;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox containerLabelFormat;
        private System.Windows.Forms.Button removeOverheadMessage;
        private System.Windows.Forms.Button addContainLabel;
        private System.Windows.Forms.ListView containerView;
        private System.Windows.Forms.ColumnHeader containerId;
        private System.Windows.Forms.ColumnHeader containerType;
        private System.Windows.Forms.ColumnHeader containerLabel;
        private System.Windows.Forms.Button setExHue;
        private System.Windows.Forms.Label lblContainerHue;
    }
}