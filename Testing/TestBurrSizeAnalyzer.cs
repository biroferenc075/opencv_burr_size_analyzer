using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using BurrSize;
using System.Diagnostics;

namespace Testing
{
    public class TestBurrSizeAnalyzer
    {
        [Fact]
        public void BurrSizeAnalysisSampleImage()
        {
            Mat testImg = new Mat(new Size(300,300), MatType.CV_8UC1, new Scalar(0));
            testImg.Rectangle(new Point(0, 0), new Point(300, 150), new Scalar(255), -1);
            testImg.Rectangle(new Point(140, 150), new Point(160, 160), new Scalar(255), -1);

            BurrSizeAnalyzer bsa = new BurrSizeAnalyzer(testImg);
            bsa.FindCutPoints();

            var cutPts = bsa.cutPoints;
            var burrPts = bsa.burrPoints;
            var expCutPts = new List<Vec2i>();
            for(int i = 0; i < 300; i++)
            {
                expCutPts.Add(new Vec2i(i,150));
            }
            var expBurrPts = new List<Vec2i>();
            for (int i = 0; i < 300; i++)
            {
                if(140 <= i && i <= 160)
                    expBurrPts.Add(new Vec2i(i, 161));
                else
                    expBurrPts.Add(new Vec2i(i, 151));
            }
            for(int i = 0; i < expCutPts.Count; i++)
            {
                Assert.Contains(expCutPts[i], cutPts);
            }
            for (int i = 0; i < expBurrPts.Count; i++)
            {
                Assert.Contains(expBurrPts[i], burrPts);
            }
        }
    }
}