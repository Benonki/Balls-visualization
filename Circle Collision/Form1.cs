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
                Pen borderPen = new Pen(Color.FromArgb(244, 252, 19), 1);
                g.DrawRectangle(borderPen, 0, 0, mainArea.Width, mainArea.Height);
            };


            balls.Add(new Ball(0, 50, 100, radius, 2, (float)Math.PI / 2, 0, mass, Color.Red));
            balls.Add(new Ball(1, 300, 100, radius, 2, (float)(-Math.PI / 2), 0, mass, Color.Blue));
            balls.Add(new Ball(2, 175, 250, radius, 2, (float)(-Math.PI / 2), 0, mass, Color.Green));

            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 16; // ~60 FPS
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

                    DetectCollisions();

                    // Aktualizacja prêdkoœci
                    ball.Speed += ball.Acceleration * 0.16f;

                    // Aktualizacja pozycji na podstawie prêdkoœci i kierunku
                    ball.PosX += ball.Speed * (float)Math.Cos(ball.Direction);
                    ball.PosY += ball.Speed * (float)Math.Sin(ball.Direction);

                    // Odbicie od œcian
                    if (ball.PosX <= 0 || ball.PosX + ball.Radius * 2 >= mainArea.Width)
                        ball.Direction = (float)Math.PI - ball.Direction;

                    if (ball.PosY <= 0 || ball.PosY + ball.Radius * 2 >= mainArea.Height)
                        ball.Direction = -ball.Direction;

                    // Upewnienie siê, ¿e pi³ka nie wychodzi poza obszar
                    ball.PosX = Math.Clamp(ball.PosX, 0, mainArea.Width - ball.Radius * 2);
                    ball.PosY = Math.Clamp(ball.PosY, 0, mainArea.Height - ball.Radius * 2);
                }
            }
            mainArea.Refresh();
            sideViewArea.Refresh();
        }

        private void DetectCollisions()
        {
            collidingPairs.Clear();

            for (int i = 0; i < balls.Count; i++)
            {
                for (int k = 0; k < balls.Count; k++) 
                {
                    if (i == k) continue;  // Pomijamy kolizjê kuli samej ze sob¹

                    Ball ball1 = balls[i];
                    Ball ball2 = balls[k];

                    if (ball1.IsDelayed && ball2.IsDelayed) continue;

                    // Obliczenia pozycji 3D
                    float ball1CenterX = ball1.PosX + ball1.Radius;
                    float ball1CenterY = ball1.PosY + ball1.Radius;
                    float ball1CenterZ = ball1.Radius;

                    float ball2CenterX = ball2.PosX + ball2.Radius;
                    float ball2CenterY = ball2.PosY + ball2.Radius;
                    float ball2CenterZ = ball2.Radius;

                    float dx = ball2CenterX - ball1CenterX;
                    float dy = ball2CenterY - ball1CenterY;
                    float dz = ball2CenterZ - ball1CenterZ;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

                    float minDistance = ball1.Radius + ball2.Radius;

                    if (distance < minDistance)
                    {
                        if (ball1.IsDelayed)
                        {
                            ball1.Delay = 0;
                            ball1.IsDelayed = false;
                        }
                        if (ball2.IsDelayed)
                        {
                            ball2.Delay = 0;
                            ball2.IsDelayed = false;
                        }

                        collidingPairs.Add(new Tuple<Ball, Ball>(ball1, ball2));

                        // Wektor normalny kolizji
                        float nx = dx / distance;
                        float ny = dy / distance;
                        float nz = dz / distance;

                        // Korekta pozycji
                        float overlap = minDistance - distance;
                        if (overlap > 0)
                        {
                            float moveX = nx * overlap * 0.5f;
                            float moveY = ny * overlap * 0.5f;

                            ball1.PosX -= moveX;
                            ball1.PosY -= moveY;
                            ball2.PosX += moveX;
                            ball2.PosY += moveY;
                        }

                        // Obliczenia prêdkoœci
                        float v1x = ball1.Speed * (float)Math.Cos(ball1.Direction);
                        float v1y = ball1.Speed * (float)Math.Sin(ball1.Direction);

                        float v2x = ball2.Speed * (float)Math.Cos(ball2.Direction);
                        float v2y = ball2.Speed * (float)Math.Sin(ball2.Direction);

                        // Prêdkoœæ wzglêdna
                        float vrelX = v2x - v1x;
                        float vrelY = v2y - v1y;

                        // Sk³adowa normalna
                        float vrelNormal = vrelX * nx + vrelY * ny;

                        if (vrelNormal > 0) continue;

                        // Wspó³czynnik restytucji
                        float restitution = 0.8f;

                        // Impuls kolizji
                        float impulse = -(1 + restitution) * vrelNormal;
                        impulse /= (1 / ball1.Mass + 1 / ball2.Mass);

                        // Zmiana prêdkoœci
                        v1x -= impulse * nx / ball1.Mass;
                        v1y -= impulse * ny / ball1.Mass;

                        v2x += impulse * nx / ball2.Mass;
                        v2y += impulse * ny / ball2.Mass;

                        // Aktualizacja prêdkoœci i kierunku
                        ball1.Speed = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
                        ball1.Direction = (float)Math.Atan2(v1y, v1x);

                        ball2.Speed = (float)Math.Sqrt(v2x * v2x + v2y * v2y);
                        ball2.Direction = (float)Math.Atan2(v2y, v2x);

                        // T³umienie
                        ball1.Speed *= 0.98f;
                        ball2.Speed *= 0.98f;
                    }
                }
            }
        }

        private void mainArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p = new Pen(Color.FromArgb(244, 252, 19)) { Width = 5.0f };
            Pen blackPen = new Pen(Brushes.Black) { Width = 1.5f };

            var sortedBalls = balls.OrderBy(b => b.Radius).ToList();
            foreach (Ball ball in sortedBalls)
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
                            center.X + ball.Radius * 2 * (float)Math.Cos(ball.Direction),
                            center.Y + ball.Radius * 2 * (float)Math.Sin(ball.Direction)
                        );
                        g.DrawLine(p, center, lineEnd);
                        float arrowSize = ball.Radius * 0.5f;
                        float angle = (float)Math.Atan2(lineEnd.Y - center.Y, lineEnd.X - center.X); 
                        PointF arrowPoint1 = new PointF(
                            lineEnd.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                            lineEnd.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6)
                        );
                        PointF arrowPoint2 = new PointF(
                            lineEnd.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                            lineEnd.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6)
                        );

                        g.DrawLine(p, lineEnd, arrowPoint1);
                        g.DrawLine(p, lineEnd, arrowPoint2);
                }
                }

            //foreach (var tuple in collidingPairs)
            //{
            //    g.DrawLine(p, new PointF(tuple.Item1.PosX + tuple.Item1.Radius, tuple.Item1.PosY + tuple.Item1.Radius),
            //                  new PointF(tuple.Item2.PosX + tuple.Item2.Radius, tuple.Item2.PosY + tuple.Item2.Radius));
            //}
        }

        private void sideViewArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            // widok z boku
            Pen borderPen = new Pen(Color.FromArgb(244, 252, 19), 1);
            g.DrawRectangle(borderPen, 0, 0, sideViewArea.Width - 1, sideViewArea.Height - 1);

            // rysowanie podlogi
            float floorLevel = sideViewArea.Height - 2;
            using (Pen floorPen = new Pen(Color.FromArgb(244, 252, 19), 3))
            {
                g.DrawLine(floorPen, 0, floorLevel, sideViewArea.Width, floorLevel);
            }

            // sortujemy po pozycji x
            var sortedBalls = balls.OrderBy(b => b.PosX).ToList();

            foreach (Ball ball in sortedBalls)
            {
                // obliczamy rozmiar - im blizej tym wieksza
                float depthFactor = 0.5f + 0.5f * (ball.PosX / mainArea.Width);
                float baseSize = ball.Radius * 2.5f;
                float scaledWidth = baseSize * depthFactor;
                float scaledHeight = baseSize * depthFactor;


                // pozycja na podlodze
                float ballBottom = floorLevel - 2;
                float sideViewY = ballBottom - scaledHeight;

                // obliczanie pozycji X z widoku z boku
                float effectiveMainHeight = mainArea.Height - ball.Radius * 2;
                float normalizedY = (ball.PosY) / effectiveMainHeight;
                float sideViewX = scaledWidth / 2 + normalizedY * (sideViewArea.Width - scaledWidth);

                // rysowanie kulki
                using (SolidBrush sb = new SolidBrush(ball.Color))
                {
                    g.FillEllipse(sb, sideViewX - scaledWidth / 2, sideViewY, scaledWidth, scaledHeight);
                }

                // obramówki wokó³ kulki
                g.DrawEllipse(Pens.White, sideViewX - scaledWidth / 2, sideViewY, scaledWidth, scaledHeight);
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

        private void textBoxRadius_Leave(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesRadius, sender as TextBox);
            if (index >= 0 && int.TryParse(textBoxesRadius[index].Text, out int newRadius))
            {
                // wartoœæ do zakresu 10-70
                newRadius = Math.Clamp(newRadius, 10, 70);
                textBoxesRadius[index].Text = newRadius.ToString();
                balls[index].Radius = newRadius;
            }
            else if (!string.IsNullOrEmpty(textBoxesRadius[index].Text))
            {
                textBoxesRadius[index].Text = balls[index].Radius.ToString();
            }
        }

        private void textBoxVelocity_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesVelocity, sender as TextBox);
            if (index >= 0 && !string.IsNullOrWhiteSpace(textBoxesVelocity[index].Text))
            {
                // wartoœæ do zakresu 1-50 
                if (float.TryParse(textBoxesVelocity[index].Text, out float speed))
                {
                    speed = Math.Clamp(speed, 1, 50);
                    balls[index].Speed = speed;

                    textBoxesVelocity[index].Text = speed.ToString();
                }
            }
        }

        private void textBoxDelay_TextChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(textBoxesDelay, sender as TextBox);
            if (index >= 0 && float.TryParse(textBoxesDelay[index].Text, out float delay))
            {
                delay = Math.Clamp(delay, 0, 50);

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
                        center.X + ball.Radius * 2 * (float)Math.Cos(ball.Direction),
                        center.Y + ball.Radius * 2 * (float)Math.Sin(ball.Direction)
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
                draggedBall.PosX = Math.Clamp(e.X - dragOffset.X, 0, mainArea.Width - draggedBall.Radius * 2);
                draggedBall.PosY = Math.Clamp(e.Y - dragOffset.Y, 0, mainArea.Height - draggedBall.Radius * 2);

                mainArea.Refresh();
            }

            if (rotatingLineBall != null && !simulationStarted)
            {
                PointF center = new PointF(rotatingLineBall.PosX + rotatingLineBall.Radius, rotatingLineBall.PosY + rotatingLineBall.Radius);
                float deltaX = e.X - center.X;
                float deltaY = e.Y - center.Y;
                rotatingLineBall.Direction = (float)Math.Atan2(deltaY, deltaX);

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