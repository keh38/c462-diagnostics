namespace Launcher
{
    partial class LEDTestForm
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
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            redSlider = new KLib.Controls.ColorSlider();
            greenSlider = new KLib.Controls.ColorSlider();
            blueSlider = new KLib.Controls.ColorSlider();
            whiteSlider = new KLib.Controls.ColorSlider();
            clearButton = new System.Windows.Forms.Button();
            closeButton = new System.Windows.Forms.Button();
            statusTextBox = new System.Windows.Forms.TextBox();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(redSlider);
            flowLayoutPanel1.Controls.Add(greenSlider);
            flowLayoutPanel1.Controls.Add(blueSlider);
            flowLayoutPanel1.Controls.Add(whiteSlider);
            flowLayoutPanel1.Controls.Add(clearButton);
            flowLayoutPanel1.Controls.Add(closeButton);
            flowLayoutPanel1.Controls.Add(statusTextBox);
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(327, 412);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // redSlider
            // 
            redSlider.BackColor = System.Drawing.Color.Transparent;
            redSlider.BarInnerColor = System.Drawing.SystemColors.Control;
            redSlider.BarOuterColor = System.Drawing.SystemColors.Control;
            redSlider.BarPenColor = System.Drawing.Color.Black;
            redSlider.BorderRoundRectSize = new System.Drawing.Size(8, 8);
            redSlider.ElapsedInnerColor = System.Drawing.Color.FromArgb(255, 128, 128);
            redSlider.ElapsedOuterColor = System.Drawing.Color.FromArgb(255, 128, 128);
            redSlider.LargeChange = 5U;
            redSlider.Location = new System.Drawing.Point(3, 10);
            redSlider.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            redSlider.Name = "redSlider";
            redSlider.Size = new System.Drawing.Size(321, 38);
            redSlider.SmallChange = 1U;
            redSlider.TabIndex = 3;
            redSlider.Text = "redSlider";
            redSlider.ThumbInnerColor = System.Drawing.Color.Red;
            redSlider.ThumbOuterColor = System.Drawing.Color.Red;
            redSlider.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            redSlider.Value = 0;
            redSlider.ValueChanged += redSlider_ValueChanged;
            // 
            // greenSlider
            // 
            greenSlider.BackColor = System.Drawing.Color.Transparent;
            greenSlider.BarInnerColor = System.Drawing.SystemColors.Control;
            greenSlider.BarOuterColor = System.Drawing.SystemColors.Control;
            greenSlider.BarPenColor = System.Drawing.Color.Black;
            greenSlider.BorderRoundRectSize = new System.Drawing.Size(8, 8);
            greenSlider.ElapsedInnerColor = System.Drawing.Color.FromArgb(128, 255, 128);
            greenSlider.ElapsedOuterColor = System.Drawing.Color.FromArgb(128, 255, 128);
            greenSlider.LargeChange = 5U;
            greenSlider.Location = new System.Drawing.Point(3, 71);
            greenSlider.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            greenSlider.Name = "greenSlider";
            greenSlider.Size = new System.Drawing.Size(321, 38);
            greenSlider.SmallChange = 1U;
            greenSlider.TabIndex = 5;
            greenSlider.Text = "colorSlider2";
            greenSlider.ThumbInnerColor = System.Drawing.Color.Lime;
            greenSlider.ThumbOuterColor = System.Drawing.Color.Lime;
            greenSlider.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            greenSlider.Value = 0;
            greenSlider.ValueChanged += greenSlider_ValueChanged;
            // 
            // blueSlider
            // 
            blueSlider.BackColor = System.Drawing.Color.Transparent;
            blueSlider.BarInnerColor = System.Drawing.SystemColors.Control;
            blueSlider.BarOuterColor = System.Drawing.SystemColors.Control;
            blueSlider.BarPenColor = System.Drawing.Color.Black;
            blueSlider.BorderRoundRectSize = new System.Drawing.Size(8, 8);
            blueSlider.ElapsedInnerColor = System.Drawing.Color.FromArgb(128, 128, 255);
            blueSlider.ElapsedOuterColor = System.Drawing.Color.FromArgb(128, 128, 255);
            blueSlider.LargeChange = 5U;
            blueSlider.Location = new System.Drawing.Point(3, 132);
            blueSlider.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            blueSlider.Name = "blueSlider";
            blueSlider.Size = new System.Drawing.Size(321, 38);
            blueSlider.SmallChange = 1U;
            blueSlider.TabIndex = 4;
            blueSlider.Text = "colorSlider1";
            blueSlider.ThumbInnerColor = System.Drawing.Color.Blue;
            blueSlider.ThumbOuterColor = System.Drawing.Color.Blue;
            blueSlider.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            blueSlider.Value = 0;
            blueSlider.ValueChanged += blueSlider_ValueChanged;
            // 
            // whiteSlider
            // 
            whiteSlider.BackColor = System.Drawing.Color.Transparent;
            whiteSlider.BarInnerColor = System.Drawing.SystemColors.Control;
            whiteSlider.BarOuterColor = System.Drawing.SystemColors.Control;
            whiteSlider.BarPenColor = System.Drawing.Color.Black;
            whiteSlider.BorderRoundRectSize = new System.Drawing.Size(8, 8);
            whiteSlider.ElapsedInnerColor = System.Drawing.Color.White;
            whiteSlider.ElapsedOuterColor = System.Drawing.Color.White;
            whiteSlider.LargeChange = 5U;
            whiteSlider.Location = new System.Drawing.Point(3, 193);
            whiteSlider.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            whiteSlider.Name = "whiteSlider";
            whiteSlider.Size = new System.Drawing.Size(321, 38);
            whiteSlider.SmallChange = 1U;
            whiteSlider.TabIndex = 5;
            whiteSlider.Text = "colorSlider1";
            whiteSlider.ThumbInnerColor = System.Drawing.Color.White;
            whiteSlider.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            whiteSlider.Value = 0;
            whiteSlider.ValueChanged += whiteSlider_ValueChanged;
            // 
            // clearButton
            // 
            clearButton.AutoSize = true;
            clearButton.Dock = System.Windows.Forms.DockStyle.Fill;
            clearButton.Location = new System.Drawing.Point(3, 254);
            clearButton.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            clearButton.Name = "clearButton";
            clearButton.Size = new System.Drawing.Size(321, 46);
            clearButton.TabIndex = 3;
            clearButton.Text = "Clear";
            clearButton.UseVisualStyleBackColor = true;
            clearButton.Click += clearButton_Click;
            // 
            // closeButton
            // 
            closeButton.Location = new System.Drawing.Point(3, 323);
            closeButton.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(321, 46);
            closeButton.TabIndex = 3;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // statusTextBox
            // 
            statusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            statusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            statusTextBox.Location = new System.Drawing.Point(3, 382);
            statusTextBox.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            statusTextBox.Name = "statusTextBox";
            statusTextBox.ReadOnly = true;
            statusTextBox.Size = new System.Drawing.Size(321, 27);
            statusTextBox.TabIndex = 3;
            statusTextBox.Visible = false;
            // 
            // LEDTestForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ClientSize = new System.Drawing.Size(333, 417);
            Controls.Add(flowLayoutPanel1);
            Name = "LEDTestForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "LED Color";
            FormClosing += LEDTestForm_FormClosing;
            Shown += LEDTestForm_Shown;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private KLib.Controls.ColorSlider redSlider;
        private KLib.Controls.ColorSlider blueSlider;
        private KLib.Controls.ColorSlider greenSlider;
        private KLib.Controls.ColorSlider whiteSlider;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.TextBox statusTextBox;
    }
}