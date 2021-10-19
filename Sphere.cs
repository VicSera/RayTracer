using System;

namespace rt
{
    public class Sphere : Geometry
    {
        private Vector Center { get; set; }
        private double Radius { get; set; }

        public Sphere(Vector center, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            var a = line.Dx * line.Dx;
            var b = 2 * (line.Dx * (line.X0 - Center));
            var c = (line.X0 - Center) * (line.X0 - Center) - Radius * Radius;
            
            var determinant = b * b - 4 * a * c;

            if (determinant < 0)
                return new Intersection();
            
            var d1 = (-b + Math.Sqrt(determinant)) / (2 * a);
            var d2 = (-b - Math.Sqrt(determinant)) / (2 * a);
            var d = Math.Min(d1, d2);

            var isVisible = d <= maxDist && d >= minDist;
            return new Intersection(true, isVisible, this, line, d);
        }

        public override Vector Normal(Vector v)
        {
            var n = v - Center;
            n.Normalize();
            return n;
        }
    }
}