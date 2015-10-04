namespace SimpleDataAccessLayer.Common.wizard
{
    partial class ModelWizard
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
            this.Back = new System.Windows.Forms.Button();
            this.Next = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.tabContainer = new SimpleDataAccessLayer.Common.wizard.WizardTabControl();
            this.DesignerConnection = new System.Windows.Forms.TabPage();
            this.designerConnectionTab = new SimpleDataAccessLayer.Common.wizard.DesignerConnectionTab();
            this.Enums = new System.Windows.Forms.TabPage();
            this.enumsTab1 = new SimpleDataAccessLayer.Common.wizard.EnumsTab();
            this.Procedures = new System.Windows.Forms.TabPage();
            this.proceduresTab1 = new SimpleDataAccessLayer.Common.wizard.ProceduresTab();
            this.tabContainer.SuspendLayout();
            this.DesignerConnection.SuspendLayout();
            this.Enums.SuspendLayout();
            this.Procedures.SuspendLayout();
            this.SuspendLayout();
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(627, 322);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(75, 23);
            this.Back.TabIndex = 101;
            this.Back.Text = "Back";
            this.Back.UseVisualStyleBackColor = true;
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // Next
            // 
            this.Next.Location = new System.Drawing.Point(720, 322);
            this.Next.Name = "Next";
            this.Next.Size = new System.Drawing.Size(75, 23);
            this.Next.TabIndex = 102;
            this.Next.Text = "Next";
            this.Next.UseVisualStyleBackColor = true;
            this.Next.Click += new System.EventHandler(this.Next_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(828, 322);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 103;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(720, 322);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 102;
            this.OK.Text = "Finish";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // tabContainer
            // 
            this.tabContainer.Controls.Add(this.DesignerConnection);
            this.tabContainer.Controls.Add(this.Enums);
            this.tabContainer.Controls.Add(this.Procedures);
            this.tabContainer.Location = new System.Drawing.Point(12, 12);
            this.tabContainer.Name = "tabContainer";
            this.tabContainer.SelectedIndex = 0;
            this.tabContainer.Size = new System.Drawing.Size(895, 301);
            this.tabContainer.TabIndex = 0;
            this.tabContainer.SelectedIndexChanged += new System.EventHandler(this.VisibleTab_Changed);
            // 
            // DesignerConnection
            // 
            this.DesignerConnection.Controls.Add(this.designerConnectionTab);
            this.DesignerConnection.Location = new System.Drawing.Point(4, 22);
            this.DesignerConnection.Name = "DesignerConnection";
            this.DesignerConnection.Padding = new System.Windows.Forms.Padding(3);
            this.DesignerConnection.Size = new System.Drawing.Size(887, 275);
            this.DesignerConnection.TabIndex = 1;
            this.DesignerConnection.Text = "Designer Connection";
            this.DesignerConnection.UseVisualStyleBackColor = true;
            // 
            // designerConnectionTab
            // 
            this.designerConnectionTab.Location = new System.Drawing.Point(50, 38);
            this.designerConnectionTab.Name = "designerConnectionTab";
            this.designerConnectionTab.Size = new System.Drawing.Size(804, 180);
            this.designerConnectionTab.TabIndex = 0;
            // 
            // Enums
            // 
            this.Enums.Controls.Add(this.enumsTab1);
            this.Enums.Location = new System.Drawing.Point(4, 22);
            this.Enums.Name = "Enums";
            this.Enums.Size = new System.Drawing.Size(887, 275);
            this.Enums.TabIndex = 3;
            this.Enums.Text = "Enums";
            this.Enums.UseVisualStyleBackColor = true;
            // 
            // enumsTab1
            // 
            this.enumsTab1.Location = new System.Drawing.Point(4, 4);
            this.enumsTab1.Name = "enumsTab1";
            this.enumsTab1.Size = new System.Drawing.Size(872, 265);
            this.enumsTab1.TabIndex = 0;
            // 
            // Procedures
            // 
            this.Procedures.Controls.Add(this.proceduresTab1);
            this.Procedures.Location = new System.Drawing.Point(4, 22);
            this.Procedures.Name = "Procedures";
            this.Procedures.Size = new System.Drawing.Size(887, 275);
            this.Procedures.TabIndex = 2;
            this.Procedures.Text = "Procedures";
            this.Procedures.UseVisualStyleBackColor = true;
            // 
            // proceduresTab1
            // 
            this.proceduresTab1.Location = new System.Drawing.Point(4, 4);
            this.proceduresTab1.Name = "proceduresTab1";
            this.proceduresTab1.Size = new System.Drawing.Size(878, 266);
            this.proceduresTab1.TabIndex = 0;
            // 
            // ModelWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(923, 357);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Next);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.tabContainer);
            this.Controls.Add(this.OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModelWizard";
            this.Text = "ModelWizard";
            this.tabContainer.ResumeLayout(false);
            this.DesignerConnection.ResumeLayout(false);
            this.Enums.ResumeLayout(false);
            this.Procedures.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Back;
        private System.Windows.Forms.Button Next;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.TabPage Procedures;
        private ProceduresTab proceduresTab1;
        private System.Windows.Forms.TabPage Enums;
        private EnumsTab enumsTab1;
        private WizardTabControl tabContainer;
        internal DesignerConnectionTab designerConnectionTab;
        private System.Windows.Forms.TabPage DesignerConnection;
    }
}