using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KG_6
{
    public partial class Form1 : Form
    {
        Point mid;
        public Form1()
        {
            InitializeComponent();
            mainFunction();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        int quad(Point p)
        {
            if (p.X >= 0 && p.Y >= 0)
                return 1;
            if (p.X <= 0 && p.Y >= 0)
                return 2;
            if (p.X <= 0 && p.Y <= 0)
                return 3;
            return 4;
        }
        bool compare(Point p1, Point q1)
        {
            Point p = new Point(p1.X - mid.X,
                                        p1.Y - mid.Y);
            Point q = new Point(q1.X - mid.X,
                                        q1.Y - mid.Y);

            int one = quad(p);
            int two = quad(q);

            if (one != two)
                return (one < two);
            return (p.Y * q.X < q.Y * p.X);
        }
        private List<Point> Sort(List<Point> points)
        {
            Point tmp = new Point();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - 1; j++)
                {
                    if (!compare(points[j], points[j + 1]))
                    {
                        tmp = points[j + 1];
                        points[j + 1] = points[j];
                        points[j] = tmp;
                    }
                }
            }
            return points;
        }
        private List<Point> bruteHull(List<Point> points)
        {
            List<Point> s = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    int x1 = points[i].X, x2 = points[j].X;
                    int y1 = points[i].Y, y2 = points[j].Y;

                    int a1 = y1 - y2;
                    int b1 = x2 - x1;
                    int c1 = x1 * y2 - y1 * x2;
                    int pos = 0, neg = 0;
                    for (int k = 0; k < points.Count; k++)
                    {
                        if (a1 * points[k].X + b1 * points[k].Y + c1 <= 0)
                            neg++;
                        if (a1 * points[k].X + b1 * points[k].Y + c1 >= 0)
                            pos++;
                    }
                    if (pos == points.Count || neg == points.Count())
                    {
                        s.Add(points[i]);
                        s.Add(points[j]);
                    }
                }
            }

            List<Point> ret = new List<Point>();
            for (int i = 0; i < s.Count; i++)
            {
                ret.Add(s[i]);
            }

            // Sorting the points in the anti-clockwise order
            mid.X = 0;
            mid.Y = 0;
            int n = ret.Count;
            for (int i = 0; i < n; i++)
            {
                mid.X += ret[i].X;
                mid.Y += ret[i].Y;
                ret[i] = new Point(ret[i].X * n, ret[i].Y * n);
            }
            ret = Sort(ret);
            for (int i = 0; i < n; i++)
                ret[i] = new Point(ret[i].X / n, ret[i].Y / n);

            return ret;
        }
        int orientation(Point a, Point b,
                Point c)
        {
            int res = (b.Y - a.Y) * (c.X - b.X) -
                    (c.Y - b.Y) * (b.X - a.X);

            if (res == 0)
                return 0;
            if (res > 0)
                return 1;
            return -1;
        }
        private List<Point> merger(List<Point> a, List<Point> b)
        {
            // n1 -> number of points in polygon a
            // n2 -> number of points in polygon b
            int n1 = a.Count(), n2 = b.Count();

            int ia = 0, ib = 0;
            for (int i = 1; i < n1; i++)
                if (a[i].X > a[ia].X)
                    ia = i;

            // ib -> leftmost point of b
            for (int i = 1; i < n2; i++)
                if (b[i].X < b[ib].X)
                    ib = i;

            // finding the upper tangent
            int inda = ia, indb = ib;
            bool done = false;
            while (!done)
            {
                done = true;
                while (orientation(b[indb], a[inda], a[(inda + 1) % n1]) >= 0)
                    inda = (inda + 1) % n1;

                while (orientation(a[inda], b[indb], b[(n2 + indb - 1) % n2]) <= 0)
                {
                    indb = (n2 + indb - 1) % n2;
                    done = false;
                }
            }

            int uppera = inda, upperb = indb;
            inda = ia;
            indb = ib;
            done = false;
            int g = 0;
            while (!done)//finding the lower tangent
            {
                done = true;
                while (orientation(a[inda], b[indb], b[(indb + 1) % n2]) >= 0)
                    indb = (indb + 1) % n2;

                while (orientation(b[indb], a[inda], a[(n1 + inda - 1) % n1]) <= 0)
                {
                    inda = (n1 + inda - 1) % n1;
                    done = false;
                }
            }

            int lowera = inda, lowerb = indb;
            List<Point> ret = new List<Point>();

            //ret contains the convex hull after merging the two convex hulls
            //with the points sorted in anti-clockwise order
            int ind = uppera;
            ret.Add(a[uppera]);
            while (ind != lowera)
            {
                ind = (ind + 1) % n1;
                ret.Add(a[ind]);
            }

            ind = lowerb;
            ret.Add(b[lowerb]);
            while (ind != upperb)
            {
                ind = (ind + 1) % n2;
                ret.Add(b[ind]);
            }
            return ret;

        }
        private List<Point> divide(List<Point> points)
        {
            if (points.Count <= 5)
            {
                return bruteHull(points);
            }
            List<Point> left = new List<Point>();
            List<Point> right = new List<Point>();
            for (int i = 0; i < points.Count / 2; i++)
            {
                left.Add(points[i]);
            }
            for (int i = points.Count / 2; i < points.Count; i++)
            {
                right.Add(points[i]);
            }
            List<Point> leftHull = divide(left);
            List<Point> rightHull = divide(right);
            return merger(leftHull, rightHull);
        }
        private void mainFunction()
        {
            List<Point> points = new List<Point>();
            points.Add(new Point(0, 0));
            points.Add(new Point(1, -4));
            points.Add(new Point(-1, -5));
            points.Add(new Point(-5, -3));
            points.Add(new Point(-3, -1));
            points.Add(new Point(-1, -3));
            points.Add(new Point(-2, -2));
            points.Add(new Point(-1, -1));
            points.Add(new Point(-2, -1));
            points.Add(new Point(-1, 1));
            int n = points.Count;
            points = points.OrderBy(p => p.X).ToList();
            List<Point> pointEnd = divide(points);
        }


    }
}