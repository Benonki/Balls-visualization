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
            var sortedBalls = balls.OrderByDescending(b => b.Radius)
                                  .ToList();

            for (int i = 0; i < sortedBalls.Count; i++)
            {
                for (int j = i + 1; j < sortedBalls.Count; j++)
                {
                    Ball largerBall = sortedBalls[i];
                    Ball smallerBall = sortedBalls[j];

                    if (largerBall.IsDelayed && smallerBall.IsDelayed) continue;

                    float dx = (smallerBall.PosX + smallerBall.Radius) - (largerBall.PosX + largerBall.Radius);
                    float dy = (smallerBall.PosY + smallerBall.Radius) - (largerBall.PosY + largerBall.Radius);
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    float minDistance = largerBall.Radius + smallerBall.Radius;
                    bool isTinyBall = smallerBall.Radius < largerBall.Radius;
                    float collisionThreshold = isTinyBall ? largerBall.Radius * 0.8f : minDistance;

                    if (distance < collisionThreshold)
                    {
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

                        bool isActuallyBelow = (smallerBall.PosY + smallerBall.Radius) > (largerBall.PosY + largerBall.Radius);
                        if (isTinyBall && isActuallyBelow)
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
            float nx = dx / distance;  // Normalizowany wektor kolizji X
            float ny = dy / distance;  // Normalizowany wektor kolizji Y

            // Rzeczywista ró¿nica pozycji (uwzglêdniaj¹ca ró¿ne promienie)
            float penetrationDepth = (tinyBall.Radius + largeBall.Radius) - distance;
            if (penetrationDepth <= 0) return;

            // Obliczanie odpowiednich przesuniêæ, które zmieniaj¹ pozycjê pi³ek
            float moveAmount = penetrationDepth * 0.5f;  // Proporcjonalne przesuniêcie, aby unikn¹æ "teleportacji"

            // Wypchniêcie pi³ek tak, by nie zachodzi³y na siebie
            tinyBall.PosX += nx * moveAmount * (largeBall.Mass / (tinyBall.Mass + largeBall.Mass));
            tinyBall.PosY += ny * moveAmount * (largeBall.Mass / (tinyBall.Mass + largeBall.Mass));
            largeBall.PosX -= nx * moveAmount * (tinyBall.Mass / (tinyBall.Mass + largeBall.Mass));
            largeBall.PosY -= ny * moveAmount * (tinyBall.Mass / (tinyBall.Mass + largeBall.Mass));

            // Obliczenie prêdkoœci przed kolizj¹
            float tinyVelX = tinyBall.Speed * (float)Math.Cos(tinyBall.Direction);
            float tinyVelY = tinyBall.Speed * (float)Math.Sin(tinyBall.Direction);
            float largeVelX = largeBall.Speed * (float)Math.Cos(largeBall.Direction);
            float largeVelY = largeBall.Speed * (float)Math.Sin(largeBall.Direction);

            // Prêdkoœæ wzglêdna wzd³u¿ normalnej
            float relativeVel = (tinyVelX - largeVelX) * nx + (tinyVelY - largeVelY) * ny;

            if (relativeVel > 0) return; // Jeœli kulki ju¿ siê oddalaj¹, nie zmieniamy nic

            // Wspó³czynnik sprê¿ystoœci (jak bardzo odbicie jest elastyczne)
            float restitution = 0.8f;

            // Impuls kolizji
            float j = -(1 + restitution) * relativeVel;
            j /= (1 / tinyBall.Mass + 1 / largeBall.Mass);

            // Nowe prêdkoœci po odbiciu
            tinyVelX += (j * nx) / tinyBall.Mass;
            tinyVelY += (j * ny) / tinyBall.Mass;
            largeVelX -= (j * nx) / largeBall.Mass;
            largeVelY -= (j * ny) / largeBall.Mass;

            // Aktualizacja kierunku i prêdkoœci
            tinyBall.Speed = (float)Math.Sqrt(tinyVelX * tinyVelX + tinyVelY * tinyVelY);
            tinyBall.Direction = (float)Math.Atan2(tinyVelY, tinyVelX);
            largeBall.Speed = (float)Math.Sqrt(largeVelX * largeVelX + largeVelY * largeVelY);
            largeBall.Direction = (float)Math.Atan2(largeVelY, largeVelX);

            // Stabilizowanie drgañ
            tinyBall.Speed *= 0.98f;
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
                float scaledWidth = baseSize * depthFactor * 1.2f;
                float scaledHeight = baseSize * depthFactor * 1.2f;

                // Minimalne rozmiary pilek
                scaledWidth = Math.Max(scaledWidth, 10);
                scaledHeight = Math.Max(scaledHeight, 10);

                // pozycja na podlodze
                float ballBottom = floorLevel - 2;
                float sideViewY = ballBottom - scaledHeight;

                // obliczanie pozycji X z widoku z boku
                float effectiveMainHeight = mainArea.Height - ball.Radius * 2;
                float normalizedY = (ball.PosY) / effectiveMainHeight;
                float sideViewX = scaledWidth / 2 + normalizedY * (sideViewArea.Width - scaledWidth);

                // cieñ
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(80, 50, 50, 50)))
                {
                    float shadowWidth = scaledWidth * 1.4f;
                    float shadowHeight = scaledHeight * 0.4f;
                    g.FillEllipse(shadowBrush,
                                sideViewX - shadowWidth / 2,
                                floorLevel - shadowHeight / 3,
                                shadowWidth,
                                shadowHeight);
                }

                // rysowanie kulki
                using (SolidBrush sb = new SolidBrush(ball.Color))
                {
                    g.FillEllipse(sb, sideViewX - scaledWidth / 2, sideViewY, scaledWidth, scaledHeight);
                }

                // Draw obramówki wokó³ kulki
                g.DrawEllipse(Pens.White, sideViewX - scaledWidth / 2, sideViewY, scaledWidth, scaledHeight);

                // 3D 3ffect
                using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(80, Color.White)))
                {
                    g.FillEllipse(highlightBrush,
                                sideViewX - scaledWidth / 4,
                                sideViewY + scaledHeight / 5,
                                scaledWidth / 2,
                                scaledHeight / 3);
                }
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
                // wartoœæ do zakresu 10-50
                newRadius = Math.Clamp(newRadius, 10, 50);

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