using System;
using System.Drawing;

namespace Circle_Collision.Classes
{
    class Ball
    {
        private float posX, posY, radius;
        private int id;
        private float mass;
        private PointF acceleration;
        private PointF velocity;
        private RectangleF ball;
        private Color color; 

        public Ball(int id, float posX, float posY, float radius, PointF velocity, PointF acceleration, float mass, Color color)
        {
            PosX = posX;
            PosY = posY;
            Radius = radius;
            Velocity = velocity;
            Acceleration = acceleration;
            Mass = mass;
            Id = id;
            Color = color;
            BallToDraw = new RectangleF(PosX, PosY, Radius * 2, Radius * 2);
        }

        public float Mass { get; set; }
        public int Id { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Radius { get; set; }
        public PointF Velocity { get; set; }
        public PointF Acceleration { get; set; }
        public RectangleF BallToDraw { get; set; }
        public Color Color { get; set; } 

        public void UpdateBall()
        {
            BallToDraw = new RectangleF(PosX, PosY, Radius * 2, Radius * 2);
        }

        public bool Contains(PointF point)
        {
            float dx = point.X - (PosX + Radius);
            float dy = point.Y - (PosY + Radius);
            return Math.Sqrt(dx * dx + dy * dy) <= Radius;
        }
    }
}