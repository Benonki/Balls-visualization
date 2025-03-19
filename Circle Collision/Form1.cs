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
        private Ball rotatingLineBall = null;
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

            balls.Add(new Ball(0, 50, 100, radius, new PointF(0, 2), new PointF(0, 0), mass, Color.Red));
            balls.Add(new Ball(1, 300, 100, radius, new PointF(0, -2), new PointF(0, 0), mass, Color.Blue));
            balls.Add(new Ball(2, 175, 250, radius, new PointF(0, -1), new PointF(0, 0), mass, Color.Green));

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
                    if (ball.Delay > 0)
                    {
                        ball.Delay -= 0.032f;
                        continue;
                    }
                    else
                    {
                        ball.IsDelayed = false;
                    }

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
                            if (target.Delay > 0)
                            {
                                target.Delay = 0;
                                target.IsDelayed = false;
                            }

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

                if (!simulationStarted)
                {
                    PointF center = new PointF(ball.PosX + ball.Radius, ball.PosY + ball.Radius);
                    PointF lineEnd = new PointF(
                        center.X + ball.Radius * 2 * (float)Math.Cos(ball.Angle),
                        center.Y + ball.Radius * 2 * (float)Math.Sin(ball.Angle)
                    );
                    g.DrawLine(p, center, lineEnd);
                }
            }

            // Rysowanie linii miêdzy kulkami w przypadku kolizji
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
                // wartoœæ do zakresu 1-50
                newMass = Math.Clamp(newMass, 1, 50);

                if (newMass.ToString() != textBoxesMass[index].Text)
                {
                    textBoxesMass[index].Text = newMass.ToString();
                }

                balls[index].Mass = newMass;
            }
        }

        private void textBoxRadius_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesRadius, sender as TextBox);
            if (index >= 0 && int.TryParse(textBoxesRadius[index].Text, out int newRadius))
            {
                // wartoœæ do zakresu 1-50
                newRadius = Math.Clamp(newRadius, 1, 50);

                if (newRadius.ToString() != textBoxesRadius[index].Text)
                {
                    textBoxesRadius[index].Text = newRadius.ToString();
                }

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
                    // wartoœci sk³adowych przyspieszenia do zakresu 1-50
                    ax = Math.Clamp(ax, 1, 50);
                    ay = Math.Clamp(ay, 1, 50);

                    string newText = $"{ax},{ay}";
                    if (newText != textBoxesAcceleration[index].Text)
                    {
                        textBoxesAcceleration[index].Text = newText;
                    }

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
                    // wartoœci sk³adowych prêdkoœci do zakresu 1-50
                    vx = Math.Clamp(vx, 1, 50);
                    vy = Math.Clamp(vy, 1, 50);

                    string newText = $"{vx},{vy}";
                    if (newText != textBoxesVelocity[index].Text)
                    {
                        textBoxesVelocity[index].Text = newText;
                    }

                    balls[index].Velocity = new PointF(vx, vy);
                }
            }
        }

        private void textBoxDelay_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesDelay, sender as TextBox);
            if (index >= 0 && float.TryParse(textBoxesDelay[index].Text, out float delay))
            {
                delay = Math.Clamp(delay, 1, 50);

                if (delay.ToString() != textBoxesDelay[index].Text)
                {
                    textBoxesDelay[index].Text = delay.ToString();
                }
                balls[index].Delay = delay;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                if (float.TryParse(textBoxesDelay[i].Text, out float delay))
                {
                    balls[i].Delay = delay;
                    balls[i].IsDelayed = true;
                }
                else
                {
                    balls[i].Delay = 0;
                    balls[i].IsDelayed = false;
                }
            }

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
                    if (ball.Contains(new PointF(e.X, e.Y)))
                    {
                        draggedBall = ball;
                        dragOffset = new PointF(e.X - ball.PosX, e.Y - ball.PosY);
                        break;
                    }
                }

                foreach (Ball ball in balls)
                {
                    PointF center = new PointF(ball.PosX + ball.Radius, ball.PosY + ball.Radius);
                    PointF lineEnd = new PointF(
                        center.X + ball.Radius * 2 * (float)Math.Cos(ball.Angle),
                        center.Y + ball.Radius * 2 * (float)Math.Sin(ball.Angle)
                    );

                    if (IsPointOnLine(center, lineEnd, new PointF(e.X, e.Y), 5))
                    {
                        rotatingLineBall = ball;
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

            if (rotatingLineBall != null && !simulationStarted)
            {
                PointF center = new PointF(rotatingLineBall.PosX + rotatingLineBall.Radius, rotatingLineBall.PosY + rotatingLineBall.Radius);
                float deltaX = e.X - center.X;
                float deltaY = e.Y - center.Y;
                rotatingLineBall.Angle = (float)Math.Atan2(deltaY, deltaX);

                float speed = (float)Math.Sqrt(rotatingLineBall.Velocity.X * rotatingLineBall.Velocity.X + rotatingLineBall.Velocity.Y * rotatingLineBall.Velocity.Y);
                rotatingLineBall.Velocity = new PointF(
                    speed * (float)Math.Cos(rotatingLineBall.Angle),
                    speed * (float)Math.Sin(rotatingLineBall.Angle)
                );

                mainArea.Refresh();
            }
        }

        private void MainArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draggedBall = null;
                rotatingLineBall = null;
            }
        }

        private bool IsPointOnLine(PointF start, PointF end, PointF point, float tolerance)
        {
            float minX = Math.Min(start.X, end.X) - tolerance;
            float maxX = Math.Max(start.X, end.X) + tolerance;
            float minY = Math.Min(start.Y, end.Y) - tolerance;
            float maxY = Math.Max(start.Y, end.Y) + tolerance;

            if (point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY)
                return false;

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float distance = Math.Abs((end.X - start.X) * (start.Y - point.Y) - (start.X - point.X) * (end.Y - start.Y)) / length;

            return distance <= tolerance;
        }
    }
}