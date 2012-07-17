using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo srcDir = new DirectoryInfo(@"C:\source");
            DirectoryInfo destDir = new DirectoryInfo(@"C:\destination");

            ProcessDirectory(srcDir, destDir, true, 96, 80);        
        }

        public static void ProcessDirectory(DirectoryInfo sourceDir, DirectoryInfo destinationDir, bool resize, double maxSize, int quality)
        {
            foreach (FileInfo file in sourceDir.GetFiles())
            {           
                if (file.Name.Replace(".", "").Length == 9)
                {
                    if (Microsoft.VisualBasic.Information.IsNumeric(file.Name.Replace(".", "")))
                    {
                        Console.WriteLine("Processing file " + file.Name);
                        Image img = Image.FromFile(file.FullName);
                        string savePath = destinationDir.FullName + file.Name.Replace(".", "") + ".jpg";                     
                        SaveImage(img, savePath, resize, maxSize, quality);
                        Console.WriteLine("File saved at " + savePath);
                    }
                }
            }           
        }

        private static Size ImageResize(double currentWidth, double currentHeight, double maxPixel)
        {
            double tempMultiplier;
            Size newSize = new Size();

            if (currentHeight > maxPixel || currentWidth > maxPixel)
            {
                if (currentHeight > currentWidth) //Portrait
                {
                    tempMultiplier = maxPixel / currentHeight;                   
                }
                else
                {
                    tempMultiplier = maxPixel / currentWidth;
                }

                newSize.Height = Convert.ToInt32(currentHeight * tempMultiplier);
                newSize.Width = Convert.ToInt32(currentWidth * tempMultiplier);
            }
            else
            {
                newSize.Height = Convert.ToInt32(currentHeight);
                newSize.Width = Convert.ToInt32(currentWidth);
            }

            return newSize;
        }

        private static bool SaveImage(Image img, string savePath, bool resize, double maxPixel, int quality)
        {
            bool isSaved = false;

            if (resize)
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                EncoderParameters ep = new EncoderParameters();
                ImageCodecInfo ici = null;

                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.MimeType == "image/jpeg") ici = codec;
                }

                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Convert.ToInt64(quality));               
                Size thumbSize = ImageResize(img.Width, img.Height, maxPixel);                
                try
                {
                    Bitmap bitmap = new Bitmap(img, thumbSize.Width, thumbSize.Height);
                    bitmap.Save(savePath, ici, ep);
                    isSaved = true;
                    bitmap.Dispose();
                }
                catch (Exception ex)
                {
                    //Write to event log           
                    LogException("ImageResizer", ex.Message.ToString());
                }
            }
            else
            {
                img.Save(savePath);
                isSaved = true;
            }
            

            return isSaved;

        }


        /// <summary>
        /// Write an error to the server's application log
        /// </summary>
        /// <param name="Error"></param>
        public static void LogException(string Source, string Error)
        {
            string Log = "Application";

            //If the log doesn't exist then create it and log an entry
            if (!EventLog.SourceExists(Source))
                EventLog.CreateEventSource(Source, Log);

            //Write to the event log
            EventLog.WriteEntry(Source, Error, EventLogEntryType.Error);            

        }
     

    }
}
