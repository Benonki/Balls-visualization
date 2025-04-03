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

            balls.Add(new Ball(0, 50, 100, radius, 2, (float)Math.PI / 2, 0, mass, Color.Red));
            balls.Add(new Ball(1, 300, 100, radius, 2, (float)(-Math.PI / 2), 0, mass, Color.Blue));
            balls.Add(new Ball(2, 175, 250, radius, 2, (float)(-Math.PI / 2), 0, mass, Color.Green));

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
        }

        private void DetectCollisions()
        {
            collidingPairs.Clear();
            var sortedBalls = balls.OrderByDescending(b => b.Radius).ToList();
            for (int i = 0; i < sortedBalls.Count; i++)
            {
                for (int j = i + 1; j < sortedBalls.Count; j++)
                {
                    Ball largerBall = sortedBalls[i];
                    Ball smallerBall = sortedBalls[j];

                    // Pomijaj kolizje jeœli obie pi³ki maj¹ opóŸnienie
                    if (largerBall.IsDelayed && smallerBall.IsDelayed) continue;

                    float dx = (smallerBall.PosX + smallerBall.Radius) - (largerBall.PosX + largerBall.Radius);
                    float dy = (smallerBall.PosY + smallerBall.Radius) - (largerBall.PosY + largerBall.Radius);
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    float minDistance = largerBall.Radius + smallerBall.Radius;
                    bool isTinyBall = smallerBall.Radius < largerBall.Radius;
                    float collisionThreshold = isTinyBall ? largerBall.Radius * 0.8f : minDistance;

                    if (distance < collisionThreshold)
                    {
                        // Jeœli któraœ z pi³ek mia³a opóŸnienie, usuñ je
                        if (largerBall.IsDelayed)
                        {
                            largerBall.Delay = 0;
                            largerBall.IsDelayed = false;
                        }
                        if (smallerBall.IsDelayed)
                        {
                            smallerBall.Delay = 0;
                            smallerBall.IsDelayed = false;
                        }

                        collidingPairs.Add(new Tuple<Ball, Ball>(largerBall, smallerBall));

                        if (isTinyBall)
                        {
                            HandleTinyBallCollision(smallerBall, largerBall, dx, dy, distance);
                        }
                        else
                        {
                            HandleStandardCollision(largerBall, smallerBall, dx, dy, distance);
                        }

                        float overlap = collisionThreshold - distance;
                        if (overlap > 0)
                        {
                            float moveX = (dx / distance) * overlap * 0.5f;
                            float moveY = (dy / distance) * overlap * 0.5f;

                            smallerBall.PosX += moveX * 2f;
                            smallerBall.PosY += moveY * 2f;
                            largerBall.PosX -= moveX * 0.5f;
                            largerBall.PosY -= moveY * 0.5f;
                        }
                    }
                }
            }
        }

        private void HandleTinyBallCollision(Ball tinyBall, Ball largeBall, float dx, float dy, float distance)
        {
            //Oblicz normalizowany wektor kolizji
            float nx = dx / distance;
            float ny = dy / distance;

            //Oblicz g³êbokoœæ penetracji (jak bardzo kulki siê nak³adaj¹)
            float penetrationDepth = (largeBall.Radius * 0.85f + tinyBall.Radius) - distance;
            if (penetrationDepth <= 0) return;

            //P³ynne wypchniêcie (zamiast natychmiastowej korekty)
            float pushFactor = 0.2f; // Im mniejsza wartoœæ, tym p³ynniejsze wypychanie
            float pushX = nx * penetrationDepth * pushFactor;
            float pushY = ny * penetrationDepth * pushFactor;

            //Zastosuj wypchniêcie proporcjonalnie do mas
            tinyBall.PosX += pushX * (largeBall.Mass / (tinyBall.Mass + largeBall.Mass)) * 2f;
            tinyBall.PosY += pushY * (largeBall.Mass / (tinyBall.Mass + largeBall.Mass)) * 2f;
            largeBall.PosX -= pushX * (tinyBall.Mass / (tinyBall.Mass + largeBall.Mass));
            largeBall.PosY -= pushY * (tinyBall.Mass / (tinyBall.Mass + largeBall.Mass));

            //Oblicz prêdkoœci wzglêdne
            float tvx = tinyBall.Speed * (float)Math.Cos(tinyBall.Direction);
            float tvy = tinyBall.Speed * (float)Math.Sin(tinyBall.Direction);
            float lvx = largeBall.Speed * (float)Math.Cos(largeBall.Direction);
            float lvy = largeBall.Speed * (float)Math.Sin(largeBall.Direction);

            //Oblicz sk³adow¹ prêdkoœci wzd³u¿ normalnej
            float velocityAlongNormal = (tvx - lvx) * nx + (tvy - lvy) * ny;

            //Jeœli kulki oddalaj¹ siê od siebie, nie wykonuj odbicia
            if (velocityAlongNormal > 0) return;

            //Oblicz impuls (z fizyki kolizji)
            float restitution = 0.6f; // Wspó³czynnik odbicia
            float j = -(1 + restitution) * velocityAlongNormal;
            j /= (1 / tinyBall.Mass + 1 / largeBall.Mass);

            //Zastosuj impuls do prêdkoœci
            tvx += (j * nx) / tinyBall.Mass;
            tvy += (j * ny) / tinyBall.Mass;
            lvx -= (j * nx) / largeBall.Mass;
            lvy -= (j * ny) / largeBall.Mass;

            //Aktualizuj prêdkoœci i kierunki
            tinyBall.Speed = (float)Math.Sqrt(tvx * tvx + tvy * tvy);
            tinyBall.Direction = (float)Math.Atan2(tvy, tvx);
            largeBall.Speed = (float)Math.Sqrt(lvx * lvx + lvy * lvy);
            largeBall.Direction = (float)Math.Atan2(lvy, lvx);

            //Delikatne t³umienie dla stabilnoœci
            tinyBall.Speed *= 0.97f;
            largeBall.Speed *= 0.99f;
        }

        private void HandleStandardCollision(Ball ball1, Ball ball2, float dx, float dy, float distance)
        {
            // Normalizowany wektor kolizji
            float nx = dx / distance;
            float ny = dy / distance;

            // Sk³adowe prêdkoœci
            float v1x = ball1.Speed * (float)Math.Cos(ball1.Direction);
            float v1y = ball1.Speed * (float)Math.Sin(ball1.Direction);
            float v2x = ball2.Speed * (float)Math.Cos(ball2.Direction);
            float v2y = ball2.Speed * (float)Math.Sin(ball2.Direction);

            // Iloczyn skalarny prêdkoœci i wektora normalnego
            float v1n = v1x * nx + v1y * ny;
            float v2n = v2x * nx + v2y * ny;

            // Obliczenie nowych prêdkoœci po kolizji
            float v1nAfter = (v1n * (ball1.Mass - ball2.Mass) + 2 * ball2.Mass * v2n) / (ball1.Mass + ball2.Mass);
            float v2nAfter = (v2n * (ball2.Mass - ball1.Mass) + 2 * ball1.Mass * v1n) / (ball1.Mass + ball2.Mass);

            // Aktualizacja prêdkoœci
            v1x += (v1nAfter - v1n) * nx;
            v1y += (v1nAfter - v1n) * ny;
            v2x += (v2nAfter - v2n) * nx;
            v2y += (v2nAfter - v2n) * ny;

            // Ustawienie nowych prêdkoœci i kierunków
            ball1.Speed = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
            ball1.Direction = (float)Math.Atan2(v1y, v1x);
            ball2.Speed = (float)Math.Sqrt(v2x * v2x + v2y * v2y);
            ball2.Direction = (float)Math.Atan2(v2y, v2x);
        }

        private void mainArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p = new Pen(Color.FromArgb(244, 252, 19)) { Width = 5.0f };
            Pen blackPen = new Pen(Brushes.Black) { Width = 1.5f };

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
                        center.X + ball.Radius * 2 * (float)Math.Cos(ball.Direction),
                        center.Y + ball.Radius * 2 * (float)Math.Sin(ball.Direction)
                    );
                    g.DrawLine(p, center, lineEnd);
                }
            }

            //foreach (var tuple in collidingPairs)
            //{
            //    g.DrawLine(p, new PointF(tuple.Item1.PosX + tuple.Item1.Radius, tuple.Item1.PosY + tuple.Item1.Radius),
            //                  new PointF(tuple.Item2.PosX + tuple.Item2.Radius, tuple.Item2.PosY + tuple.Item2.Radius));
            //}
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