using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIRS
{
    public partial class Form1 : Form
    {
        public Bitmap image;
        public byte[] b;
        public byte[] b2;
        public byte[] b3;

        public string text;
        public char[] ch;
        public char[] ch2;
        public char[] ch3;

        public static int Position;

        public int RLElength;
        public int LZWlength;
        public string filename = null;
        public string filepath = null;

        public bool imageLoaded = false;
        public bool textLoaded = false;
        public bool musicLoaded = false;

        public string extension;

        public List<string> imageFormat = new List<string>{ ".cr2",".NEF", ".jpg", ".jpeg", ".bmp", ".png", ".tif",".tiff", ".gif" };
        public List<string> textFormat = new List<string>{".txt"};

        public int l;
        public Form1()
        {
            InitializeComponent();
        }

        //BWT+RLE
        public static string BWT(string input)
        {
            string inputToFind = input;
            int length = input.Length;
            int count = 0;
            List<string> Reshuffle = new List<string>();
            while (count != length)
            {
                string sLast = input.Substring(0, 1);
                input = input.Remove(0, 1);
                input = input + sLast;
                Reshuffle.Add(input);
                count++;
            }
            Reshuffle.Sort();
            Position = Reshuffle.IndexOf(inputToFind);
            string output = "";
            foreach (string item in Reshuffle)
            {
                output = output + item.Substring(item.Length - 1, 1);
            }
            return RunLengthEncoding(output);
        }

        public static string RunLengthEncoding(string s)
        {
            string srle = string.Empty;
            int count = 1;
            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[i] != s[i + 1] || i == s.Length - 2)
                {
                    if (s[i] == s[i + 1] && i == s.Length - 2)
                        count++;
                    srle += count + ("1234567890".Contains(s[i]) ? "" : "") + s[i];
                    if (s[i] != s[i + 1] && i == s.Length - 2)
                        srle += ("1234567890".Contains(s[i + 1]) ? "1" : "") + s[i + 1];
                    count = 1;
                }
                else
                {
                    count++;
                }

            }
            return srle;
        }

        //RLE для изображения
        public static byte[] ImageRLE(byte[] source)
        {
            List<byte> dest = new List<byte>();
            byte runLength;
            byte r;
            r = source[0];
            runLength = 1;
            for (int i = 1; i < source.Length; i++)
            {

                if (r == source[i])
                {
                    runLength++;
                }
                else
                {
                    dest.Add(r);
                    runLength = 1;
                    r = source[i];
                }
            }

            dest.Add(runLength);
            dest.Add(r);
            return dest.ToArray();
        }

        public static char[] TextRLE(char[] source)
        {
            List<char> dest = new List<char>();
            byte runLength;
            char r;
            r = source[0];
            runLength = 1;
            for (int i = 1; i < source.Length; i++)
            {

                if (r == source[i])
                {
                    runLength++;
                }
                else
                {
                    dest.Add(r);
                    runLength = 1;
                    r = source[i];
                }
            }

            dest.Add(Convert.ToChar(runLength));
            dest.Add(r);
            return dest.ToArray();
        }

        //LZW
        public void ImageLZW()
        {
            int number = 0;
            bool f = false;
            string m = "";
            List<string> dictionary = new List<string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i.ToString());
            string w = string.Empty;
            foreach (byte c in b)
            {
                string wc;
                if (w == string.Empty)
                    wc = c.ToString();
                else
                    wc = w + "," + c.ToString();

                f = false;
                for (int i = 0; i < dictionary.Count; i++)
                {
                    if (dictionary[i] == wc)
                    {
                        f = true;
                        break;
                    }
                }

                if (f)
                {
                    w = wc;
                }
                else
                {
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        if (dictionary[i] == w)
                        {
                            m += i.ToString() + "_";
                            if (i < 256)
                                number += 8;
                            else
                                number += (int)Math.Log(i, 2) + 1;
                            break;
                        }
                    }
                    dictionary.Add(wc);
                    w = c.ToString();
                }
            }
            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary[i] == w)
                {
                    m += i.ToString() + "_";
                    if (i < 256)
                        number += 8;
                    else
                        number += (int)Math.Log(i, 2) + 1;
                    break;
                }
            }
            LZWlength = number/8;
        }

        public void TextLZW()
        {
            int number = 0;
            bool f = false;
            string m = "";
            List<string> dictionary = new List<string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i.ToString());
            string w = string.Empty;
            foreach (char c in ch)
            {
                string wc;
                if (w == string.Empty)
                    wc = c.ToString();
                else
                    wc = w + "," + c.ToString();

                f = false;
                for (int i = 0; i < dictionary.Count; i++)
                {
                    if (dictionary[i] == wc)
                    {
                        f = true;
                        break;
                    }
                }

                if (f)
                {
                    w = wc;
                }
                else
                {
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        if (dictionary[i] == w)
                        {
                            m += i.ToString() + "_";
                            if (i < 256)
                                number += 8;
                            else
                                number += (int)Math.Log(i, 2) + 1;
                            break;
                        }
                    }
                    dictionary.Add(wc);
                    w = c.ToString();
                }
            }
            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary[i] == w)
                {
                    m += i.ToString() + "_";
                    if (i < 256)
                        number += 8;
                    else
                        number += (int)Math.Log(i, 2) + 1;
                    break;
                }
            }
            LZWlength = number / 8;
        }

        //JPEG
        public static long VaryQualityLevel(string input, string output)
        {
            long lenghtJPG = 0;
            Bitmap bmp1 = new Bitmap(input);
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 0L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            FileStream outputStream = new FileStream(output + "jpeg.jpg", FileMode.Create);
            BinaryWriter outputWriter = new BinaryWriter(outputStream);
            //bmp1.Save(outputStream, jpgEncoder, myEncoderParameters);
            lenghtJPG = outputStream.Length;
            outputWriter.Close();
            outputStream.Close();
            return lenghtJPG;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        //Deflate

        public static long Deflate(string sourceFile, string compressedFile)
        {
            long DeflateLength = 0;
            // поток для чтения исходного файла
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                // поток для записи сжатого файла
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    // поток архивации
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream); // копируем байты из одного потока в другой
                        DeflateLength = targetStream.Length;
                    }
                }
            }
            return DeflateLength;
        }

        private void Open_Click(object sender, EventArgs e)
        {
            Bitmap image;
            OpenFileDialog open_dialog = new OpenFileDialog();
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = open_dialog.FileName;
                    extension = Path.GetExtension(filename);
                    FileInfo fi = new FileInfo(open_dialog.FileName);
                    filepath = fi.DirectoryName;
                    fi.CopyTo("copy.tiff", true);
                    if (imageFormat.Contains(extension))
                    {
                        imageLoaded = true;
                        image = new Bitmap(open_dialog.FileName);
                        string imageString = image.ToString();
                        b = ImageToByteArray(image);
                        l = Convert.ToInt32(fi.Length);
                        label1.Text = "Тип файла: изображение";

                        Stopwatch StopwatchRLE = new System.Diagnostics.Stopwatch();
                        StopwatchRLE.Start(); //запуск
                       
                        b2 = ImageRLE(b);
                        int l2 = b2.Length;

                        StopwatchRLE.Stop(); //остановить
                        TimeSpan ts = StopwatchRLE.Elapsed;

                        string elapsedTime = "RLE:" + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                      //  label4.Text = elapsedTime;
                        //ImageLZW();

                        Stopwatch StopwatchJPEG = new System.Diagnostics.Stopwatch();
                        StopwatchJPEG.Start(); //запуск

                        long lenghtJPG = VaryQualityLevel(filename, filepath + "\\");

                        StopwatchJPEG.Stop(); //остановить
                        TimeSpan ts1 = StopwatchJPEG.Elapsed;

                        string elapsedTime1 = "JPEG:" + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts1.Hours, ts1.Minutes, ts1.Seconds,
                        ts1.Milliseconds / 10);
                       // label5.Text = elapsedTime1;

                        Stopwatch StopwatchDeflate = new System.Diagnostics.Stopwatch();
                        StopwatchDeflate.Start(); //запуск

                        long lengthDeflate = Deflate("copy.tiff", "book.png");

                        StopwatchDeflate.Stop(); //остановить
                        TimeSpan ts2 = StopwatchDeflate.Elapsed;

                        string elapsedTime2 = "Deflate:" + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts2.Hours, ts2.Minutes, ts2.Seconds,
                        ts2.Milliseconds / 10);
                       // label6.Text = elapsedTime2;

                        Stopwatch StopwatchBWT = new System.Diagnostics.Stopwatch();
                        StopwatchBWT.Start(); //запуск

                        string outputBWT = BWT(imageString);
                        int BWTlength = outputBWT.Length;


                        StopwatchBWT.Stop(); //остановить
                        TimeSpan ts3 = StopwatchBWT.Elapsed;

                        List<int> rawList = new List<int>();
                        rawList.Add(l);
                        List<int> rleList = new List<int>();
                        rleList.Add(l2);
                        List<int> deflateList = new List<int>();
                        deflateList.Add(Convert.ToInt32(lengthDeflate));
                        //List<int> lzwList = new List<int>();
                        //lzwList.Add(LZWlength);
                        List<int> jpegList = new List<int>();
                        jpegList.Add(Convert.ToInt32(lenghtJPG));
                        List<int> BWTList = new List<int>();
                        BWTList.Add(BWTlength);
                        chart1.Series[0].Points.DataBindY(rawList);
                        chart1.Series[1].Points.DataBindY(rleList);
                        //chart1.Series[2].Points.DataBindY(lzwList);
                        chart1.Series[3].Points.DataBindY(jpegList);
                        chart1.Series[4].Points.DataBindY(deflateList);
                        chart1.Series[5].Points.DataBindY(BWTList);

                        List<int> timeRLEList = new List<int>();
                        timeRLEList.Add(ts.Seconds / 1000 + ts.Milliseconds);
                        List<int> timeDeflateList = new List<int>();
                        timeDeflateList.Add(ts2.Seconds / 1000 + ts2.Milliseconds);
                        List<int> timeJPEGList = new List<int>();
                        timeJPEGList.Add(ts1.Seconds / 1000 + ts1.Milliseconds);
                        List<int> timeBWTList = new List<int>();
                        timeBWTList.Add(ts3.Seconds / 1000 + ts1.Milliseconds);
                        //List<int> lzwList = new List<int>();
                        //lzwList.Add(LZWlength);
                        chart2.Series[0].Points.DataBindY(timeRLEList);
                        chart2.Series[2].Points.DataBindY(timeJPEGList);
                        //chart1.Series[1].Points.DataBindY(lzwList);
                        chart2.Series[3].Points.DataBindY(timeDeflateList);
                        chart2.Series[4].Points.DataBindY(timeBWTList);
                        
                    }
                    if (textFormat.Contains(extension))
                    {
                        
                        textLoaded = true;
                        text = File.ReadAllText(filename);
                        ch = text.ToCharArray(0, text.Length);
                        l = text.Length;
                        label1.Text = "Тип файла: текст";

                        Stopwatch StopwatchRLE = new System.Diagnostics.Stopwatch();
                        StopwatchRLE.Start(); //запуск

                        string outputRLE = RunLengthEncoding(text);
                        int l2 = outputRLE.Length;

                        StopwatchRLE.Stop(); //остановить
                        TimeSpan ts = StopwatchRLE.Elapsed;

                        string elapsedTime = "RLE:" + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                     //   label4.Text = elapsedTime;

                        Stopwatch StopwatchLZW = new System.Diagnostics.Stopwatch();
                        StopwatchLZW.Start(); //запуск

                        TextLZW();

                        StopwatchLZW.Stop(); //остановить
                        TimeSpan ts1 = StopwatchLZW.Elapsed;

                        string elapsedTime1 = "LZW:" + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts1.Hours, ts1.Minutes, ts1.Seconds,
                        ts1.Milliseconds / 10);
                    //    label5.Text = elapsedTime1;

                        Stopwatch StopwatchDeflate = new System.Diagnostics.Stopwatch();
                        StopwatchDeflate.Start(); //запуск

                        long lengthDeflate = Deflate("text1.txt", "book.txt");

                        StopwatchDeflate.Stop(); //остановить
                        TimeSpan ts2 = StopwatchDeflate.Elapsed;

                        string elapsedTime2 = "Deflate:" + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts2.Hours, ts2.Minutes, ts2.Seconds,
                        ts2.Milliseconds / 10);

                        Stopwatch StopwatchBWT = new System.Diagnostics.Stopwatch();
                        StopwatchBWT.Start(); //запуск

                        string outputBWT = BWT(text);
                        int BWTlength = outputBWT.Length;


                        StopwatchBWT.Stop(); //остановить
                        TimeSpan ts3 = StopwatchBWT.Elapsed;

                        //long lenghtJPG = VaryQualityLevel(filename, filepath + "\\");
                        List<int> rawList = new List<int>();
                        rawList.Add(l);
                        List<int> rleList = new List<int>();
                        rleList.Add(l2);
                        List<int> lzwList = new List<int>();
                        lzwList.Add(LZWlength);
                        /*List<int> jpegList = new List<int>();
                        jpegList.Add(Convert.ToInt32(lenghtJPG));*/
                       List<int> BWTList = new List<int>();
                        BWTList.Add(BWTlength);
                        List<int> deflateList = new List<int>();
                        deflateList.Add(Convert.ToInt32(lengthDeflate));
                        chart1.Series[0].Points.DataBindY(rawList);
                        chart1.Series[1].Points.DataBindY(rleList);
                        chart1.Series[2].Points.DataBindY(lzwList);
                        //chart1.Series[3].Points.DataBindY(jpegList);
                        chart1.Series[4].Points.DataBindY(deflateList);
                        chart1.Series[5].Points.DataBindY(BWTList);

                        List<int> timeRLEList = new List<int>();
                        timeRLEList.Add(ts.Seconds / 1000 + ts.Milliseconds);
                        List<int> timeDeflateList = new List<int>();
                        timeDeflateList.Add(ts2.Seconds / 1000 + ts2.Milliseconds);
                        List<int> timeLZWList = new List<int>();
                        timeLZWList.Add(ts1.Seconds / 1000 + ts1.Milliseconds);
                        List<int> timeBWTList = new List<int>();
                        timeBWTList.Add(ts3.Seconds / 1000 + ts1.Milliseconds);
                        chart2.Series[0].Points.DataBindY(timeRLEList);
                        chart2.Series[1].Points.DataBindY(timeLZWList);
                        chart2.Series[3].Points.DataBindY(timeDeflateList);
                        chart2.Series[4].Points.DataBindY(timeBWTList);
                    }
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private byte[] ImageToByteArray(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
