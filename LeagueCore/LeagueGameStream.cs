using LeagueOfMixerClient.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueOfMixerClient.LeagueCore
{
    public class LeagueGameStream
    {
        LeagueChopper m_chopper;
        Thread m_worker;

        public delegate void RawUpdate(List<LeagueTextRegionUpdate> updates);
        public event RawUpdate OnRawUpdate;
               
        public LeagueGameStream(string processName)
        {
            m_chopper = new LeagueChopper(processName);
        }

        public void Start()
        {
            // Add the current regions we pull.
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "PlayerScore",
                Region = new LeagueRegion(85.2, 89.7, 0, 3),
                SendBackBitmap = true
            });
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "EnmyKills",
                Region = new LeagueRegion(81.1, 83, 0, 3),
                SendBackBitmap = true
            });
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "YourTeamKills",
                Region = new LeagueRegion(78, 80.3, 0, 3),
                SendBackBitmap = true
            });
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "CsScore",
                Region = new LeagueRegion(91.8, 94, 0, 3),
                SendBackBitmap = true
            });
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "Time",
                Region = new LeagueRegion(96.3, 100, 0, 3),
                SendBackBitmap = true
            });
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "Gold",
                Region = new LeagueRegion(64, 68.8, 97, 99),
                SendBackBitmap = true
            });
            m_chopper.AddRegionGrab(new LeagueTextRegion()
            {
                Name = "Level",
                Region = new LeagueRegion(30.4, 31.5, 96.5, 98.1),
                SendBackBitmap = true
            });


            m_worker = new Thread(ThreadWorker);
            m_worker.Start();
        }

        private void ThreadWorker()
        {
            while(true)
            {
                var res = m_chopper.Update();
                // Always fire the update.
                if (OnRawUpdate != null)
                {
                    OnRawUpdate(res);
                }

                if (res != null)
                {                
                    foreach(var re in res)
                    {
                        if (re.Success)
                        {
                            Logger.Info($"Result {re.TextRegion.Name} {re.Text}");
                        }
                    }
                }
                Logger.Info($"Loop time: {m_chopper.GetAvgLoopTimeMs()}ms");
                Thread.Sleep(200);
            }
        }
    }
}
