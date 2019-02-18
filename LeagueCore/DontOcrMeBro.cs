using LeagueOfMixerClient.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace LeagueOfMixerClient.LeagueCore
{
    class DontOcrMeBro
    {
        TesseractEngine m_engine;

        public const string VAR_CHAR_WHITELIST = "tessedit_char_whitelist";
        public const string VAR_CHAR_BLACKLIST = "tessedit_char_blacklist";

        public DontOcrMeBro()
        {
            m_engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndCube);
            m_engine.SetVariable(VAR_CHAR_WHITELIST, "/01234567890:");
            m_engine.SetVariable(VAR_CHAR_BLACKLIST, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmopqrstuvwxyz");
        }

        public Tuple<string, float> GetMeDatSingleLineText(Bitmap bitmap, Rect searchSpace)
        {
            try
            {
                using (var page = m_engine.Process(bitmap, searchSpace, PageSegMode.SingleLine))
                {
                    return new Tuple<string, float>(page.GetText(), page.GetMeanConfidence());
                }
            }
            catch(Exception e)
            {
                Logger.Error("Failed to OCR text", e);
                return null;
            }
        }
    }
}
