namespace GestureDetectionApp
{
    partial class SimulationForm
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
            openglControl = new SharpGL.OpenGLControl();
            ((System.ComponentModel.ISupportInitialize)openglControl).BeginInit();
            SuspendLayout();
            // 
            // openglControl
            // 
            openglControl.AutoSize = true;
            openglControl.Dock = DockStyle.Fill;
            openglControl.DrawFPS = false;
            openglControl.FrameRate = 60;
            openglControl.Location = new Point(0, 0);
            openglControl.Margin = new Padding(4, 3, 4, 3);
            openglControl.Name = "openglControl";
            openglControl.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            openglControl.RenderContextType = SharpGL.RenderContextType.DIBSection;
            openglControl.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
            openglControl.Size = new Size(800, 450);
            openglControl.TabIndex = 0;
            // 
            // SimulationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(openglControl);
            Name = "SimulationForm";
            Text = "SimulationForm";
            ((System.ComponentModel.ISupportInitialize)openglControl).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SharpGL.OpenGLControl openglControl;
    }
}