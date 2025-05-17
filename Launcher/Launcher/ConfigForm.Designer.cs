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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            dataGridView = new System.Windows.Forms.DataGridView();
            Jack = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Modality = new System.Windows.Forms.DataGridViewComboBoxColumn();
            Transducer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Channel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Extra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            mapDropDown = new System.Windows.Forms.ComboBox();
            saveButton = new System.Windows.Forms.Button();
            comPortDropDown = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            continueButton = new System.Windows.Forms.Button();
            detectButton = new System.Windows.Forms.Button();
            messageLabel = new System.Windows.Forms.Label();
            dsrDropDown = new System.Windows.Forms.ComboBox();
            ledDetectButton = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            ledComPortDropDown = new System.Windows.Forms.ComboBox();
            brightnessNumeric = new KLib.Controls.KNumericBox();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            gammaNumeric = new KLib.Controls.KNumericBox();
            testButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            SuspendLayout();
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToResizeColumns = false;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Jack, Modality, Transducer, Channel, Extra });
            dataGridView.Location = new System.Drawing.Point(15, 117);
            dataGridView.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            dataGridView.Name = "dataGridView";
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowHeadersWidth = 51;
            dataGridView.RowTemplate.Height = 24;
            dataGridView.Size = new System.Drawing.Size(671, 448);
            dataGridView.TabIndex = 0;
            dataGridView.CellClick += dataGridView_CellClick;
            dataGridView.CellValueChanged += dataGridView_CellValueChanged;
            dataGridView.CurrentCellDirtyStateChanged += dataGridView_CurrentCellDirtyStateChanged;
            dataGridView.Leave += dataGridView_Leave;
            // 
            // Jack
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            Jack.DefaultCellStyle = dataGridViewCellStyle1;
            Jack.HeaderText = "Jack";
            Jack.MinimumWidth = 6;
            Jack.Name = "Jack";
            Jack.ReadOnly = true;
            Jack.Width = 125;
            // 
            // Modality
            // 
            Modality.HeaderText = "Modality";
            Modality.Items.AddRange(new object[] { "Audio", "Haptic", "Electric" });
            Modality.MinimumWidth = 6;
            Modality.Name = "Modality";
            Modality.Width = 125;
            // 
            // Transducer
            // 
            Transducer.HeaderText = "Transducer";
            Transducer.MinimumWidth = 6;
            Transducer.Name = "Transducer";
            Transducer.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            Transducer.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Transducer.Width = 125;
            // 
            // Channel
            // 
            Channel.HeaderText = "Channel";
            Channel.MinimumWidth = 6;
            Channel.Name = "Channel";
            Channel.Width = 125;
            // 
            // Extra
            // 
            Extra.HeaderText = "Max";
            Extra.MinimumWidth = 6;
            Extra.Name = "Extra";
            Extra.Width = 125;
            // 
            // mapDropDown
            // 
            mapDropDown.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            mapDropDown.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            mapDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            mapDropDown.FormattingEnabled = true;
            mapDropDown.Location = new System.Drawing.Point(15, 59);
            mapDropDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            mapDropDown.Name = "mapDropDown";
            mapDropDown.Size = new System.Drawing.Size(212, 28);
            mapDropDown.TabIndex = 1;
            mapDropDown.SelectedIndexChanged += mapDropDown_SelectedIndexChanged;
            // 
            // saveButton
            // 
            saveButton.Location = new System.Drawing.Point(478, 56);
            saveButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(101, 36);
            saveButton.TabIndex = 2;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // comPortDropDown
            // 
            comPortDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comPortDropDown.FormattingEnabled = true;
            comPortDropDown.Location = new System.Drawing.Point(511, 589);
            comPortDropDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            comPortDropDown.Name = "comPortDropDown";
            comPortDropDown.Size = new System.Drawing.Size(92, 28);
            comPortDropDown.TabIndex = 3;
            comPortDropDown.SelectedIndexChanged += comPortDropDown_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(430, 596);
            label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(81, 20);
            label1.TabIndex = 4;
            label1.Text = "Audio sync";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(16, 28);
            label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(142, 20);
            label2.TabIndex = 5;
            label2.Text = "Select configuration";
            // 
            // continueButton
            // 
            continueButton.Location = new System.Drawing.Point(585, 56);
            continueButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            continueButton.Name = "continueButton";
            continueButton.Size = new System.Drawing.Size(101, 36);
            continueButton.TabIndex = 6;
            continueButton.Text = "Continue";
            continueButton.UseVisualStyleBackColor = true;
            continueButton.Click += continueButton_Click;
            // 
            // detectButton
            // 
            detectButton.Location = new System.Drawing.Point(613, 588);
            detectButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            detectButton.Name = "detectButton";
            detectButton.Size = new System.Drawing.Size(73, 36);
            detectButton.TabIndex = 7;
            detectButton.Text = "Detect";
            detectButton.UseVisualStyleBackColor = true;
            detectButton.Click += detectButton_Click;
            // 
            // messageLabel
            // 
            messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            messageLabel.Location = new System.Drawing.Point(15, 589);
            messageLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            messageLabel.Name = "messageLabel";
            messageLabel.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            messageLabel.Size = new System.Drawing.Size(386, 162);
            messageLabel.TabIndex = 8;
            messageLabel.Text = "label3";
            messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            messageLabel.Visible = false;
            // 
            // dsrDropDown
            // 
            dsrDropDown.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            dsrDropDown.FormattingEnabled = true;
            dsrDropDown.Location = new System.Drawing.Point(275, 41);
            dsrDropDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            dsrDropDown.Name = "dsrDropDown";
            dsrDropDown.Size = new System.Drawing.Size(138, 28);
            dsrDropDown.TabIndex = 9;
            dsrDropDown.Visible = false;
            dsrDropDown.DrawItem += dsrDropDown_DrawItem;
            dsrDropDown.SelectedIndexChanged += dsrDropDown_SelectedIndexChanged;
            // 
            // ledDetectButton
            // 
            ledDetectButton.Location = new System.Drawing.Point(613, 631);
            ledDetectButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            ledDetectButton.Name = "ledDetectButton";
            ledDetectButton.Size = new System.Drawing.Size(73, 36);
            ledDetectButton.TabIndex = 12;
            ledDetectButton.Text = "Detect";
            ledDetectButton.UseVisualStyleBackColor = true;
            ledDetectButton.Click += ledDetectButton_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(424, 639);
            label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(86, 20);
            label3.TabIndex = 11;
            label3.Text = "LED control";
            // 
            // ledComPortDropDown
            // 
            ledComPortDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ledComPortDropDown.FormattingEnabled = true;
            ledComPortDropDown.Location = new System.Drawing.Point(511, 632);
            ledComPortDropDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            ledComPortDropDown.Name = "ledComPortDropDown";
            ledComPortDropDown.Size = new System.Drawing.Size(92, 28);
            ledComPortDropDown.TabIndex = 10;
            ledComPortDropDown.SelectedIndexChanged += ledComPortDropDown_SelectedIndexChanged;
            // 
            // brightnessNumeric
            // 
            brightnessNumeric.AllowInf = false;
            brightnessNumeric.AutoSize = true;
            brightnessNumeric.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            brightnessNumeric.ClearOnDisable = false;
            brightnessNumeric.FloatValue = 0F;
            brightnessNumeric.IntValue = 0;
            brightnessNumeric.IsInteger = true;
            brightnessNumeric.Location = new System.Drawing.Point(511, 728);
            brightnessNumeric.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            brightnessNumeric.MaxCoerce = true;
            brightnessNumeric.MaximumSize = new System.Drawing.Size(22857, 36);
            brightnessNumeric.MaxValue = 100D;
            brightnessNumeric.MinCoerce = true;
            brightnessNumeric.MinimumSize = new System.Drawing.Size(11, 36);
            brightnessNumeric.MinValue = -1D;
            brightnessNumeric.Name = "brightnessNumeric";
            brightnessNumeric.Size = new System.Drawing.Size(94, 36);
            brightnessNumeric.TabIndex = 13;
            brightnessNumeric.TextFormat = "K4";
            brightnessNumeric.ToolTip = "";
            brightnessNumeric.Units = "";
            brightnessNumeric.Value = 0D;
            brightnessNumeric.ValueChanged += brightnessNumeric_ValueChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(433, 732);
            label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(77, 20);
            label4.TabIndex = 14;
            label4.Text = "Brightness";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(418, 681);
            label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(91, 20);
            label5.TabIndex = 16;
            label5.Text = "LED Gamma";
            // 
            // gammaNumeric
            // 
            gammaNumeric.AllowInf = false;
            gammaNumeric.AutoSize = true;
            gammaNumeric.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            gammaNumeric.ClearOnDisable = false;
            gammaNumeric.FloatValue = 1F;
            gammaNumeric.IntValue = 1;
            gammaNumeric.IsInteger = false;
            gammaNumeric.Location = new System.Drawing.Point(511, 676);
            gammaNumeric.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            gammaNumeric.MaxCoerce = false;
            gammaNumeric.MaximumSize = new System.Drawing.Size(22857, 36);
            gammaNumeric.MaxValue = 100D;
            gammaNumeric.MinCoerce = true;
            gammaNumeric.MinimumSize = new System.Drawing.Size(11, 36);
            gammaNumeric.MinValue = 1D;
            gammaNumeric.Name = "gammaNumeric";
            gammaNumeric.Size = new System.Drawing.Size(94, 36);
            gammaNumeric.TabIndex = 15;
            gammaNumeric.TextFormat = "K4";
            gammaNumeric.ToolTip = "";
            gammaNumeric.Units = "";
            gammaNumeric.Value = 1D;
            gammaNumeric.ValueChanged += gammaNumeric_ValueChanged;
            // 
            // testButton
            // 
            testButton.Location = new System.Drawing.Point(613, 673);
            testButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            testButton.Name = "testButton";
            testButton.Size = new System.Drawing.Size(73, 36);
            testButton.TabIndex = 17;
            testButton.Text = "Test";
            testButton.UseVisualStyleBackColor = true;
            testButton.Click += testButton_Click;
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(705, 777);
            Controls.Add(testButton);
            Controls.Add(label5);
            Controls.Add(gammaNumeric);
            Controls.Add(label4);
            Controls.Add(brightnessNumeric);
            Controls.Add(ledDetectButton);
            Controls.Add(label3);
            Controls.Add(ledComPortDropDown);
            Controls.Add(dsrDropDown);
            Controls.Add(messageLabel);
            Controls.Add(detectButton);
            Controls.Add(continueButton);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(comPortDropDown);
            Controls.Add(saveButton);
            Controls.Add(mapDropDown);
            Controls.Add(dataGridView);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            Name = "ConfigForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Adapter Map";
            Load += ConfigForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.Button detectButton;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.ComboBox dsrDropDown;
        private System.Windows.Forms.Button ledDetectButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ledComPortDropDown;
        private KLib.Controls.KNumericBox brightnessNumeric;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private KLib.Controls.KNumericBox gammaNumeric;
        private System.Windows.Forms.Button testButton;
    }
}