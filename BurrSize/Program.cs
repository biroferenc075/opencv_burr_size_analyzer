
using OpenCvSharp;
using org.mariuszgromada.math.mxparser;

namespace BurrSize
{
    internal class Program
    {
        static void Main(string[] args)
        {
            mXparser.disableCanonicalRounding();
            mXparser.disableUlpRounding();
            mXparser.disableAlmostIntRounding();

            ConfigurationManager cfg = new ConfigurationManager();
            var eqx = new Expression(cfg.eqx);
            var eqy = new Expression(cfg.eqy);
            var veqx = new Expression(cfg.veqx);
            var veqy = new Expression(cfg.veqy);
            eqx.addArguments(new Argument("t"));
            eqy.addArguments(new Argument("t"));
            veqx.addArguments(new Argument("t"));
            veqy.addArguments(new Argument("t"));
            ImageBinarizer binarizer = new ImageBinarizer(
                coi: cfg.coi,
                noiseRemovalMode: cfg.noiseRemovalMode,
                noiseRemovalSize: cfg.noiseRemovalSize,
                gaussianBlurSigma: cfg.gaussianBlurSigma,
                gaussianBlurSize: cfg.gaussianBlurSize,
                thresholdType: cfg.thresholdType,
                thresholdVal: cfg.thresholdVal,
                eqx: eqx,
                eqy: eqy,
                veqx: veqx,
                veqy: veqy
                );
            var files = Directory.GetFiles(cfg.location, "*.jpg");

            ResultExporter resExp = new ResultExporter("results.xlsx");
            Mat img;
            Mat bin = new Mat();

            

            BurrSizeAnalyzer burrSizeAnalyzer = new BurrSizeAnalyzer(eqx, eqy, veqx, veqy);
            burrSizeAnalyzer.tLower=cfg.tmin;
            burrSizeAnalyzer.tUpper=cfg.tmax;
            burrSizeAnalyzer.imgPadding=cfg.padding;
            int i = 0;
            foreach (var file in files.Where(s => !s.Contains("res")).OrderBy(s => s, new FileNameComparer()))
            {
                Console.WriteLine("Processing: " + file.ToString());
                img = Cv2.ImRead(file);
                bin = new Mat();

                binarizer.xOffs = cfg.xOffs[i] * cfg.pixmm;
                binarizer.yOffs = cfg.yOffs[i] * cfg.pixmm;
                binarizer.tpi = cfg.tpi[i];

                binarizer.BinarizeImage(img, bin);

                burrSizeAnalyzer.SetImg(bin);
                burrSizeAnalyzer.SetTpi(cfg.tpi[i]);
                
                burrSizeAnalyzer.SetOffset(cfg.xOffs[i] * cfg.pixmm, cfg.yOffs[i] * cfg.pixmm);

                burrSizeAnalyzer.FindCutPoints();

                if (cfg.showRes || cfg.saveRes)
                {
                    img = img.CopyMakeBorder(cfg.padding, cfg.padding, cfg.padding, cfg.padding, BorderTypes.Constant, new Scalar(0, 0, 0));
                    var pts = burrSizeAnalyzer.cutPoints;
                    var edgePts = burrSizeAnalyzer.burrPoints;
                    var ind = img.GetGenericIndexer<Vec3b>();
                    foreach (var pt in pts)
                    {
                        ind[pt.Y, pt.X] = new Vec3b(255, 0, 255);
                    }
                    foreach (var pt in edgePts)
                    {
                        ind[pt.Y, pt.X] = new Vec3b(255, 255, 0);
                    }
                    img = img.SubMat(cfg.padding, img.Height - cfg.padding, cfg.padding, img.Width - cfg.padding);
                    if (cfg.showRes) {
                        Cv2.ImShow("result", img);
                        Cv2.ImShow("binaryImg", bin);
                        if (Cv2.WaitKey() == 27)
                            break;
                    }
                    if(cfg.saveRes)
                    {
                        string fname = file.Insert(file.LastIndexOf('\\')+1, "res_");
                        Cv2.ImWrite(fname, img);
                    }
                    
                }
                if (cfg.exportResults)
                    resExp.SaveResults(burrSizeAnalyzer.burrSizes, String.Format("img{0}_res", i+1));
                i++;
            }
            if (cfg.exportResults)
                resExp.AggregateResults();
        }

        private class FileNameComparer : IComparer<string>
        {
            public int Compare(string? s1, string? s2)
            {
                var str1 = s1.Split('\\').Last();
                var str2 = s2.Split('\\').Last();
                str1 = str1.Substring(0, str1.IndexOf('.')).Substring(str1.IndexOf('_')+1).PadLeft(3, '0');
                str2 = str2.Substring(0, str2.IndexOf('.')).Substring(str2.IndexOf('_')+1).PadLeft(3, '0');

                return str1.CompareTo(str2); 

            }
        }
    }
}        



    