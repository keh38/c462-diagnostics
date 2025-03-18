namespace Launcher
{
    partial class ConfigForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Jack = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Modality = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Transducer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Channel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Extra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mapDropDown = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.comPortDropDown = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.continueButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Jack,
            this.Modality,
            this.Transducer,
            this.Channel,
            this.Extra});
            this.dataGridView.Location = new System.Drawing.Point(11, 76);
            this.dataGridView.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowTemplate.Height = 24;
            this.dataGridView.Size = new System.Drawing.Size(503, 291);
            this.dataGridView.TabIndex = 0;
            // 
            // Jack
            // 
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Jack.DefaultCellStyle = dataGridViewCellStyle3;
            this.Jack.HeaderText = "Jack";
            this.Jack.Name = "Jack";
            this.Jack.ReadOnly = true;
            // 
            // Modality
            // 
            this.Modality.HeaderText = "Modality";
            this.Modality.Items.AddRange(new object[] {
            "Audio",
            "Haptic",
            "Electric"});
            this.Modality.Name = "Modality";
            // 
            // Transducer
            // 
            this.Transducer.HeaderText = "Transducer";
            this.Transducer.Name = "Transducer";
            this.Transducer.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Transducer.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Channel
            // 
            this.Channel.HeaderText = "Channel";
            this.Channel.Name = "Channel";
            // 
            // Extra
            // 
            this.Extra.HeaderText = "Max";
            this.Extra.Name = "Extra";
            // 
            // mapDropDown
            // 
            this.mapDropDown.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.mapDropDown.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.mapDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mapDropDown.FormattingEnabled = true;
            this.mapDropDown.Location = new System.Drawing.Point(11, 38);
            this.mapDropDown.Name = "mapDropDown";
            this.mapDropDown.Size = new System.Drawing.Size(160, 21);
            this.mapDropDown.TabIndex = 1;
            this.mapDropDown.SelectedIndexChanged += new System.EventHandler(this.mapDropDown_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(358, 36);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // comPortDropDown
            // 
            this.comPortDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comPortDropDown.FormattingEnabled = true;
            this.comPortDropDown.Location = new System.Drawing.Point(444, 381);
            this.comPortDropDown.Name = "comPortDropDown";
            this.comPortDropDown.Size = new System.Drawing.Size(70, 21);
            this.comPortDropDown.TabIndex = 3;
            this.comPortDropDown.SelectedIndexChanged += new System.EventHandler(this.comPortDropDown_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(383, 385);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Audio sync";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Select configuration";
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(439, 36);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(75, 23);
            this.continueButton.TabIndex = 6;
            this.continueButton.Text = "Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 437);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comPortDropDown);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.mapDropDown);
            this.Controls.Add(this.dataGridView);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Adapter Map";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Jack;
        private System.Windows.Forms.DataGridViewComboBoxColumn Modality;
        private System.Windows.Forms.DataGridViewTextBoxColumn Transducer;
        private System.Windows.Forms.DataGridViewTextBoxColumn Channel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Extra;
        private System.Windows.Forms.ComboBox mapDropDown;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.ComboBox comPortDropDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button continueButton;
    }
}