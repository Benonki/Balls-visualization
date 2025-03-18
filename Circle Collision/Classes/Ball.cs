using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circle_Collision.Classes
{
    class Ball
    {
        private float posX, posY, radius;
        int id;
        private float mass;
        private PointF acceleration;
        private PointF velocity;
        private RectangleF ball;

        public Ball(int id, float posX, float posY, float radius, PointF velocity, PointF acceleration, float mass)
        {
            PosX = posX;
            PosY = posY;
            Radius = radius;
            Velocity = velocity;
            Acceleration = acceleration;
            Mass = mass;
            Id = id;
            BallToDraw = new RectangleF(PosX, PosY, Radius * 2, Radius * 2);
        }


        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public float PosX
        {
            get { return posX; }
            set { posX = value; }
        }

        public float PosY
        {
            get { return posY; }
            set { posY = value; }
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }
        public PointF Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public PointF Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public RectangleF BallToDraw
        {
            get { return ball; }
            set { ball = value; }
        }

        public void UpdateBall()
        {
            BallToDraw = new RectangleF(PosX, PosY, Radius * 2, Radius * 2);
        }







    }
}
