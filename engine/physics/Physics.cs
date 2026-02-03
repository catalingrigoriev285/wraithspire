using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace wraithspire.engine.physics
{
    public static class Physics
    {
        // Intersect ray with a box (axis-aligned for simplicity, or localized if we transform ray)
        // Returns distance to hit, or null if no hit
        public static float? RayIntersectsBox(Ray ray, Vector3 boxMin, Vector3 boxMax)
        {
            float tMin = (boxMin.X - ray.Origin.X) / ray.Direction.X;
            float tMax = (boxMax.X - ray.Origin.X) / ray.Direction.X;

            if (tMin > tMax) (tMin, tMax) = (tMax, tMin);

            float tyMin = (boxMin.Y - ray.Origin.Y) / ray.Direction.Y;
            float tyMax = (boxMax.Y - ray.Origin.Y) / ray.Direction.Y;

            if (tyMin > tyMax) (tyMin, tyMax) = (tyMax, tyMin);

            if ((tMin > tyMax) || (tyMin > tMax)) return null;

            if (tyMin > tMin) tMin = tyMin;
            if (tyMax < tMax) tMax = tyMax;

            float tzMin = (boxMin.Z - ray.Origin.Z) / ray.Direction.Z;
            float tzMax = (boxMax.Z - ray.Origin.Z) / ray.Direction.Z;

            if (tzMin > tzMax) (tzMin, tzMax) = (tzMax, tzMin);

            if ((tMin > tzMax) || (tzMin > tMax)) return null;

            if (tzMin > tMin) tMin = tzMin;
            if (tzMax < tMax) tMax = tzMax;

            return tMin;
        }

        public static Ray ScreenPointToRay(Vector2 mousePos, Vector2 screenSize, Matrix4 view, Matrix4 proj)
        {
            float x = (2.0f * mousePos.X) / screenSize.X - 1.0f;
            float y = 1.0f - (2.0f * mousePos.Y) / screenSize.Y;
            
            // In OpenTK/OpenGL, clip space Z is -1 to 1 typically. 
            // We want a ray going from near plane to far plane.
            
            Vector4 rayStartNds = new Vector4(x, y, -1.0f, 1.0f);
            Vector4 rayEndNds = new Vector4(x, y, 1.0f, 1.0f);

            Matrix4 invProj = Matrix4.Invert(proj);
            Matrix4 invView = Matrix4.Invert(view);

            Vector4 rayStartCamera = rayStartNds * invProj;
            rayStartCamera /= rayStartCamera.W;

            Vector4 rayStartWorld = rayStartCamera * invView;
            rayStartWorld /= rayStartWorld.W;
            
            Vector4 rayEndCamera = rayEndNds * invProj;
            rayEndCamera /= rayEndCamera.W;
            
            Vector4 rayEndWorld = rayEndCamera * invView;
            rayEndWorld /= rayEndWorld.W;

            Vector3 rayDir = Vector3.Normalize(rayEndWorld.Xyz - rayStartWorld.Xyz);

            return new Ray(rayStartWorld.Xyz, rayDir);
        }
    }
}
