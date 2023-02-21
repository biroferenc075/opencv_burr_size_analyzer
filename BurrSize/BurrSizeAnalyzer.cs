using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using org.mariuszgromada.math.mxparser;

namespace BurrSize
{
    public class BurrSizeAnalyzer
    {
        private bool useParametricFunction;
        public HashSet<Point> cutPoints { get; set; } = new HashSet<Point>();
        public HashSet<Point> burrPoints { get; set; } = new HashSet<Point>();
        public List<float> burrSizes { get; set; } = new List<float>();
        private Mat img;
        private Mat.Indexer<byte> ind;
        public float xOffs { get; set; } = 0 ;
        public float yOffs { get; set; } = 0;
        public float tpi { get; set; }
        private float _tLower;
        public float tLower { get => _tLower; set {_tLower = value;tMinProcessed = value;} }
        public float tUpper { get; set; }
        public float tMinProcessed { get; set; }
        private const float res = 1 / 65536.0f;
        private const float dRes = 0.5f;
        public int imgPadding { get; set; } = 40;
        private Expression eqx;
        private Expression eqy;
        private Expression veqx;
        private Expression veqy;

        public HashSet<Point> testPoints = new HashSet<Point>();
        public BurrSizeAnalyzer(Mat sourceImage, Expression _eqx, Expression _eqy, Expression _veqx, Expression _veqy)
        {
            if (sourceImage.Type() != MatType.CV_8UC1)
                throw new Exception("MatType != MatType.CV_8UC1");
            this.useParametricFunction = true;
            img = sourceImage.Clone();
            img = img.CopyMakeBorder(0,0, imgPadding, imgPadding, BorderTypes.Constant, 255);
            ind = img.GetGenericIndexer<byte>();

            eqx = _eqx;
            eqy = _eqy;
            veqx = _veqx;
            veqy = _veqy;
        }
        public BurrSizeAnalyzer(Expression _eqx, Expression _eqy, Expression _veqx, Expression _veqy)
        {
            useParametricFunction = true;
            
            eqx = _eqx;
            eqy = _eqy;
            veqx = _veqx;
            veqy = _veqy;
        }
        public BurrSizeAnalyzer(Mat sourceImage)
        {
            if (sourceImage.Type() != MatType.CV_8UC1)
                throw new Exception("MatType != MatType.CV_8UC1");
            useParametricFunction = false;
            img = sourceImage.Clone();
            ind = img.GetGenericIndexer<byte>();
        }
        public BurrSizeAnalyzer()
        {
            useParametricFunction = false;
        }

        internal void SetImg(Mat sourceImage)
        {

            if (sourceImage.Type() != MatType.CV_8UC1)
                throw new Exception("MatType != MatType.CV_8UC1");
            img = sourceImage.Clone();
            if(useParametricFunction)
                img = img.CopyMakeBorder(imgPadding, imgPadding, imgPadding, imgPadding, BorderTypes.Constant, 255);
            ind = img.GetGenericIndexer<byte>();
        }

        public void SetTpi(float _tpi)
        {
            tpi = _tpi;
        }
        public void FindCutPoints()
        {
            cutPoints = new HashSet<Point>();
            burrPoints = new HashSet<Point>();
            burrSizes = new List<float>();
            testPoints = new HashSet<Point>();
            if (useParametricFunction)
            {
                FindParametric();
            }
            else
            {
                ApproximateCutFunction();
                FindApprox();
            }
        }

        public void SetOffset(float x, float y)
        {
            xOffs = x;
            yOffs = y;
        }

        private void FindParametric()
        {
            float t = tpi;
            var pt = SubstituteFunction(t);

            while (PointInsideImage(pt) && t > tLower && t > tMinProcessed)
            {
                cutPoints.Add(pt);
                if (FindPointAlongNormalOutwards(pt, t, dRes))
                    break;
                t -= res;
                pt = SubstituteFunction(t);
            }
            t = tpi;
            pt = SubstituteFunction(t);

            while (PointInsideImage(pt) && t < tUpper)
            {
                cutPoints.Add(pt);
                if (FindPointAlongNormalOutwards(pt, t, dRes))
                    break;
                
                t += res;
                pt = SubstituteFunction(t);
            }
            tMinProcessed = t;
        }
        private Point SubstituteLinearFunction(Point pt, Vec2f dirVec, float t)
        {
            var res = new Point(pt.X + t * dirVec.Item0, pt.Y + t * dirVec.Item1);
            return res;
        }
        private bool FindPointAlongNormalOutwards(Point pt, float t, float dDelta)
        {
            var foundEdge = ind[pt.Y, pt.X] == 0;
            float d = 0;
            Vec2f perpDir = GetPerpendicular(t);
            var edgePt = SubstituteLinearFunction(pt, perpDir, d);
            Point? burrPt = null;
            float? burrSize = null;

            while (PointInsideImage(edgePt))
            {
                testPoints.Add(edgePt);
                if (ind[edgePt.Y, edgePt.X] == 0 && !foundEdge)
                {
                    foundEdge = true;
                    burrPt = edgePt;
                    burrSize = d;
                    if (PointOnEdgeOfOriginal(edgePt))
                    {
                        burrPoints.Add(burrPt.Value);
                        burrSizes.Add(burrSize.Value);
                        return true;
                    }
                }
                if (ind[edgePt.Y, edgePt.X] == 255)
                {
                    foundEdge = false;
                }
                d += dDelta;
                edgePt = SubstituteLinearFunction(pt, perpDir, d);
            }
            if (burrPt is not null && burrSize is not null)
            {
                burrPoints.Add(burrPt.Value);
                burrSizes.Add(burrSize.Value);
            }
            else
            {
                FindPointAlongNormalInwards(pt, t, -dDelta);
            }
            return false;
        }

        private bool FindPointAlongNormalInwards(Point pt, float t, float dDelta)
        {
            float d = 0;
            Vec2f perpDir = GetPerpendicular(t);
            var edgePt = SubstituteLinearFunction(pt, perpDir, d);

            while (PointInsideImage(edgePt))
            {
                testPoints.Add(edgePt);
                if (ind[edgePt.Y, edgePt.X] == 255)
                {
                    burrPoints.Add(edgePt);
                    burrSizes.Add(d);
                    return true;
                }
                d += dDelta;
                edgePt = SubstituteLinearFunction(pt, perpDir, d);
            }
            return false;
        }
        private void FindApprox()
        {
            float t = 0;
            var pt = SubstituteFunction(t);
            Vec2f perpDir = GetPerpendicular(t);
            while (PointInsideImage(pt))
            {
                if (cutPoints.Add(pt))
                {
                    float d = 0;
                    var edgePt = SubstituteLinearFunction(pt,perpDir,d);

                    while (PointInsideImage(edgePt))
                    {
                        if (ind[edgePt.Y, edgePt.X] == 0)
                        {
                            if (burrPoints.Add(edgePt))
                                burrSizes.Add(d);
                            break;
                        }
                        d += 0.5f;
                        edgePt = SubstituteLinearFunction(pt, perpDir, d);
                    }
                }
                t += 0.5f;
                pt = SubstituteFunction(t);
            }

        }
        private void ApproximateCutFunction()
        {
            var p1 = FindLeftmostPoint();
            var p2 = FindRightmostPoint();

            var vx = p2.X - p1.X;
            var vy = p2.Y - p1.Y;
            var norm = Math.Sqrt(vx * vx + vy * vy);

            var r0 = new Point(p1.X, p1.Y);
            var v = new Point2f((float)(vx / norm), (float)(vy / norm));

            eqx = new Expression("a*t+b");
            eqy = new Expression("c*t+d");
            veqx = new Expression("a");
            veqy = new Expression("c");
            
            var a = new Constant("a", v.X);
            var b = new Constant("b", r0.X);
            var c = new Constant("c", v.Y);
            var d = new Constant("d", r0.Y);

            eqx.addConstants(a, b);
            veqx.addConstants(a);

            eqy.addConstants(c, d);
            veqy.addConstants(c);

            eqx.addArguments(new Argument("t"));
            eqy.addArguments(new Argument("t"));
            veqx.addArguments(new Argument("t"));
            veqy.addArguments(new Argument("t"));
        }
        private Point SubstituteFunction(float t)
        {
            eqx.setArgumentValue("t", t);
            eqy.setArgumentValue("t", t);
            Point pt;
            
            pt = new Point(-xOffs + img.Width / 2.0f + eqx.calculate(), yOffs + img.Height / 2.0f - eqy.calculate());
            return pt;
        }
        private Vec2f GetPerpendicular(float t)
        {
            veqx.setArgumentValue("t", t);
            veqy.setArgumentValue("t", t);
            Vec2f pt;
            var x = veqy.calculate();
            var y = veqx.calculate();
            var len = Math.Sqrt(x * x + y * y);
            pt = new Vec2f((float)(x/len), (float)(y/len));
            return pt;
        }

        private bool PointInsideImage(Point pt) {
            var w = img.Width;
            var h = img.Height;
            return 0 <= pt.X && pt.X < w && 0 <= pt.Y && pt.Y < h;
        }
        private bool PointOnEdgeOfOriginal(Point pt)
        {
            var w = img.Width;
            var h = img.Height;
            var p = imgPadding;
            return p == pt.X || pt.X == w - p - 1|| p == pt.Y || pt.Y == h - p - 1;
        }

        private Point2d FindLeftmostPoint() {
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = img.Height - 1; y > 0; y--)
                {
                    if (ind[y, x] == 255)
                    {
                        return new Point2d(x, y);
                    }
                }
            }
            return new Point2d(-1, -1);
        }
        private Point2d FindRightmostPoint()
        {
            for (int x = img.Width - 1; x > 0; x--)
            {
                for (int y = img.Height - 1; y > 0; y--)
                {
                    if (ind[y, x] == 255)
                    {
                        return new Point2d(x, y);
                    }
                }
            }
            return new Point2d(-1, -1);
        }
        public static Point SubstLinearFunction(Point pt, Vec2f dirVec, float t)
        {
            var res = new Point(pt.X + t * dirVec.Item0, pt.Y + t * dirVec.Item1);
            return res;
        }
        public static Point SubstFunction(float t, Expression eqx, Expression eqy, float xOffs, float yOffs, int width, int height)
        {
            eqx.setArgumentValue("t", t);
            eqy.setArgumentValue("t", t);
            Point pt;
            pt = new Point(-xOffs + width / 2.0f + eqx.calculate(), yOffs + height / 2.0f - eqy.calculate());
            return pt;
        }
        public static Vec2f GetPerpendicular(float t, Expression veqx, Expression veqy)
        {
            veqx.setArgumentValue("t", t);
            veqy.setArgumentValue("t", t);
            Vec2f pt;
            var x = veqy.calculate();
            var y = veqx.calculate();
            var len = Math.Sqrt(x * x + y * y);
            pt = new Vec2f((float)(x / len), (float)(y / len));
            return pt;
        }
        public static bool PointInsideImage(Point pt, int w, int h)
        {
            return 0 <= pt.X && pt.X < w && 0 <= pt.Y && pt.Y < h;
        }
    } 
}
