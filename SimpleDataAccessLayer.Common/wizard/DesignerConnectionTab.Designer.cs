namespace SimpleDataAccessLayer.Common.wizard
{
    partial class DesignerConnectionTab
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
            this.AuthenticationLabel = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.UserNameLabel = new System.Windows.Forms.Label();
            this.ServerNameLabel = new System.Windows.Forms.Label();
            this.Authentication = new System.Windows.Forms.ComboBox();
            this.ServerName = new System.Windows.Forms.TextBox();
            this.SavePassword = new System.Windows.Forms.CheckBox();
            this.DatabaseNameLabel = new System.Windows.Forms.Label();
            this.DatabaseName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Namespace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ConnectionString = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // AuthenticationLabel
            // 
            this.AuthenticationLabel.AutoSize = true;
            this.AuthenticationLabel.Location = new System.Drawing.Point(40, 42);
            this.AuthenticationLabel.Name = "AuthenticationLabel";
            this.AuthenticationLabel.Size = new System.Drawing.Size(78, 13);
            this.AuthenticationLabel.TabIndex = 3;
            this.AuthenticationLabel.Text = "Authentication:";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(155, 86);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.Size = new System.Drawing.Size(178, 20);
            this.Password.TabIndex = 8;
            this.Password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(40, 89);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(56, 13);
            this.PasswordLabel.TabIndex = 7;
            this.PasswordLabel.Text = "Password:";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(155, 63);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(178, 20);
            this.UserName.TabIndex = 6;
            this.UserName.TextChanged += new System.EventHandler(this.UserName_TextChanged);
            // 
            // UserNameLabel
            // 
            this.UserNameLabel.AutoSize = true;
            this.UserNameLabel.Location = new System.Drawing.Point(40, 66);
            this.UserNameLabel.Name = "UserNameLabel";
            this.UserNameLabel.Size = new System.Drawing.Size(61, 13);
            this.UserNameLabel.TabIndex = 5;
            this.UserNameLabel.Text = "User name:";
            // 
            // ServerNameLabel
            // 
            this.ServerNameLabel.AutoSize = true;
            this.ServerNameLabel.Location = new System.Drawing.Point(40, 19);
            this.ServerNameLabel.Name = "ServerNameLabel";
            this.ServerNameLabel.Size = new System.Drawing.Size(70, 13);
            this.ServerNameLabel.TabIndex = 1;
            this.ServerNameLabel.Text = "Server name:";
            // 
            // Authentication
            // 
            this.Authentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Authentication.FormattingEnabled = true;
            this.Authentication.Items.AddRange(new object[] {
            "Windows Authentication",
            "SQL Server Authentication"});
            this.Authentication.Location = new System.Drawing.Point(132, 39);
            this.Authentication.Name = "Authentication";
            this.Authentication.Size = new System.Drawing.Size(201, 21);
            this.Authentication.TabIndex = 4;
            this.Authentication.SelectedIndexChanged += new System.EventHandler(this.Authentication_SelectedIndexChanged);
            // 
            // ServerName
            // 
            this.ServerName.Location = new System.Drawing.Point(132, 16);
            this.ServerName.Name = "ServerName";
            this.ServerName.Size = new System.Drawing.Size(201, 20);
            this.ServerName.TabIndex = 2;
            this.ServerName.TextChanged += new System.EventHandler(this.ServerName_TextChanged);
            // 
            // SavePassword
            // 
            this.SavePassword.AutoSize = true;
            this.SavePassword.Location = new System.Drawing.Point(155, 110);
            this.SavePassword.Name = "SavePassword";
            this.SavePassword.Size = new System.Drawing.Size(99, 17);
            this.SavePassword.TabIndex = 9;
            this.SavePassword.Text = "Save password";
            this.SavePassword.UseVisualStyleBackColor = true;
            // 
            // DatabaseNameLabel
            // 
            this.DatabaseNameLabel.AutoSize = true;
            this.DatabaseNameLabel.Location = new System.Drawing.Point(40, 133);
            this.DatabaseNameLabel.Name = "DatabaseNameLabel";
            this.DatabaseNameLabel.Size = new System.Drawing.Size(85, 13);
            this.DatabaseNameLabel.TabIndex = 10;
            this.DatabaseNameLabel.Text = "Database name:";
            // 
            // DatabaseName
            // 
            this.DatabaseName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DatabaseName.FormattingEnabled = true;
            this.DatabaseName.Location = new System.Drawing.Point(132, 130);
            this.DatabaseName.Name = "DatabaseName";
            this.DatabaseName.Size = new System.Drawing.Size(201, 21);
            this.DatabaseName.TabIndex = 11;
            this.DatabaseName.DropDown += new System.EventHandler(this.DatabaseName_ReloadItems);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(160, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Namespace for generated code:";
            // 
            // Namespace
            // 
            this.Namespace.Location = new System.Drawing.Point(173, 73);
            this.Namespace.Name = "Namespace";
            this.Namespace.Size = new System.Drawing.Size(201, 20);
            this.Namespace.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Connection string:";
            // 
            // ConnectionString
            // 
            this.ConnectionString.Location = new System.Drawing.Point(173, 43);
            this.ConnectionString.Name = "ConnectionString";
            this.ConnectionString.Size = new System.Drawing.Size(201, 20);
            this.ConnectionString.TabIndex = 13;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DatabaseNameLabel);
            this.groupBox1.Controls.Add(this.DatabaseName);
            this.groupBox1.Controls.Add(this.SavePassword);
            this.groupBox1.Controls.Add(this.ServerName);
            this.groupBox1.Controls.Add(this.Authentication);
            this.groupBox1.Controls.Add(this.ServerNameLabel);
            this.groupBox1.Controls.Add(this.AuthenticationLabel);
            this.groupBox1.Controls.Add(this.Password);
            this.groupBox1.Controls.Add(this.PasswordLabel);
            this.groupBox1.Controls.Add(this.UserName);
            this.groupBox1.Controls.Add(this.UserNameLabel);
            this.groupBox1.Location = new System.Drawing.Point(8, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(377, 161);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Design Time Database Connection";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.Namespace);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.ConnectionString);
            this.groupBox2.Location = new System.Drawing.Point(405, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(383, 160);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Run Time Options";
            // 
            // DesignerConnectionTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DesignerConnectionTab";
            this.Size = new System.Drawing.Size(800, 182);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label AuthenticationLabel;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.Label UserNameLabel;
        private System.Windows.Forms.Label ServerNameLabel;
        private System.Windows.Forms.Label DatabaseNameLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        protected internal System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox ServerName;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.ComboBox Authentication;
        private System.Windows.Forms.CheckBox SavePassword;
        private System.Windows.Forms.ComboBox DatabaseName;
        private System.Windows.Forms.TextBox Namespace;
        private System.Windows.Forms.TextBox ConnectionString;
    }
}
