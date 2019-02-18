using LeagueOfMixerClient.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace LeagueOfMixerClient.LeagueCore
{
    public class LeagueRegion
    {
        public LeagueRegion(double left, double right, double top, double bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public double Left;
        public double Right;
        public double Top;
        public double Bottom;
    }

    public class LeagueTextRegion
    {
        public LeagueRegion Region;
        public string Name;
        public bool UseCrispyImage = true;
        public bool SendBackBitmap = false;
    }

    public class LeagueTextRegionUpdate
    {
        public LeagueTextRegion TextRegion;
        public string Text;
        public float Confidence;
        public bool Success;
        public Bitmap Bitmap;
        public Bitmap CrispyBitmap;
    }

    public class LeagueChopper
    {
        LeagueGrabber m_grabber;
        DontOcrMeBro m_ocr;
        List<LeagueTextRegion> m_regions;
        List<double> m_avgTime;

        public LeagueChopper(string processName)
        {
            m_avgTime = new List<double>();
            m_regions = new List<LeagueTextRegion>();
            m_grabber = new LeagueGrabber(processName);
            m_ocr = new DontOcrMeBro();
        }

        public void AddRegionGrab(LeagueTextRegion region)
        {
            lock(m_regions)
            {
                m_regions.Add(region);
            }
        }

        public List<LeagueTextRegionUpdate> Update()
        {
            DateTime start = DateTime.Now;
            Bitmap leagueScreen = m_grabber.Capture();
            Bitmap crispyScreen = Crispen(leagueScreen, 100f, 150);
            if (leagueScreen == null)
            {
                return null;
            }

            List<LeagueTextRegionUpdate> updates = new List<LeagueTextRegionUpdate>();
            lock (m_regions)
            {
                foreach (var region in m_regions)
                {
                    // Try to ocr each region.
                    LeagueTextRegionUpdate up = new LeagueTextRegionUpdate()
                    {
                        TextRegion = region,
                        Success = false
                    };
                    var res = GetMeDatText(region.UseCrispyImage ? crispyScreen : leagueScreen, region.Region);
                    if(res != null)
                    {
                        up.Success = true;
                        up.Text = res.Item1;
                        up.Confidence = res.Item2;
                    }
                    if(region.SendBackBitmap)
                    {
                        up.Bitmap = BitmapGetLeagueRegion(leagueScreen, region.Region);
                        up.CrispyBitmap = BitmapGetLeagueRegion(crispyScreen, region.Region);
                    }
                    updates.Add(up);
                }
            }

            lock (m_avgTime)
            {
                m_avgTime.Add((DateTime.Now - start).TotalMilliseconds);
                if (m_avgTime.Count > 50)
                {
                    m_avgTime.RemoveAt(0);
                }
            }

            return updates;
        }

        public int GetAvgLoopTimeMs()
        {
            lock (m_avgTime)
            {
                if(m_avgTime.Count == 0)
                {
                    return -1;
                }
                int accum = 0;
                foreach(var val in m_avgTime)
                {
                    accum = (int)val;
                }
                return accum / m_avgTime.Count;
            }
        }

        private Tuple<string,float> GetMeDatText(Bitmap input, LeagueRegion searchRect)
        {
            if(input == null)
            {
                return null;
            }
            var rect = LeagueRegionToRect(input.Size, searchRect);
            if (!rect.HasValue)
            {
                return null;
            }
            return m_ocr.GetMeDatSingleLineText(input, new Rect(rect.Value.X, rect.Value.Y, rect.Value.Width, rect.Value.Height));
        }

        private Rectangle? LeagueRegionToRect(Size ogSize, LeagueRegion rect)
        {
            if (ogSize.Width == 0 || ogSize.Height == 0)
            {
                return null;
            }
            if (rect.Left < 0 || rect.Left > 100 ||
                rect.Right < 0 || rect.Right > 100 ||
                rect.Top < 0 || rect.Top > 100 ||
                rect.Bottom < 0 || rect.Bottom > 100 ||
                rect.Right < rect.Left ||
                rect.Bottom < rect.Top)
            {
                Logger.Info("Inalid rect bounds passed to get region");
                return null;
            }

            // Convert from the % based value to the bitmap
            Rectangle newRec = new Rectangle();
            newRec.X = (int)(rect.Left / 100.0 * (double)ogSize.Width);
            newRec.Y = (int)(rect.Top / 100.0 * (double)ogSize.Height);
            newRec.Height = (int)(rect.Bottom / 100.0 * (double)ogSize.Height) - newRec.Y;
            newRec.Width = (int)(rect.Right / 100.0 * (double)ogSize.Width) - newRec.X;

            // Make sure the params are still valid.
            if (newRec.Height == 0 || newRec.Width == 0)
            {
                return null;
            }
            return newRec;
        }

        private Bitmap BitmapGetLeagueRegion(Bitmap input, LeagueRegion cropRect)
        {
            if (input == null)
            {
                return null;
            }
            var rect = LeagueRegionToRect(input.Size, cropRect);
            if (!rect.HasValue)
            {
                return null;
            }
            return GetRegion(input, rect.Value);
        }

        private Bitmap GetRegion(Bitmap input, Rectangle cropRect)
        {
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(input, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            return target;
        }

        public static Bitmap Crispen(Bitmap image, float contrast, int binaryLevel)
        {
            if(image == null)
            {
                return null;
            }
            contrast = (100.0f + contrast) / 100.0f;
            contrast *= contrast;
            Bitmap NewBitmap = (Bitmap)image.Clone();
            BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
            int Height = NewBitmap.Height;
            int Width = NewBitmap.Width;

            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;
                    for (int x = 0; x < Width; ++x)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];

                        float Red = R / 255.0f;
                        float Green = G / 255.0f;
                        float Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * contrast) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * contrast) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * contrast) + 0.5f) * 255.0f;

                        int iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        // Snap the pixel to be a binary (white or black) 
                        if(iB > binaryLevel || iG > binaryLevel || iR > binaryLevel)
                        {
                            iB = iG = iR = 255;
                        }
                        else
                        {
                            iB = iG = iR = 0;
                        }

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }

            NewBitmap.UnlockBits(data);

            return NewBitmap;
        }
    }
}
