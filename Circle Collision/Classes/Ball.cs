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

        public Ball(int id, float posX, float posY, int radius, PointF velocity, PointF acceleration, float mass, Color color)
        {
            PosX = posX;
            PosY = posY;
            Radius = radius;
            Mass = mass;
            Id = id;
            Color = color;
            Angle = (float)Math.Atan2(velocity.Y, velocity.X);
            Velocity = velocity;
            Acceleration = acceleration;
            Delay = 0;
            IsDelayed = true;
        }

        public int Id { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public int Radius { get; set; }
        public PointF Velocity { get; set; }
        public PointF Acceleration { get; set; }
        public float Mass { get; set; }
        public Color Color { get; set; }
        public float Angle { get; set; }
        public float Delay { get; set; }  
        public bool IsDelayed { get; set; }
        public RectangleF BallToDraw => new RectangleF(PosX, PosY, Radius * 2, Radius * 2);

        public bool Contains(PointF point)
        {
            return BallToDraw.Contains(point);
        }
    }
}