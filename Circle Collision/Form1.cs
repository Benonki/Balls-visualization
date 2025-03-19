using Circle_Collision.Classes;

namespace Circle_Collision
{
    public partial class Form1 : Form
    {
        private bool simulationStarted = false;
        private int radius = 40;
        private float mass = 10;
        private List<Ball> balls = new List<Ball>();
        private List<Tuple<Ball, Ball>> collidingPairs = new List<Tuple<Ball, Ball>>();
        private System.Windows.Forms.Timer moveTimer;
        private Ball draggedBall = null;
        private PointF dragOffset;
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            mainArea.BackColor = Color.Black;
            mainArea.Paint += (sender, e) =>
            {
                Graphics g = e.Graphics;
                Pen borderPen = new Pen(Color.FromArgb(244, 252, 19), 3);
                g.DrawRectangle(borderPen, 0, 0, mainArea.Width - 3, mainArea.Height - 3);
            };

            balls.Add(new Ball(0, 10, 100, radius, new PointF(2, 2), new PointF(0, 0), mass, Color.Red));
            balls.Add(new Ball(1, 300, 200, radius, new PointF(-3, 1), new PointF(0, 0), mass, Color.Blue));
            balls.Add(new Ball(2, 100, 200, radius, new PointF(1, -2), new PointF(0, 0), mass, Color.Green));

            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 16;
            moveTimer.Tick += MoveBalls;
            moveTimer.Start();

            mainArea.MouseDown += MainArea_MouseDown;
            mainArea.MouseMove += MainArea_MouseMove;
            mainArea.MouseUp += MainArea_MouseUp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void MoveBalls(object sender, EventArgs e)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                Ball ball = balls[i];

                if (simulationStarted)
                {
                    ball.Velocity = new PointF(
                        ball.Velocity.X + ball.Acceleration.X * 0.16f,
                        ball.Velocity.Y + ball.Acceleration.Y * 0.16f
                    );
                    ball.PosX += ball.Velocity.X;
                    ball.PosY += ball.Velocity.Y;

                    if (ball.PosX <= 0 || ball.PosX + ball.Radius * 2 >= mainArea.Width)
                        ball.Velocity = new PointF(-ball.Velocity.X, ball.Velocity.Y);

                    if (ball.PosY <= 0 || ball.PosY + ball.Radius * 2 >= mainArea.Height)
                        ball.Velocity = new PointF(ball.Velocity.X, -ball.Velocity.Y);

                    ball.PosX = Math.Clamp(ball.PosX, 0, mainArea.Width - ball.Radius * 2);
                    ball.PosY = Math.Clamp(ball.PosY, 0, mainArea.Height - ball.Radius * 2);
                }

                ball.UpdateBall();
            }

            var BallsOverlap = (Ball ball, Ball target) =>
            {
                double dx = (ball.PosX + ball.Radius) - (target.PosX + target.Radius);
                double dy = (ball.PosY + ball.Radius) - (target.PosY + target.Radius);
                double distance = Math.Sqrt(dx * dx + dy * dy);
                return distance <= ball.Radius + target.Radius;
            };

            var pairsToRemove = new List<Tuple<Ball, Ball>>();

            foreach (Ball ball in balls)
            {
                foreach (Ball target in balls)
                {
                    if (target.Id != ball.Id)
                    {

                        bool isColliding = BallsOverlap(ball, target);

                        if (isColliding && !collidingPairs.Any(tuple => (tuple.Item1 == ball && tuple.Item2 == target ||
                                                                        tuple.Item1 == target && tuple.Item2 == ball)))
                        {
                            collidingPairs.Add(new Tuple<Ball, Ball>(ball, target));
                        }

                        if (!isColliding && collidingPairs.Any(tuple => (tuple.Item1 == ball && tuple.Item2 == target ||
                                                                        tuple.Item1 == target && tuple.Item2 == ball)))
                        {
                            pairsToRemove.Add(new Tuple<Ball, Ball>(ball, target));
                        }

                        if (isColliding)
                        {
                            double dx = (ball.PosX + ball.Radius) - (target.PosX + target.Radius);
                            double dy = (ball.PosY + ball.Radius) - (target.PosY + target.Radius);
                            double distance = Math.Sqrt(dx * dx + dy * dy);

                            float overlap = 0.5f * ((float)distance - ball.Radius - target.Radius);

                            ball.PosX -= overlap * (ball.PosX - target.PosX) / (float)distance;
                            ball.PosY -= overlap * (ball.PosY - target.PosY) / (float)distance;

                            target.PosX += overlap * (ball.PosX - target.PosX) / (float)distance;
                            target.PosY += overlap * (ball.PosY - target.PosY) / (float)distance;

                            HandleCollision(ball, target);

                        }
                    }
                }

                foreach (var pair in pairsToRemove)
                {
                    collidingPairs.Remove(pair);
                }


            }
            mainArea.Refresh();
        }

        private void HandleCollision(Ball ball1, Ball ball2)
        {
            double dx = (ball1.PosX + ball1.Radius) - (ball2.PosX + ball2.Radius);
            double dy = (ball1.PosY + ball1.Radius) - (ball2.PosY + ball2.Radius);
            double distance = Math.Sqrt(dx * dx + dy * dy);

            float nx = (ball2.PosX - ball1.PosX) / (float)distance;
            float ny = (ball2.PosY - ball1.PosY) / (float)distance;

            float tx = -ny;
            float ty = nx;


            float iloczynTan1 = ball1.Velocity.X * tx + ball1.Velocity.Y * ty;
            float iloczynTan2 = ball2.Velocity.X * tx + ball2.Velocity.Y * ty;

            float iloczynNorm1 = ball1.Velocity.X * nx + ball1.Velocity.Y * ny;
            float iloczynNorm2 = ball2.Velocity.X * nx + ball2.Velocity.Y * ny;

            float m1 = (iloczynNorm1 * (ball1.Mass - ball2.Mass) + 2.0f * ball2.Mass * iloczynNorm2) / (ball1.Mass + ball2.Mass);
            float m2 = (iloczynNorm2 * (ball2.Mass - ball1.Mass) + 2.0f * ball1.Mass * iloczynNorm1) / (ball1.Mass + ball2.Mass);

            float vx1 = tx * iloczynTan1 + nx * m1;
            float vy1 = ty * iloczynTan1 + ny * m1;
            float vx2 = tx * iloczynTan2 + nx * m2;
            float vy2 = ty * iloczynTan2 + ny * m2;

            ball1.Velocity = new PointF(vx1, vy1);
            ball2.Velocity = new PointF(vx2, vy2);
        }

        private void mainArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p = new Pen(Color.FromArgb(244, 252, 19));
            Pen blackPen = new Pen(Brushes.Black);
            blackPen.Width = 1.5f;
            p.Width = 5.0f;

            foreach (Ball ball in balls)
            {
                using (SolidBrush sb = new SolidBrush(ball.Color))
                {
                    g.FillEllipse(sb, ball.BallToDraw);
                    g.DrawEllipse(blackPen, ball.BallToDraw);
                }
            }

            foreach (var tuple in collidingPairs)
            {
                g.DrawLine(p, new PointF(tuple.Item1.PosX + tuple.Item1.Radius, tuple.Item1.PosY + tuple.Item1.Radius), new PointF(tuple.Item2.PosX + tuple.Item2.Radius, tuple.Item2.PosY + tuple.Item2.Radius));
            }
        }

        private void textBoxMass_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesMass, sender as TextBox);
            if (index >= 0 && float.TryParse(textBoxesMass[index].Text, out float newMass))
            {
                balls[index].Mass = newMass;
            }
        }

        private void textBoxRadius_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesRadius, sender as TextBox);
            if (index >= 0 && int.TryParse(textBoxesRadius[index].Text, out int newRadius))
            {
                balls[index].Radius = newRadius;
            }
        }

        private void textBoxAcceleration_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesAcceleration, sender as TextBox);
            if (index >= 0 && !string.IsNullOrWhiteSpace(textBoxesAcceleration[index].Text))
            {
                string[] accelerationValues = textBoxesAcceleration[index].Text.Split(',');
                if (accelerationValues.Length == 2 && float.TryParse(accelerationValues[0], out float ax) && float.TryParse(accelerationValues[1], out float ay))
                {
                    balls[index].Acceleration = new PointF(ax, ay);
                }
            }
        }

        private void textBoxVelocity_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesVelocity, sender as TextBox);
            if (index >= 0 && !string.IsNullOrWhiteSpace(textBoxesVelocity[index].Text))
            {
                string[] velocityValues = textBoxesVelocity[index].Text.Split(',');
                if (velocityValues.Length == 2 && float.TryParse(velocityValues[0], out float vx) && float.TryParse(velocityValues[1], out float vy))
                {
                    balls[index].Velocity = new PointF(vx, vy);
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            simulationStarted = true;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            simulationStarted = false;
        }

        private void MainArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !simulationStarted)
            {
                foreach (Ball ball in balls)
                {
                    if (e.X >= ball.PosX && e.X <= ball.PosX + ball.Radius * 2 &&
                        e.Y >= ball.PosY && e.Y <= ball.PosY + ball.Radius * 2)
                    {
                        draggedBall = ball;
                        dragOffset = new PointF(e.X - ball.PosX, e.Y - ball.PosY);
                        break;
                    }
                }
            }
        }

        private void MainArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedBall != null && !simulationStarted)
            {
                draggedBall.PosX = e.X - dragOffset.X;
                draggedBall.PosY = e.Y - dragOffset.Y;
                draggedBall.PosX = Math.Clamp(draggedBall.PosX, 0, mainArea.Width - draggedBall.Radius * 2);
                draggedBall.PosY = Math.Clamp(draggedBall.PosY, 0, mainArea.Height - draggedBall.Radius * 2);

                mainArea.Refresh();
            }
        }

        private void MainArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draggedBall = null;
            }
        }
    }
}