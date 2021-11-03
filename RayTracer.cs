using System;
using System.Runtime.InteropServices;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            var u = n * viewPlaneSize / imgSize;
            u -= viewPlaneSize / 2;
            return u;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = new Intersection();

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {
            var lightToPoint = point - light.Position;
            var ray = new Line(light.Position, point);
            var epsilon = 1f;
            var intersection = FindFirstIntersection(ray, 0, lightToPoint.Length() - epsilon);
            return !intersection.Visible;
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color();
            var image = new Image(width, height);

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var viewPlanePoint = camera.Position +
                              camera.Direction * camera.ViewPlaneDistance +
                              (camera.Up ^ camera.Direction) * ImageToViewPlane(i, width, camera.ViewPlaneWidth) +
                              camera.Up * ImageToViewPlane(j, height, camera.ViewPlaneHeight);
                    var ray = new Line(camera.Position, viewPlanePoint);

                    var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);
                    image.SetPixel(i, j, intersection.Visible? CalculateColor(intersection, camera) : background);
                }
            }

            image.Store(filename);
        }

        private Color CalculateColor(Intersection intersection, Camera camera)
        {
            var color = new Color();
            var N = intersection.Geometry.Normal(intersection.Position);
            var E = (camera.Position - intersection.Position).Normalize();
            var material = intersection.Geometry.Material;
            foreach (var light in lights)
            {
                var colorFromLight = CalculateColorForLight(intersection, light, N, E, material);
                color += colorFromLight;
            }
            return color.Red != 0 || color.Green != 0 || color.Blue != 0 || color.Alpha != 0? 
                color : intersection.Geometry.Material.Ambient;
        }

        private Color CalculateColorForLight(Intersection intersection, Light light, Vector N, Vector E, Material material)
        {
            if (!IsLit(intersection.Position, light)) 
                return new Color();
            
            var T = (light.Position - intersection.Position).Normalize();
            var R = (N * (N * T) * 2 - T).Normalize();

            var color = intersection.Geometry.Material.Ambient * light.Ambient;
            if (N * T > 0)
                color += material.Diffuse * light.Diffuse * (N * T);
            if (E * R > 0)
                color += material.Specular * light.Specular * Math.Pow(E * R, material.Shininess);

            return color * light.Intensity;
        }
    }
}