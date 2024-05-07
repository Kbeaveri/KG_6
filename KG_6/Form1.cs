using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace KG_6
{
    public partial class Form1 : Form
    {
        Point mid;
        float pictureBoxWidth;
        float pictureBoxHeight;
        Graphics g;
        Pen axisPen = new Pen(Color.Black, 2);
        Pen gridPen = new Pen(Color.Black, 0.5f);
        Font Fon = new Font("Arial", 9, FontStyle.Regular);
        Brush brush = new SolidBrush(Color.Black);
        Brush fillArea = new SolidBrush(Color.Red);
        Brush noFillArea = new SolidBrush(Color.Gray);
        float divX;
        float divY;
        const int countX = 20;
        const int countY = 20;
        float centerX, centerY;

        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            pictureBoxWidth = pictureBox1.Width;
            pictureBoxHeight = pictureBox1.Height;
            divX = pictureBoxWidth / countX;
            divY = pictureBoxHeight / countY;
            centerX = pictureBoxWidth / 2;
            centerY = pictureBoxHeight / 2;

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
        private Point[] Convert(List<Point> point)
        {
            List<Point> result = new List<Point>();
            for (int i = 0; i < point.Count; i++)
            {
                result.Add(Convert(point[i]));
            }
            return result.ToArray();
        }
        private Point Convert(Point point)
        {
            return new Point((int)(divX + (point.X * divX)), (int)((pictureBoxHeight - divY) - point.Y * divY));
        }
        private void mainFunction()
        {
            List<Point> points = inputFile();
            Point[]right = Convert(points);
            int n = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                g.FillEllipse(brush,right[i].X, right[i].Y,10,10);
            }
            points = points.OrderBy(p => p.X).ToList();
            List<Point> pointEnd = divide(points);
            Point[] coorPoint = Convert(pointEnd);
            g.DrawLines(gridPen, coorPoint);
            g.DrawLine(gridPen, coorPoint[0], coorPoint[(coorPoint.Count() - 1)]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            drawAxis();
            mainFunction();
        }

        private void drawAxis()
        {
            g.Clear(Color.White);
            PointF axisXStart = new PointF(divX, pictureBoxHeight - divY);
            PointF axisXEnd = new PointF(pictureBoxWidth, pictureBoxHeight - divY);
            PointF axisYStart = new PointF(divX, 0);
            PointF axisYEnd = new PointF(divX, pictureBoxHeight - divY);
            g.DrawLine(axisPen, axisXStart, axisXEnd);
            g.DrawLine(axisPen, axisYStart, axisYEnd);
            for (int i = 1; i <= countY; i++)
            {
                g.DrawString((i).ToString(), Fon, brush, divX - 17, pictureBoxHeight + divY * -i - divY);
            }
            for (int i = 1; i <= countX; i++)
            {
                g.DrawString(i.ToString(), Fon, brush, divX * i + 5, pictureBoxHeight - 15);
            }
        }
        private List<Point> inputFile()
        {
            string line;
            List<Point> points = new List<Point>();
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("input.txt");
                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    string[] coordinates = line.Split(' ');
                    int x = int.Parse(coordinates[0]);
                    int y = int.Parse(coordinates[1]);
                    points.Add(new Point(x, y));
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            return points;

        }

    }
}