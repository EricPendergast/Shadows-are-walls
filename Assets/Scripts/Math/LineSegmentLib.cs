// The MIT License (MIT)
// 
// Copyright (c) 2017 setchi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

public static class LineSegmentLib
{

    private static float GetParameterizedProjection(Vector2 l1, Vector2 l2, Vector2 point) {
        float segLenSq = (l2 - l1).sqrMagnitude;  // i.e. |w-v|^2 -  avoid a sqrt
        if (segLenSq == 0.0) {
            return 0;
        }
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line. 
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        float t = Vector2.Dot(point - l1, l2 - l1) / segLenSq;
        return t;
    }

    public static Vector2 ClosestPointOnLineSeg(Vector2 l1, Vector2 l2, Vector2 point) {
        float t = GetParameterizedProjection(l1, l2, point);
        t = Mathf.Clamp01(t);
        Vector2 projection = l1 + t * (l2 - l1);
        return projection;
    }

    public static Vector2 ClosestPointOnRay(Vector2 rayStart, Vector2 rayDirection, Vector2 point) {
        float t = GetParameterizedProjection(rayStart, rayStart + rayDirection, point);
        t = Mathf.Clamp(t, 0, float.MaxValue);
        Vector2 projection = rayStart + t*rayDirection;
        return projection;
    }

    public static void LineParametrizedIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out float u, out float v) {

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f) {
            u = float.PositiveInfinity;
            v = float.PositiveInfinity;
        } else {
            u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;
        }
    }

    public static bool LineSegmentRayIntersection(Vector2 p1, Vector2 p2, Vector2 rayP3, Vector2 rayP4, out Vector2 intersection) {
        intersection = Vector2.positiveInfinity;

        LineParametrizedIntersection(p1, p2, rayP3, rayP4, out float u, out float v);

        if (u < 0.0f || u > 1.0f || v < 0.0f) {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.positiveInfinity;

        LineParametrizedIntersection(p1, p2, p3, p4, out float u, out float v);

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }


    public static Vector2 Project(Vector2 point, Vector2 line1, Vector2 line2) {
        Vector2 pVec = point - line1;
        Vector2 lVec = line2 - line1;
        Vector2 proj = (Vector2.Dot(pVec, lVec)/lVec.sqrMagnitude) * lVec;
        return line1 + proj;
    }

    public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection, float epsilon, bool line2IsRay = false)
    {
        intersection = Vector2.positiveInfinity;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
        if (d == 0.0f) {
            if (epsilon == 0) {
                return false;
            }
            intersection = ClosestPointOnLineSeg(p1, p2, p3);
            var closest = ClosestPointOnLineSeg(p3, p4, intersection);
            return (closest - intersection).sqrMagnitude < epsilon*epsilon;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || (!line2IsRay && v > 1.0f)) {
            if (epsilon == 0) {
                return false;
            }
            Vector2 lineIntersection = p1 + u*(p2 - p1);
            
            Vector2 t1 = p1 + Mathf.Clamp01(u)*(p2 - p1);
            Vector2 t2 = p3 + Mathf.Clamp01(v)*(p4 - p3);

            Vector2 close1;
            Vector2 close2;

            if (Math.IsObtuse(lineIntersection, t1, t2) || u >= 0 && u <= 1) {
                close1 = ClosestPointOnLineSeg(p1, p2, t2);
                close2 = t2;
            } else if (Math.IsObtuse(lineIntersection, t2, t1) || v >= 0 && v <= 1) {
                close1 = t1;
                close2 = ClosestPointOnLineSeg(p3, p4, t1);
            } 
            else {
                close1 = t1;
                close2 = t2;
            }

            intersection = (close1 + close2)/2;
            return (close1 - close2).sqrMagnitude < epsilon*epsilon;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.positiveInfinity;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }
}
