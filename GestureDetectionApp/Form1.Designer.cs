namespace GestureDetectionApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox_gesture = new TextBox();
            label1 = new Label();
            textBox_log = new TextBox();
            label2 = new Label();
            button_log = new Button();
            SuspendLayout();
            // 
            // textBox_gesture
            // 
            textBox_gesture.Location = new Point(12, 26);
            textBox_gesture.Multiline = true;
            textBox_gesture.Name = "textBox_gesture";
            textBox_gesture.ReadOnly = true;
            textBox_gesture.Size = new Size(293, 178);
            textBox_gesture.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(129, 9);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 1;
            label1.Text = "Gesture";
            // 
            // textBox_log
            // 
            textBox_log.Location = new Point(382, 26);
            textBox_log.Multiline = true;
            textBox_log.Name = "textBox_log";
            textBox_log.ReadOnly = true;
            textBox_log.ScrollBars = ScrollBars.Vertical;
            textBox_log.Size = new Size(406, 178);
            textBox_log.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(575, 9);
            label2.Name = "label2";
            label2.Size = new Size(27, 15);
            label2.TabIndex = 3;
            label2.Text = "Log";
            // 
            // button_log
            // 
            button_log.Location = new Point(616, 210);
            button_log.Name = "button_log";
            button_log.Size = new Size(75, 23);
            button_log.TabIndex = 4;
            button_log.Text = "Create Log";
            button_log.UseVisualStyleBackColor = true;
            button_log.Click += button_log_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button_log);
            Controls.Add(label2);
            Controls.Add(textBox_log);
            Controls.Add(label1);
            Controls.Add(textBox_gesture);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox_gesture;
        private Label label1;
        private TextBox textBox_log;
        private Label label2;
        private Button button_log;
    }


}
