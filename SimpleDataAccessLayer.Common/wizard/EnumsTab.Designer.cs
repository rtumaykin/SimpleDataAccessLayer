namespace SimpleDataAccessLayer.Common.wizard
{
    partial class EnumsTab
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
            this.enumsGrid = new System.Windows.Forms.DataGridView();
            this.Schema = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KeyColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ValueColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Alias = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GenerateInterface = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.enumsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // enumsGrid
            // 
            this.enumsGrid.AllowUserToAddRows = false;
            this.enumsGrid.AllowUserToDeleteRows = false;
            this.enumsGrid.AllowUserToOrderColumns = true;
            this.enumsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.enumsGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.enumsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.enumsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Schema,
            this.TableName,
            this.KeyColumn,
            this.ValueColumn,
            this.Alias,
            this.GenerateInterface});
            this.enumsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.enumsGrid.Location = new System.Drawing.Point(3, 0);
            this.enumsGrid.MultiSelect = false;
            this.enumsGrid.Name = "enumsGrid";
            this.enumsGrid.Size = new System.Drawing.Size(872, 265);
            this.enumsGrid.TabIndex = 2;
            this.enumsGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.CellValueChangedHandler);
            this.enumsGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.ForceEndEditMode);
            // 
            // Schema
            // 
            this.Schema.HeaderText = "Source Table Schema";
            this.Schema.Name = "Schema";
            this.Schema.ReadOnly = true;
            // 
            // TableName
            // 
            this.TableName.FillWeight = 150F;
            this.TableName.HeaderText = "Source Table Name";
            this.TableName.Name = "TableName";
            this.TableName.ReadOnly = true;
            this.TableName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // KeyColumn
            // 
            this.KeyColumn.AutoComplete = false;
            this.KeyColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeyColumn.HeaderText = "Key Column";
            this.KeyColumn.Name = "KeyColumn";
            this.KeyColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.KeyColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ValueColumn
            // 
            this.ValueColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ValueColumn.HeaderText = "Value Column";
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ValueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // Alias
            // 
            this.Alias.HeaderText = "Enum Alias";
            this.Alias.Name = "Alias";
            // 
            // GenerateInterface
            // 
            this.GenerateInterface.FillWeight = 50F;
            this.GenerateInterface.HeaderText = "Generate";
            this.GenerateInterface.Name = "GenerateInterface";
            // 
            // EnumsTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.enumsGrid);
            this.Name = "EnumsTab";
            this.Size = new System.Drawing.Size(872, 265);
            ((System.ComponentModel.ISupportInitialize)(this.enumsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView enumsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Schema;
        private System.Windows.Forms.DataGridViewTextBoxColumn TableName;
        private System.Windows.Forms.DataGridViewComboBoxColumn KeyColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn ValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Alias;
        private System.Windows.Forms.DataGridViewCheckBoxColumn GenerateInterface;
    }
}
