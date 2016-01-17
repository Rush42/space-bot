using System;
using System.Diagnostics;
using System.IO;
using CharonBot.Properties;
using Newtonsoft.Json;
using SpaceInvaders;
using SpaceInvaders.Command;
using SpaceInvaders.Core;

namespace CharonBot
{
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;

    public class CharonBot
    {
        private StateEngine state;

        internal enum Moves
        {
            Nothing,

            MoveLeft,

            MoveRight,

            Shoot,

            BuildAlienFactory,

            BuildMissileController,

            BuildShield
        }

        public CharonBot(string outputPath)
        {
            OutputPath = outputPath;
            state = new StateEngine(@"output/state.json");
        }

        protected string OutputPath { get; private set; }

        public void Execute()
        {
            var match = LoadState();
            LogMatchState(match);

            var map = LoadMap();
            Log(String.Format("Map:{0}{1}", Environment.NewLine, map));

            string move = CreateMove();
            //END: Move logic
            SaveMove(move);
        }

        private string CreateMove()
        {
            if (state.Deity == null)
            {
                return "Nothing";
            }

            string move = state.TryDodge();

            if (move != null)
            {
                return move;
            }

            move = state.TryShoot();

            if (move != null)
            {
                return move;
            }

            move = state.TryBuild();

            if (move != null)
            {
                return move;
            }

            move = this.state.TryPlace();

            if (move != null)
            {
                return move;
            }

            move = this.state.TryShift();

            if (move != null)
            {
                return move;
            }
            
            //move = state.TryBuild();
            //return GetRandomMove();
            return CharonBot.Moves.Nothing.ToString();
        }

        private Match LoadState()
        {
            var filename = Path.Combine(OutputPath, Settings.Default.StateFile);
            try
            {
                string jsonText;
                using (var file = new StreamReader(filename))
                {
                    jsonText = file.ReadToEnd();
                }

                return DeserializeState(jsonText);
            }
            catch (IOException e)
            {
                Log(String.Format("Unable to read state file: {0}", filename));
                var trace = new StackTrace(e);
                Log(String.Format("Stacktrace: {0}", trace));
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return null;
            }
        }

        private static Match DeserializeState(string jsonText)
        {
            var match = JsonConvert.DeserializeObject<Match>(
                jsonText,
                new JsonSerializerSettings
                    {
                        Converters = { new EntityConverter() },
                        NullValueHandling = NullValueHandling.Ignore
                    });
            return match;
        }

        private void LogMatchState(Match match)
        {
            Log("Game state:");
            Log(String.Format("\tRound Number: {0}", match.RoundNumber));

            foreach (var player in match.Players)
            {
                LogPlayerState(player);
            }
        }

        private void LogPlayerState(Player player)
        {
            Log(String.Format("\tPlayer {0} Kills: {1}", player.PlayerNumber, player.Kills));
            Log(String.Format("\tPlayer {0} Lives: {1}", player.PlayerNumber, player.Lives));
            Log(
                String.Format(
                    "\tPlayer {0} Missiles: {1}/{2}",
                    player.PlayerNumber,
                    player.Missiles.Count,
                    player.MissileLimit));
        }

        private string LoadMap()
        {
            var filename = Path.Combine(OutputPath, Settings.Default.MapFile);
            try
            {
                using (var file = new StreamReader(filename))
                {
                    return file.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                Log(String.Format("Unable to read map file: {0}", filename));
                var trace = new StackTrace(e);
                Log(String.Format("Stacktrace: {0}", trace));
                return "Failed to load map!";
            }
        }

        private string GetRandomMove()
        {
            var random = new Random();
            var possibleMoves = Enum.GetValues(typeof(ShipCommand));
            return possibleMoves.GetValue(random.Next(0, possibleMoves.Length)).ToString();
        }

        private void SaveMove(string move)
        {
            var filename = Path.Combine(OutputPath, Settings.Default.OutputFile);
            try
            {
                using (var file = new StreamWriter(filename))
                {
                    file.WriteLine(move);
                }

                Log("Move: " + move);
            }
            catch (IOException e)
            {
                Log(String.Format("Unable to write move file: {0}", filename));

                var trace = new StackTrace(e);
                Log(String.Format("Stacktrace: {0}", trace));
            }
        }

        private void Log(string message)
        {
            Console.WriteLine("[BOT]\t{0}", message);
        }

        public static Wave[][] WaveArrayDeepCopy(Wave[][] waveToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, waveToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (Wave[][])binaryFormatter.Deserialize(memoryStream);
            }
        }

        public static List<Wave[]> WaveListDeepCopy(List<Wave[]> listToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, listToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (List<Wave[]>)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}