using System;
using System.Drawing;

namespace Circle_Collision.Classes
{
    class Ball
    {
        private float posX, posY, radius;
        private int id;
        private float mass;
        private float acceleration;
        private float speed;
        private float direction; //w radianach
        private RectangleF ball;
        private Color color;

        public Ball(int id, float posX, float posY, int radius, float speed, float direction, float acceleration, float mass, Color color)
        {
            Id = id;
            PosX = posX;
            PosY = posY;
            Radius = radius;
            Speed = speed;
            Direction = direction;
            Acceleration = acceleration;
            Mass = mass;
            Color = color;
            Delay = 0;
            IsDelayed = true;
        }

        public int Id { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public int Radius { get; set; }
        public float Speed { get; set; } 
        public float Acceleration { get; set; }
        public float Direction { get; set; } 
        public float Mass { get; set; }
        public Color Color { get; set; }
        public float Delay { get; set; }
        public bool IsDelayed { get; set; }
        public RectangleF BallToDraw => new RectangleF(PosX, PosY, Radius * 2, Radius * 2);

        public bool Contains(PointF point)
        {
            return BallToDraw.Contains(point);
        }
    }
}