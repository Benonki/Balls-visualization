namespace Circle_Collision
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
            addBallBtn = new Button();
            mainArea = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)mainArea).BeginInit();
            SuspendLayout();
            // 
            // addBallBtn
            // 
            addBallBtn.Location = new Point(997, 53);
            addBallBtn.Name = "addBallBtn";
            addBallBtn.Size = new Size(119, 45);
            addBallBtn.TabIndex = 0;
            addBallBtn.Text = "Add Ball";
            addBallBtn.UseVisualStyleBackColor = true;
            addBallBtn.Click += addBallBtn_Click;
            // 
            // mainArea
            // 
            mainArea.BorderStyle = BorderStyle.FixedSingle;
            mainArea.Location = new Point(122, 39);
            mainArea.Name = "mainArea";
            mainArea.Size = new Size(755, 494);
            mainArea.TabIndex = 1;
            mainArea.TabStop = false;
            mainArea.Paint += mainArea_Paint;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1160, 595);
            Controls.Add(mainArea);
            Controls.Add(addBallBtn);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)mainArea).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button addBallBtn;
        private PictureBox mainArea;
    }
}
