using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using org.mariuszgromada.math.mxparser;

namespace BurrSize
{
    public enum NoiseRemovalMode
    {
        MedianBlur,
        MorphOpen,
        MorphClose,
    }
    public class ImageBinarizer
    {
        private NoiseRemovalMode noiseRemovalMode;
        private int coi;
        private int noiseRemovalSize;
        private ThresholdTypes thresholdType;
        private int thresholdVal;
        private double gaussianBlurSigma;
        private int gaussianBlurSize;

        public Expression eqx { get; set; }
        public Expression eqy { get; set; }
        public Expression veqx { get; set; }
        public Expression veqy { get; set; }
        public float xOffs { get; set; }
        public float yOffs { get; set; }
        public float tpi { get; set; }
        public ImageBinarizer(Expression eqx, Expression eqy, Expression veqx, Expression veqy, int coi = 0,
            NoiseRemovalMode noiseRemovalMode = NoiseRemovalMode.MedianBlur, int noiseRemovalSize = 0,
            ThresholdTypes thresholdType = ThresholdTypes.Otsu, int thresholdVal = 0, double gaussianBlurSigma = 10, int gaussianBlurSize = 0)
        {
            this.noiseRemovalMode = noiseRemovalMode;
            this.noiseRemovalSize = noiseRemovalSize;
            this.coi = coi;
            this.thresholdType = thresholdType;
            this.thresholdVal = thresholdVal;
            this.gaussianBlurSigma = gaussianBlurSigma;
            this.gaussianBlurSize = gaussianBlurSize;
            this.eqx = eqx;
            this.eqy = eqy;
            this.veqx = veqx;
            this.veqy = veqy;

        }
        public void BinarizeImage(Mat src, Mat dst)
        {
            src.CopyTo(dst);

            if (gaussianBlurSize > 0)
            {
                Cv2.GaussianBlur(dst, dst, new Size(noiseRemovalSize, noiseRemovalSize), gaussianBlurSigma);
            }

            Cv2.CvtColor(dst, dst, ColorConversionCodes.BGR2HSV);

            Mat extractedCh = dst.ExtractChannel(coi);
            Cv2.Threshold(extractedCh, dst, thresholdVal, 255, thresholdType);

            if (coi == 1) // when using S values, the metal's saturation is below the threshold
                Cv2.BitwiseNot(dst, dst);

            if (noiseRemovalSize > 0)
            {
                switch (noiseRemovalMode)
                {
                    case NoiseRemovalMode.MorphClose:
                        Cv2.Dilate(dst, dst, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(noiseRemovalSize, noiseRemovalSize)));
                        Cv2.Erode(dst, dst, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(noiseRemovalSize, noiseRemovalSize)));
                        break;
                    case NoiseRemovalMode.MorphOpen:
                        Cv2.Erode(dst, dst, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(noiseRemovalSize, noiseRemovalSize)));
                        Cv2.Dilate(dst, dst, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(noiseRemovalSize, noiseRemovalSize)));
                        break;
                    case NoiseRemovalMode.MedianBlur:
                        Cv2.MedianBlur(dst, dst, noiseRemovalSize);
                        break;
                    default:
                        break;

                }
            }
            //removeBackground(dst);
        }
        private void removeBackground(Mat img)
        {
            float t = tpi;
            float d = 0;
            var ind = img.GetGenericIndexer<byte>();
            var pt = BurrSizeAnalyzer.SubstFunction(t, eqx, eqy, xOffs, yOffs, img.Width, img.Height);
            Vec2f perpDir = BurrSizeAnalyzer.GetPerpendicular(t, veqx, veqy);
            var edgePt = BurrSizeAnalyzer.SubstLinearFunction(pt, perpDir, d);
        
            /*
            while (BurrSizeAnalyzer.PointInsideImage(edgePt, img.Width, img.Height))
            {
                Cv2.FloodFill(img, edgePt, 255);
                if (ind[edgePt.Y, edgePt.X] == 255)
                {
                    Cv2.FloodFill(img, edgePt, 128);
                    break;
                }
                d -= 0.5f;
                edgePt = BurrSizeAnalyzer.SubstLinearFunction(pt, perpDir, d);
            }
            for (int x = 0; x < img.Width; x++)
                for (int y = 0; y < img.Height; y++)
                {
                    if (ind[y, x] == 255)
                    {
                        Cv2.FloodFill(img, new Point(x, y), 0);
                    }
                }
            Cv2.FloodFill(img, edgePt, 255);*/
        }
    }


}
