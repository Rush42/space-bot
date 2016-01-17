using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharonBot
{
    public class PlayerJson
    {
        public int PlayerNumberReal { get; set; }
        public int PlayerNumber { get; set; }
        public string PlayerName { get; set; }
        public Ship Ship { get; set; }
        public int Kills { get; set; }
        public int Lives { get; set; }
        public int RespawnTimer { get; set; }
        public Missile[] Missiles { get; set; }
        public int MissileLimit { get; set; }
        public int AlienWaveSize { get; set; }
        public Alienfactory AlienFactory { get; set; }
        public object MissileController { get; set; }
        public Alienmanager AlienManager { get; set; }
    }

    public class Ship
    {
        public string Command { get; set; }
        public string CommandFeedback { get; set; }
        public int Id { get; set; }
        public bool Alive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public int PlayerNumber { get; set; }
    }

    public class Alienfactory
    {
        public int LivesCost { get; set; }
        public int Id { get; set; }
        public bool Alive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public int PlayerNumber { get; set; }
    }

    public class Alienmanager
    {
        public int PlayerNumber { get; set; }
        public bool Disabled { get; set; }
        public Wave[][] Waves { get; set; }
        public int ShotEnergyCost { get; set; }
        public int ShotEnergy { get; set; }
        public int DeltaX { get; set; }
    }

    [Serializable]
    public class Wave
    {
        public int Id { get; set; }
        public bool Alive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public int PlayerNumber { get; set; }
    }

    public class Missile
    {
        public int Id { get; set; }
        public bool Alive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public int PlayerNumber { get; set; }
    }

}
