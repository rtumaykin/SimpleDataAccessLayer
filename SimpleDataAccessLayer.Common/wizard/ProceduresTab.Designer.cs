namespace SimpleDataAccessLayer.Common.wizard
{
    partial class ProceduresTab
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.proceduresGrid = new System.Windows.Forms.DataGridView();
            this.Schema = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProcedureName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Alias = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GenerateInterface = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.proceduresGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // proceduresGrid
            // 
            this.proceduresGrid.AllowUserToAddRows = false;
            this.proceduresGrid.AllowUserToDeleteRows = false;
            this.proceduresGrid.AllowUserToOrderColumns = true;
            this.proceduresGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.proceduresGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.proceduresGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.proceduresGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Schema,
            this.ProcedureName,
            this.Alias,
            this.GenerateInterface});
            this.proceduresGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.proceduresGrid.Location = new System.Drawing.Point(3, 1);
            this.proceduresGrid.MultiSelect = false;
            this.proceduresGrid.Name = "proceduresGrid";
            this.proceduresGrid.Size = new System.Drawing.Size(872, 265);
            this.proceduresGrid.TabIndex = 2;
            this.proceduresGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.CellValueChangedHandler);
            this.proceduresGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.ForceEndEditMode);
            // 
            // Schema
            // 
            this.Schema.HeaderText = "Stored Procedure Schema";
            this.Schema.Name = "Schema";
            this.Schema.ReadOnly = true;
            // 
            // ProcedureName
            // 
            this.ProcedureName.FillWeight = 150F;
            this.ProcedureName.HeaderText = "Stored Procedure Name";
            this.ProcedureName.Name = "ProcedureName";
            this.ProcedureName.ReadOnly = true;
            this.ProcedureName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // Alias
            // 
            this.Alias.HeaderText = "Alias";
            this.Alias.Name = "Alias";
            // 
            // GenerateInterface
            // 
            this.GenerateInterface.FillWeight = 50F;
            this.GenerateInterface.HeaderText = "Generate";
            this.GenerateInterface.Name = "GenerateInterface";
            // 
            // ProceduresTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.proceduresGrid);
            this.Name = "ProceduresTab";
            this.Size = new System.Drawing.Size(878, 266);
            ((System.ComponentModel.ISupportInitialize)(this.proceduresGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView proceduresGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Schema;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProcedureName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Alias;
        private System.Windows.Forms.DataGridViewCheckBoxColumn GenerateInterface;
    }
}
