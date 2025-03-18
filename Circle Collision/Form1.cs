
using Circle_Collision.Classes;

namespace Circle_Collision
{
    public partial class Form1 : Form
    {
        private int radius = 40;
        private float mass = 10;
        private List<Ball> balls = new List<Ball>();
        private List<Tuple<Ball, Ball>> collidingPairs = new List<Tuple<Ball, Ball>>();
        private System.Windows.Forms.Timer moveTimer;
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            mainArea.Paint += new PaintEventHandler(mainArea_Paint);
            System.Console.WriteLine("AAAAAAAAAAAA");
            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 16;
            moveTimer.Tick += MoveBalls;
            moveTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void addBallBtn_Click(object sender, EventArgs e)
        {
           
            float posX = new Random().Next(10, mainArea.Width - radius * 2);
            float posY = new Random().Next(10, mainArea.Height - radius * 2);
            float speedX = new Random().Next(-5, 5);
            float speedY = new Random().Next(-5, 5);
            PointF acceleration = new Point(0,0);
            PointF velocity = new PointF(speedX == 0 ? 1 : speedX, speedY == 0 ? 1 : speedY);
            balls.Add(new Ball(balls.Count, posX, posY, radius, velocity, acceleration, mass));
            
            /* Testowe wartoœci satyczne
            Point velocity = new Point(1,0);
            balls.Add(new Ball(balls.Count, 10, 100, radius, velocity, acceleration,mass * 10));
            Point velocity2 = new Point(0, 0);
            balls.Add(new Ball(balls.Count, 500, 100, radius, velocity2, acceleration, mass));
            */

        }

        private void MoveBalls (object sender, EventArgs e)
        {
            for(int i = 0; i < balls.Count; i++)
            {
                Ball ball = balls[i];

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

                ball.UpdateBall();

            }

            var BallsOverlap = (Ball ball, Ball target) =>
            {
                double dx = (ball.PosX + ball.Radius) - (target.PosX + target.Radius);
                double dy = (ball.PosY + ball.Radius) - (target.PosY + target.Radius);
                double distance = Math.Sqrt(dx * dx + dy * dy);
                return distance <= ball.Radius  + target.Radius;
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
                            // Adding to the list of colliding balls
                            collidingPairs.Add(new Tuple<Ball, Ball>(ball, target));
                        }

                        if (!isColliding && collidingPairs.Any(tuple => (tuple.Item1 == ball && tuple.Item2 == target ||
                                                                        tuple.Item1 == target && tuple.Item2 == ball)))
                        {
                            pairsToRemove.Add(new Tuple<Ball, Ball>(ball, target));
                        }

                        if (isColliding)
                        {
                            // Distance between the ball centers
                            double dx = (ball.PosX + ball.Radius) - (target.PosX + target.Radius);
                            double dy = (ball.PosY + ball.Radius) - (target.PosY + target.Radius);
                            double distance = Math.Sqrt(dx * dx + dy * dy);

                            float overlap = 0.5f * ((float)distance - ball.Radius - target.Radius);


                            // Displace current ball
                            ball.PosX -= overlap * (ball.PosX - target.PosX) / (float)distance;
                            ball.PosY -= overlap * (ball.PosY - target.PosY) / (float)distance;

                            // Displace target ball
                            target.PosX += overlap * (ball.PosX - target.PosX) / (float)distance;
                            target.PosY += overlap * (ball.PosY - target.PosY) / (float)distance;

                            HandleCollision(ball, target);
                            
                        }
                    }
                }

                foreach(var pair in pairsToRemove)
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

            // Wektor normalny
            float nx = (ball2.PosX - ball1.PosX) / (float)distance;
            float ny = (ball2.PosY - ball1.PosY) / (float)distance;

            //Tangens 
            float tx = -ny;
            float ty = nx;

            // Iloczyn skalarny

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
            Pen p = new Pen(Color.FromArgb(199, 2, 18));
            Pen blackPen = new Pen(Brushes.Black);
            blackPen.Width = 1.5f;
            p.Width = 5.0f;
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(24, 171, 102)))
            {
                foreach(Ball ball in balls)
                {
                    g.FillEllipse(sb,ball.BallToDraw);
                    g.DrawEllipse(blackPen, ball.BallToDraw);
                }

                foreach(var tuple in collidingPairs)
                {
                    g.DrawLine(p, new PointF(tuple.Item1.PosX + tuple.Item1.Radius ,tuple.Item1.PosY + tuple.Item1.Radius), new PointF(tuple.Item2.PosX + tuple.Item2.Radius, tuple.Item2.PosY + tuple.Item2.Radius));
                }
            }
        }
    }
}
