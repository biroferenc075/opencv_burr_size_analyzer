using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
namespace BurrSize
{
    public class ConfigurationManager
    {
        public ConfigurationManager(string cfgFName = "params.txt")
        {
            try
            {
                using (StreamReader reader = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\" + cfgFName))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length > 0 && line[0] == '#')
                            continue;
                        string[] items = line.Split('=');
                        setPropertyFromString(items[0], items[1]);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Konfigurációs fájl nem található! (params.txt)");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            try
            {
                using (StreamReader reader = File.OpenText(location + "\\" + data))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length > 0 && line.Contains('X'))
                            continue;
                        string[] items = line.Split('\t');
                        processData(items);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Bemeneti adat fájl nem található! (" + data + ")");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void processData(string[] items)
        {
            xOffs.Add(float.Parse(items[0].Replace(".", ",")));
            yOffs.Add(float.Parse(items[1].Replace(".", ",")));
            tpi.Add(float.Parse(items[2].Replace(".", ",")));
        }

        public string location { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        public bool showRes { get; set; } = true;
        public bool saveRes { get; set; } = false;
        public string data { get; set; } = "data.txt";
        public bool exportResults { get; set; } = true;
        public string eqx { get; set; }
        public string eqy { get; set; }
        public string veqx { get; set; }
        public string veqy { get; set; }
        public float tmin { get; set; }
        public float tmax { get; set; }

        public int coi { get; set; } = 0;
        public NoiseRemovalMode noiseRemovalMode { get; set; } = NoiseRemovalMode.MorphClose;
        public int noiseRemovalSize { get; set; } = 9;
        public int gaussianBlurSigma { get; set; } = 10;
        public int gaussianBlurSize { get; set; } = 5;
        public ThresholdTypes thresholdType { get; set; } = ThresholdTypes.Binary;
        public int thresholdVal { get; set; } = 20;
        public int padding { get; set; }
        public float pixmm { get; set; }


        public List<float> xOffs { get; set; } = new();

        public List<float> yOffs { get; set; } = new();

        public List<float> tpi { get; set; } = new();
        private void setPropertyFromString(string prop, string value)
        {
            switch (prop)
            {
                case "location":
                    if (!value.Equals("-"))
                        location = value;
                    break;
                case "data":
                    data = value;
                    break;
                case "showRes":
                    showRes = Boolean.Parse(value);
                    break;
                case "saveRes":
                    saveRes = Boolean.Parse(value);
                    break;
                case "exportResults":
                    exportResults = Boolean.Parse(value);
                    break;
                case "eqx":
                    eqx = value;
                    break;
                case "eqy":
                    eqy = value;
                    break;
                case "veqx":
                    veqx = value;
                    break;
                case "veqy":
                    veqy = value;
                    break;
                case "tmin":
                    tmin = float.Parse(value.Replace('.', ','));
                    break;
                case "tmax":
                    tmax = float.Parse(value.Replace(".", ","));
                    break;
                case "coi":
                    coi = int.Parse(value);
                    break;
                case "noiseRemovalMode":
                    var nrm = int.Parse(value);
                    switch (nrm)
                    {
                        case 0:
                            noiseRemovalMode = NoiseRemovalMode.MorphClose;
                            break;

                        case 1:
                            noiseRemovalMode = NoiseRemovalMode.MorphOpen;
                            break;

                        case 2:
                            noiseRemovalMode = NoiseRemovalMode.MedianBlur;
                            break;
                    }
                    break;
                case "noiseRemovalSize":
                    noiseRemovalSize = int.Parse(value);
                    break;
                case "gaussianBlurSigma":
                    gaussianBlurSigma = int.Parse(value);
                    break;
                case "gaussianBlurSize":
                    gaussianBlurSize = int.Parse(value);
                    break;
                case "thresholdType":
                    var tht = int.Parse(value);
                    switch (tht)
                    {
                        case 0:
                            thresholdType = ThresholdTypes.Otsu;
                            break;

                        case 1:
                            thresholdType = ThresholdTypes.Binary;
                            break;
                    }
                    break;
                case "thresholdVal":
                    thresholdVal = int.Parse(value);
                    break;
                case "padding":
                    padding = int.Parse(value);
                    break;
                case "pixmm":
                    pixmm = float.Parse(value);
                    break;
            }

        }
    }
}
