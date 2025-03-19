﻿namespace Circle_Collision
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            mainArea = new PictureBox();
            labels = new Label[3];
            textBoxesMass = new TextBox[3];
            textBoxesAcceleration = new TextBox[3];
            textBoxesVelocity = new TextBox[3];
            textBoxesRadius = new TextBox[3];
            startButton = new Button(); 
            stopButton = new Button(); 

            ((System.ComponentModel.ISupportInitialize)mainArea).BeginInit();
            SuspendLayout();

            // Obszar rysowania
            mainArea.BorderStyle = BorderStyle.FixedSingle;
            mainArea.Location = new Point(50, 50);
            mainArea.Name = "mainArea";
            mainArea.Size = new Size(500, 500);
            mainArea.TabIndex = 1;
            mainArea.TabStop = false;
            mainArea.Paint += mainArea_Paint;

            // Przycisk Start
            startButton.Location = new Point(600, 500);
            startButton.Size = new Size(150, 40);
            startButton.Name = "startButton";
            startButton.Text = "Start symulacji";
            startButton.Click += StartButton_Click;
            startButton.ForeColor = Color.FromArgb(244, 252, 19);
            Controls.Add(startButton);

            // Przycisk Stop
            stopButton.Location = new Point(600, 550);
            stopButton.Size = new Size(150, 40);
            stopButton.Name = "stopButton";
            stopButton.Text = "Stop symulacji";
            stopButton.Click += StartButton_Click;
            stopButton.ForeColor = Color.FromArgb(244, 252, 19);
            Controls.Add(stopButton);

            // Tworzenie kontrolek dla 3 kulek
            for (int i = 0; i < 3; i++)
            {
                int yOffset = 50 + i * 160;

                // Etykieta kulki
                labels[i] = new Label();
                labels[i].AutoSize = true;
                labels[i].Location = new Point(600, yOffset);
                labels[i].Name = $"labelBall{i + 1}";
                labels[i].Size = new Size(100, 20);
                labels[i].Text = $"Kulka {i + 1}";
                labels[i].ForeColor = Color.FromArgb(244, 252, 19);
                labels[i].Font = new Font("Arial", 10, FontStyle.Bold);
                Controls.Add(labels[i]);

                // Pole masa
                textBoxesMass[i] = new TextBox();
                textBoxesMass[i].Location = new Point(600, yOffset + 30);
                textBoxesMass[i].Size = new Size(80, 27);
                textBoxesMass[i].Name = $"textBoxMass{i + 1}";
                textBoxesMass[i].PlaceholderText = "Masa";
                textBoxesMass[i].TextChanged += textBoxMass_TextChanged;
                Controls.Add(textBoxesMass[i]);

                // Pole promień
                textBoxesRadius[i] = new TextBox();
                textBoxesRadius[i].Location = new Point(700, yOffset + 30);
                textBoxesRadius[i].Size = new Size(80, 27);
                textBoxesRadius[i].Name = $"textBoxRadius{i + 1}";
                textBoxesRadius[i].PlaceholderText = "Promień";
                textBoxesRadius[i].TextChanged += textBoxRadius_TextChanged;
                Controls.Add(textBoxesRadius[i]);

                // Pole przyspieszenie
                textBoxesAcceleration[i] = new TextBox();
                textBoxesAcceleration[i].Location = new Point(600, yOffset + 70);
                textBoxesAcceleration[i].Size = new Size(80, 27);
                textBoxesAcceleration[i].Name = $"textBoxAcceleration{i + 1}";
                textBoxesAcceleration[i].PlaceholderText = "Przyspieszenie";
                textBoxesAcceleration[i].TextChanged += textBoxAcceleration_TextChanged;
                Controls.Add(textBoxesAcceleration[i]);

                // Pole prędkość
                textBoxesVelocity[i] = new TextBox();
                textBoxesVelocity[i].Location = new Point(700, yOffset + 70);
                textBoxesVelocity[i].Size = new Size(80, 27);
                textBoxesVelocity[i].Name = $"textBoxVelocity{i + 1}";
                textBoxesVelocity[i].PlaceholderText = "Prędkość";
                textBoxesVelocity[i].TextChanged += textBoxVelocity_TextChanged;
                Controls.Add(textBoxesVelocity[i]);
            }

            // Form1
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(850, 600);
            Controls.Add(mainArea);
            Name = "Form1";
            Text = "Circle Collision";
            Load += Form1_Load;

            ((System.ComponentModel.ISupportInitialize)mainArea).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox mainArea;
        private Label[] labels;
        private TextBox[] textBoxesMass;
        private TextBox[] textBoxesAcceleration;
        private TextBox[] textBoxesVelocity;
        private TextBox[] textBoxesRadius;
        private Button startButton;
        private Button stopButton;
    }
}