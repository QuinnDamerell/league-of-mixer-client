using LeagueOfMixerClient.LeagueCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LeagueOfMixerClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static LeagueGameStream GameStream;

        public App()
        {
            GameStream = new LeagueGameStream("League of Legends");
            GameStream.Start();

        }
    }
}
