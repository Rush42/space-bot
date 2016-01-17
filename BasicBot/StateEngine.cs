namespace CharonBot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    public class StateEngine
    {
        private readonly Map gameMap;

        private int mapMiddleIndex;

        private int round;

        private readonly PlayerJson otherGuy;

        private bool moveLeftToBuild;

        private bool moveRightToBuild;

        private Tuple<Wave, int> waveAndWait;

        public StateEngine(string pathToJson)
        {
            JObject jobj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(pathToJson));

            JToken token = jobj.SelectToken("Map");
            this.gameMap = token.ToObject<Map>();

            JToken round = jobj.SelectToken("RoundNumber");
            this.round = round.ToObject<int>();

            this.mapMiddleIndex = Convert.ToInt32(Math.Round(((double)this.gameMap.Height / (double)2), 0, MidpointRounding.AwayFromZero));

            token = jobj.SelectToken("Players");
            PlayerJson[] players = token.ToObject<PlayerJson[]>();

            foreach (PlayerJson player in players)
            {
                if (player.PlayerNumber == 1)
                {
                    this.Deity = player;
                }
                else
                {
                    this.otherGuy = player;
                }
            }
        }

        public PlayerJson Deity { get; set; }

        public string TryDodge()
        {
            string command = this.DodgeInSameRow();


            if (command == null)
            {
             command = this.DodgeTwoAbove();
            }

            if (command == null)
            {
                command = this.DodgeThreeAbove();
            }

            if (command == null)
            {
                command = this.DodgeFourAbove();
            }
            return command;
        }

        private string DodgeInSameRow()
        {
            Row leftRow = this.gameMap.Rows[this.Deity.Ship.Y][this.Deity.Ship.X - 1];

            if (leftRow != null && leftRow.Type.Equals("Alien"))
            {
                return CharonBot.Moves.MoveRight.ToString();
            }

            Row rightRow = this.gameMap.Rows[this.Deity.Ship.Y][this.Deity.Ship.X + 3];

            if (rightRow != null && rightRow.Type.Equals("Alien"))
            {
                return CharonBot.Moves.MoveLeft.ToString();
            }

            return null;
        }

        private string DodgeFourAbove()
        {
            Row left = this.gameMap.Rows[this.Deity.Ship.Y - 4][this.Deity.Ship.X];
            Row right = this.gameMap.Rows[this.Deity.Ship.Y - 4][this.Deity.Ship.X + 2];

            if (left == null || right == null)
            {
                return null;
            }

            if (this.CheckDangerDeep(left.Y, left.X) && this.CheckDangerDeep(right.Y, right.X))
            {
                if (this.IsMoveLeftBest() && this.CanMoveLeft(this.Deity.Ship.X))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }

                if (this.CanMoveRight(this.Deity.Ship.X))
                {
                    return CharonBot.Moves.MoveRight.ToString();
                }

                if (this.CanMoveLeft(this.Deity.Ship.X))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }
            }

            return null;
        }

        private string DodgeThreeAbove()
        {
            Row[] rowTwoAbove = gameMap.Rows[Deity.Ship.Y - 3];

            Row aboveMiddle = rowTwoAbove[Deity.Ship.X + 1];

            bool danger = false;

            foreach (Missile missile in otherGuy.Missiles)
            {
                if (missile.X == Deity.Ship.X + 1 && missile.Y == Deity.Ship.Y - 3)
                {
                    danger = true;

                }
            }

            if (aboveMiddle == null)
            {
                return null;
            }

            if (this.CheckDangerDeep(aboveMiddle.Y, aboveMiddle.X))
            {
                danger = true;
            }

            if (danger)
            {
                if (IsMoveLeftBest() && CanMoveLeft(Deity.Ship.X)) return CharonBot.Moves.MoveLeft.ToString();
                if (!IsMoveLeftBest() && CanMoveRight(Deity.Ship.X)) return CharonBot.Moves.MoveRight.ToString();
                if (CanShoot() && FindBlockage(Deity.Ship.Y, aboveMiddle.Y, aboveMiddle.X) == null) return CharonBot.Moves.Shoot.ToString();
                if (this.CanMoveRight(Deity.Ship.X)) return CharonBot.Moves.MoveRight.ToString();
                if (CanMoveLeft(Deity.Ship.X)) return CharonBot.Moves.MoveLeft.ToString();

            }

            Row left = rowTwoAbove[this.Deity.Ship.X];
            Row right = rowTwoAbove[this.Deity.Ship.X + 2];

            if (left == null || right == null)
            {
                return null;
            }

            if (this.CheckDangerDeep(left.Y, left.X) && this.CheckDangerDeep(right.Y, right.X))
            {
                if (this.IsMoveLeftBest() && this.CanMoveLeft(this.Deity.Ship.X))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }

                if (this.CanMoveRight(this.Deity.Ship.X))
                {
                    return CharonBot.Moves.MoveRight.ToString();
                }

                if (this.CanMoveLeft(this.Deity.Ship.X))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }
            }

            return null;
        }

        public bool IsMoveLeftBest()
        {
            double totalLength = gameMap.Rows[0].Length;
            // The middle position was calculated like this: int middleShipX = deity.Ship.X + 1;
            // But it must be zero-based. So just check the normal X as it makes up +1 for the middle and -1 to get it zero-based

            if (totalLength / 2 > (double)Deity.Ship.X) return false; //check to see if we're past midway in the X plane
            return true; // If we're not past midway, say LEFT is BEST
        }

        private Row FindBlockage(int startY, int finishY, int X)
        {
            // Ensures only null blocks between the two points. Aliens are ok because hitting them would be great!
            for (int y = startY - 1; y > finishY; y--)
            {
                Row blockage = this.gameMap.Rows[y][X];

                if (blockage != null && !blockage.Type.Equals("Alien"))
                {
                    if (blockage.Type.Equals("Missile") && blockage.PlayerNumber == 1)
                    {
                        continue;
                    }

                    return blockage;
                }
            }

            return null;
        }

        public string DodgeTwoAbove() // Is something dangerous 2 Y positions above the ship?
        {
            foreach (Missile missile in otherGuy.Missiles)
            {
                if (missile.Y == Deity.Ship.Y - 2) //If the missile is 2 rows above
                {
                    if (missile.X == Deity.Ship.X && CanMoveLeft(Deity.Ship.X)) return CharonBot.Moves.MoveRight.ToString(); //If it's on the left side, move right to dodge
                    if (missile.X - 2 == Deity.Ship.X && CanMoveRight(Deity.Ship.X)) return CharonBot.Moves.MoveLeft.ToString(); //If it's on the right
                    if (CanShoot()) return CharonBot.Moves.Shoot.ToString(); //If it's in the centre
                }
            }

            Row[] rowAbove = gameMap.Rows[Deity.Ship.Y - 2];

            for (int i = Deity.Ship.X; i < Deity.Ship.X + 3; i++)
            {
                Row x = rowAbove[i];

                if (x == null)
                {
                    continue;
                }

                if (x.Type.Equals("Bullet"))
                {
                    if (x.X == Deity.Ship.X && CanMoveRight(Deity.Ship.X)) return CharonBot.Moves.MoveRight.ToString(); //If it's on the left side, move right to dodge
                    if (x.X - 2 == Deity.Ship.X && CanMoveLeft(Deity.Ship.X)) return CharonBot.Moves.MoveLeft.ToString(); //If it's on the right
                    if (CanShoot()) return CharonBot.Moves.Shoot.ToString(); //If it's in the centre
                }
            }

            return null;
        }

        private bool CanShoot()
        {
            if (Deity.Missiles.Length < Deity.MissileLimit)
            {
                return true;
            }
            return false;
        }

        // XPosition is the left block of where the ship currently is
        public bool CanMoveLeft(int XPosition)
        {
            Row toCheck = gameMap.Rows[Deity.Ship.Y][XPosition - 1];

            if (toCheck == null)
            {
                return true;
            }

            if (XPosition < this.Deity.Ship.X)
            {
                if (this.CheckDangerDeep(toCheck.Y, toCheck.X))
                {
                    return true;
                }
            }

            return false;
        }

        // XPosition is the left block of where the ship currently is
        public bool CanMoveRight(int XPosition)
        {
            Row toCheck = gameMap.Rows[Deity.Ship.Y][XPosition + 3];
            if (toCheck == null)
            {
                return true;
            }

            if (XPosition > this.Deity.Ship.X)
            {
                if (this.CheckDangerDeep(toCheck.Y, toCheck.X))
                {
                    return true;
                }
            }

            return false;
        }

        // This wrapper method to CalculateShot tries to see if a shot in one or two later rounds would shoot down closer aliens.
        public string TryShoot()
        {
            if (!this.CanShoot())
            {
                return null;
            }

            Wave waveNow = this.CalculateShot(0);

            if (waveNow == null)
            {
                return null;
            }

            // Checking for danger three above the ship. Don't want to wait to take a shot, and then be hit because of it.
            if (!this.CheckDangerDeep(this.Deity.Ship.Y - 3, this.Deity.Ship.X) && !this.CheckDangerDeep(this.Deity.Ship.Y - 3, this.Deity.Ship.X + 2))
            {
                Wave waveInOneRound = this.CalculateShot(1);

                if (waveInOneRound != null && waveInOneRound.Y > waveNow.Y)
                {
                    this.waveAndWait = new Tuple<Wave, int>(waveInOneRound, 1);
                    return null;
                }

                // Checking for danger four above the ship.
                if (!this.CheckDangerDeep(this.Deity.Ship.Y - 4, this.Deity.Ship.X) && !this.CheckDangerDeep(this.Deity.Ship.Y - 4, this.Deity.Ship.X + 2))
                {
                    Wave waveInTwoRounds = this.CalculateShot(2);

                    if (waveInTwoRounds != null && waveInTwoRounds.Y > waveNow.Y)
                    {
                        this.waveAndWait = new Tuple<Wave, int>(waveInTwoRounds, 2);
                        return null;
                    }

                    // Checking for danger five above the ship.
                    if (!this.CheckDangerDeep(this.Deity.Ship.Y - 5, this.Deity.Ship.X) && !this.CheckDangerDeep(this.Deity.Ship.Y - 5, this.Deity.Ship.X + 2))
                    {
                        Wave waveInThreeRounds = this.CalculateShot(3);

                        if (waveInThreeRounds != null && waveInThreeRounds.Y > waveNow.Y + 1)
                        {
                            this.waveAndWait = new Tuple<Wave, int>(waveInThreeRounds, 3);
                            return null;
                        }
                    }
                }
            }

            string deathFile = "MarkedForDeath.txt";
            if (File.Exists(deathFile))
            {
                string id = "-2";
                bool successfulRead = true;
                try
                {
                    id = File.ReadAllText(deathFile);
                }
                catch (Exception)
                {
                    successfulRead = false;
                    Console.WriteLine("Could not read MarkedForDeath.txt!");
#if DEBUG
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
#endif
                }

                int deathId = -1;
                if (int.TryParse(id, out deathId) && successfulRead)
                {
                    if (deathId == waveNow.Id)
                    {
                        return null;
                    }
                }
            }

            try
            {
                File.WriteAllText(deathFile, waveNow.Id.ToString());
            }
            catch (Exception)
            {
                Console.WriteLine("Could not write to MarkedForDath.txt!");
#if DEBUG
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
#endif
            }

            return CharonBot.Moves.Shoot.ToString();
        }

        public string TryShift()
        {
            bool tooClose =
                (this.gameMap.Rows[this.Deity.Ship.Y].Any(
                    r => r != null && r.Type == "Alien" && (r.X > this.Deity.Ship.X - 2 || r.X < this.Deity.Ship.X + 5)))
                || (this.gameMap.Rows[this.Deity.Ship.Y - 1].Any(
                    r => r != null && r.Type == "Alien" && (r.X > this.Deity.Ship.X - 2 || r.X < this.Deity.Ship.X + 5)))
                || (this.gameMap.Rows[this.Deity.Ship.Y - 2].Any(
                    r => r != null && r.Type == "Alien" && (r.X > this.Deity.Ship.X - 2 || r.X < this.Deity.Ship.X + 5)));

            if (tooClose)
            {
                return null;
            }

            Alienmanager manager = new Alienmanager()
                                       {
                                           DeltaX = this.otherGuy.AlienManager.DeltaX,
                                           Disabled = this.otherGuy.AlienManager.Disabled,
                                           PlayerNumber = this.otherGuy.PlayerNumber,
                                           ShotEnergy = this.otherGuy.AlienManager.ShotEnergy,
                                           ShotEnergyCost = this.otherGuy.AlienManager.ShotEnergyCost,
                                           Waves =
                                               CharonBot.WaveArrayDeepCopy(this.otherGuy.AlienManager.Waves)
                                       };
            List<Wave[]> orderedWaves = new List<Wave[]>(manager.Waves.OrderByDescending(w => w.First().Y).ToList());

            // For each row of aliens
            for (int wait = 1; wait < 5; wait++)
            {
                foreach (Wave[] waves in orderedWaves)
                {
                    var wavesAndDelta = this.PredictAlienPositions(
                        orderedWaves,
                        this.Deity.Ship.Y - waves[0].Y + wait);

                    List<Wave[]> predictedWaves = wavesAndDelta.Item1;

                    int leftOrRight = wavesAndDelta.Item2 < 0 ? -1 : 1;

                    foreach (Wave[] goatWave in predictedWaves)
                    {
                        foreach (Wave alien in goatWave)
                        {
                            // Use wait to check if the alien would be by left/right of me. Move in that direction to meet it in due time
                            if (waves.Any(a => a.Id == alien.Id))
                            {
                                if (((alien.X == this.Deity.Ship.X + 1 + wait) || (alien.X == this.Deity.Ship.X + 1 + leftOrRight + wait)))
                                {
                                    if (this.waveAndWait == null
                                        || (this.waveAndWait != null && alien.Y < this.waveAndWait.Item1.Y
                                            && wait < this.waveAndWait.Item2))
                                    {
                                        if (this.FindBlockage(this.Deity.Ship.Y, alien.Y, alien.X) == null
                                            && this.CanMoveRight(this.Deity.Ship.X)
                                            && !this.CheckDangerDeep(this.Deity.Ship.Y - 3, this.Deity.Ship.X + 2)
                                            && !this.CheckDangerDeep(this.Deity.Ship.Y - 2, this.Deity.Ship.X + 3)
                                            && alien.Y < this.Deity.Ship.Y - 2)
                                        {
                                            this.waveAndWait = null;
                                            return CharonBot.Moves.MoveRight.ToString();
                                        }
                                    }
                                }

                                if (((alien.X == this.Deity.Ship.X + 1 - wait) || (alien.X == this.Deity.Ship.X + 1 + leftOrRight - wait)))
                                {
                                    if (this.waveAndWait == null
                                        || (this.waveAndWait != null && alien.Y < this.waveAndWait.Item1.Y
                                            && wait < this.waveAndWait.Item2))
                                    {
                                        if (this.FindBlockage(this.Deity.Ship.Y, alien.Y, alien.X) == null
                                            && this.CanMoveLeft(this.Deity.Ship.X)
                                            && !this.CheckDangerDeep(this.Deity.Ship.Y - 3, this.Deity.Ship.X)
                                            && !this.CheckDangerDeep(this.Deity.Ship.Y - 2, this.Deity.Ship.X - 1)
                                            && alien.Y < this.Deity.Ship.Y - 2)
                                        {
                                            this.waveAndWait = null;
                                            return CharonBot.Moves.MoveLeft.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.waveAndWait = null;
            return null;
        }

        private Wave CalculateShot(int waitingPeriod)
        {
            Alienmanager manager = new Alienmanager()
            {
                DeltaX = this.otherGuy.AlienManager.DeltaX,
                Disabled = this.otherGuy.AlienManager.Disabled,
                PlayerNumber = this.otherGuy.PlayerNumber,
                ShotEnergy = this.otherGuy.AlienManager.ShotEnergy,
                ShotEnergyCost = this.otherGuy.AlienManager.ShotEnergyCost,
                Waves = CharonBot.WaveArrayDeepCopy(this.otherGuy.AlienManager.Waves)

            };
            List<Wave[]> orderedWaves = new List<Wave[]>(manager.Waves.OrderByDescending(w => w.First().Y).ToList());          

            foreach (Wave[] waves in orderedWaves)
            {
                // For each row of aliens
                var wavesAndDelta = this.PredictAlienPositions(CharonBot.WaveListDeepCopy(orderedWaves), this.Deity.Ship.Y - waves[0].Y + waitingPeriod);

                List<Wave[]> predictedWaves = wavesAndDelta.Item1;
                int leftOrRight = wavesAndDelta.Item2 < 0 ? -1 : 1;

                // For each alien in the row, was it hit?
                foreach (Wave[] goatWave in predictedWaves)
                {
                    foreach (Wave alien in goatWave)
                    {
                        if (((alien.X == this.Deity.Ship.X + 1) || (alien.X == this.Deity.Ship.X + 1 + leftOrRight)) && waves.Any(a => a.Id == alien.Id))
                        {
                            if (this.CanShoot() && this.FindBlockage(this.Deity.Ship.Y, alien.Y, this.Deity.Ship.X + 1) == null)
                            {
                                return alien;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // The waitingPeriod is used to update the aliens for n extra turns, as if you were not going to shoot this round but rather wait n turns and want to see if it would be successful
        private Tuple<List<Wave[]>, int> PredictAlienPositions(List<Wave[]> waves, int roundsToCalculate)
        {
            waves = new List<Wave[]>(waves);

            Alienmanager manager = new Alienmanager()
            {
                DeltaX = this.otherGuy.AlienManager.DeltaX,
                Disabled = this.otherGuy.AlienManager.Disabled,
                PlayerNumber = this.otherGuy.PlayerNumber,
                ShotEnergy = this.otherGuy.AlienManager.ShotEnergy,
                ShotEnergyCost = this.otherGuy.AlienManager.ShotEnergyCost,
                Waves = CharonBot.WaveArrayDeepCopy(this.otherGuy.AlienManager.Waves)
            };



            for (int x = 0; x < roundsToCalculate; x++) // Count through the moves that it would take to hit the alien
            {
                    // The offset takes into account the  fact that the aliens are in a different row (due to moving down) and would be in a different horizontal position.
                    // We check at the last predicted round in what position is the alien? But due to moving down we must check one row before..
                    // Instead, look at the last row, but take into account the fact that we should check one row before (so basically just roll back the alien by one move per move down)

                    bool topRowMySideContainsAlien = this.gameMap.Rows[this.mapMiddleIndex].Any(r => r != null && r.Type.Equals("Alien"));
                    bool secondTopRowMySideContainsAlien = this.gameMap.Rows[this.mapMiddleIndex + 1].Any(r => r != null && r.Type.Equals("Alien"));

                    Wave[] possibleLefts = new Wave[waves.Count];
                    Wave[] possibleRights = new Wave[waves.Count];

                    for (int i = 0; i < waves.Count; i++)
                    {
                        possibleLefts[i] = waves[i].Aggregate((w1, w2) => w1.X < w2.X ? w1 : w2);
                    }

                    for (int i = 0; i < waves.Count; i++)
                    {
                        possibleRights[i] = waves[i].Aggregate((w1, w2) => w1.X > w2.X ? w1 : w2);
                    }

                    Wave leftMost = possibleLefts.Aggregate((w1, w2) => w1.X < w2.X ? w1 : w2);
                    Wave rightMost = possibleRights.Aggregate((w1, w2) => w1.X > w2.X ? w1 : w2);

                    Row atLeft = leftMost.X > 0 && leftMost.Y < this.gameMap.Height ? this.gameMap.Rows[leftMost.Y][leftMost.X - 1] : null;
                    Row atRight = rightMost.X < this.gameMap.Width - 1 && rightMost.Y < this.gameMap.Height ? this.gameMap.Rows[rightMost.Y][rightMost.X + 1] : null;

                    // Aliens moving right
                    if (manager.DeltaX > 0) 
                    {
                        // We spawn aliens after we have moved down, and when we are about to move in a new direction
                        if (atLeft != null && atLeft.Type.Equals("Wall") && !topRowMySideContainsAlien && !secondTopRowMySideContainsAlien)
                        {
                            waves = this.SpawnAliens(waves, true);
                        }

                        if (atRight != null && atRight.Type.Equals("Wall"))
                        {
                            foreach (Wave[] alienWave in waves)
                            {
                                foreach (Wave alien in alienWave)
                                {
                                    alien.Y++;
                                }
                            }

                            roundsToCalculate--;
                            manager.DeltaX = -1;
                        }
                        else
                        {
                            foreach (Wave[] alienWave in waves)
                            {
                                foreach (Wave alien in alienWave)
                                {
                                    alien.X++;
                                }
                            }
                        }
                    }
                    else // Aliens moving left
                    {
                        if (atRight != null && atRight.Type.Equals("Wall") && !topRowMySideContainsAlien && !secondTopRowMySideContainsAlien)
                        {
                            waves = this.SpawnAliens(waves, false);
                        }

                        if (atLeft != null && atLeft.Type.Equals("Wall"))
                        {
                            foreach (Wave[] alienWave in waves)
                            {
                                foreach (Wave alien in alienWave)
                                {
                                    alien.Y++;
                                }
                            }

                            roundsToCalculate--;
                            manager.DeltaX = 1;
                        }
                        else
                        {
                            foreach (Wave[] alienWave in waves)
                            {
                                foreach (Wave alien in alienWave)
                                {
                                    alien.X--;
                                }
                            }
                        }
                    }
            }

            return new Tuple<List<Wave[]>, int>(new List<Wave[]>(waves), manager.DeltaX);
        }

        private List<Wave[]> SpawnAliens(List<Wave[]> alienWaves, bool isAtLeft)
        {
            Wave[] highestWave = alienWaves.Aggregate((w1, w2) => w1[0].Y < w2[0].Y ? w1 : w2);

            int newAliensCount = 3;

            if (otherGuy.AlienFactory != null)
            {
                newAliensCount++;
            }

            if (this.round > 40)
            {
                newAliensCount++;
            }

            Wave[] newWave = new Wave[newAliensCount];

            int xPosition = isAtLeft ? 2 : this.gameMap.Width - 3;

            for (int i = 0; i < newAliensCount; i++)
            {
                newWave[i] = new Wave()
                {
                    Alive = true,
                    Height = 1,
                    Id = 123,
                    PlayerNumber = this.otherGuy.PlayerNumber,
                    Type = highestWave[0].Type,
                    Width = 1,
                    X = xPosition,
                    Y = this.mapMiddleIndex + 1
                };

                gameMap.Rows[newWave[i].Y][newWave[i].X] = new Row()
                {
                    Alive = true,
                    Height = 1,
                    Id = 123,
                    PlayerNumber = this.otherGuy.PlayerNumber,
                    Type = highestWave[0].Type,
                    Width = 1,
                    X = xPosition,
                    Y = this.mapMiddleIndex + 1,
                };

                xPosition = isAtLeft ? xPosition + 3 : xPosition - 3;
            }

            alienWaves.Add(newWave);
            return alienWaves;
        }

        public string TryBuild()
        {
            // TODO: Trial and error, which is best, missilecontroller, alienmanager, or both? Maybe no shields are needed?

            // SHIELDS
            //int shields =
            //    this.gameMap.Rows.SelectMany(rows => rows)
            //        .Count(row => row != null && row.Type == "Shield" && row.PlayerNumber == 1);
            //if (this.Deity.Lives > 0 && shields < 25)
            //{
            //    bool canBuildShields = true;

            //    for (int y = this.Deity.Ship.Y - 1; y > this.Deity.Ship.Y - 4; y--)
            //    {
            //        if (!canBuildShields)
            //        {
            //            break;
            //        }

            //        for (int x = this.Deity.Ship.X; x < this.Deity.Ship.X + 3; x++)
            //        {
            //            if (this.gameMap.Rows[y][x] != null)
            //            {
            //                canBuildShields = false;
            //                break;
            //            }
            //        }
            //    }

            //    if (canBuildShields)
            //    {
            //        return CharonBot.Moves.BuildShield.ToString();
            //    }
            //}

            Row blockageLeft = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 10, this.Deity.Ship.X);
            Row blockageMiddle = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 10, this.Deity.Ship.X + 1);
            Row blockageRight = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 10, this.Deity.Ship.X + 2);

            if ((this.Deity.MissileController != null || this.otherGuy.AlienFactory == null) && this.Deity.AlienFactory != null)
            {
                return null;
            }

            if (blockageLeft == null || !blockageLeft.Type.Equals("Shield") || blockageMiddle == null
                || !blockageMiddle.Type.Equals("Shield") ||
            blockageRight == null || !blockageRight.Type.Equals("Shield"))
            {
                // There is no coverage. Try move to a side to build
                Row leftPosition1 = this.gameMap.Rows[this.Deity.Ship.Y + 1][2];
                Row leftPosition2 = this.gameMap.Rows[this.Deity.Ship.Y + 1][3];
                Row leftPosition3 = this.gameMap.Rows[this.Deity.Ship.Y + 1][4];

                if (leftPosition1 == null && leftPosition2 == null && leftPosition3 == null)
                {
                    this.moveLeftToBuild = true;
                    return null;
                }

                Row rightPosition1 = this.gameMap.Rows[this.Deity.Ship.Y + 1][14];
                Row rightPosition2 = this.gameMap.Rows[this.Deity.Ship.Y + 1][15];
                Row rightPosition3 = this.gameMap.Rows[this.Deity.Ship.Y + 1][16];

                if (rightPosition1 == null && rightPosition2 == null && rightPosition3 == null)
                {
                    this.moveRightToBuild = true;
                    return null;
                }
            }

            // ALIENFACTORY
            if (this.Deity.Lives > 0 && this.Deity.AlienFactory == null)
            {
                bool canBuildFactory = true;

                for (int x = this.Deity.Ship.X; x < this.Deity.Ship.X + 3; x++)
                {
                    if (this.gameMap.Rows[this.Deity.Ship.Y + 1][x] != null)
                    {
                        canBuildFactory = false;
                        break;
                    }
                }

                if (canBuildFactory)
                {
                    return CharonBot.Moves.BuildAlienFactory.ToString();
                }
            }

            // MISSILECONTROLLER
            // Only build a missile controller if the other guy builds an alien factory
            if (this.Deity.Lives > 0 && (this.Deity.MissileController == null && this.otherGuy.AlienFactory != null))
            {
                bool canBuildController = true;

                for (int x = this.Deity.Ship.X; x < this.Deity.Ship.X + 3; x++)
                {
                    if (this.gameMap.Rows[this.Deity.Ship.Y + 1][x] != null)
                    {
                        canBuildController = false;
                        break;
                    }
                }

                if (canBuildController)
                {
                    return CharonBot.Moves.BuildMissileController.ToString();
                }
            }

            return null;
        }

        public string TryPlace()
        {
            Row leftTwoBlockages = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 4, this.Deity.Ship.X - 1);
            Row rightTwoBlockages = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 4, this.Deity.Ship.X + 3);

            if (this.moveLeftToBuild && this.CanMoveLeft(this.Deity.Ship.X) && !this.IsDangerous(this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 5, this.Deity.Ship.X - 1)))
            {
                return CharonBot.Moves.MoveLeft.ToString();
            }
            else if (this.moveRightToBuild && this.CanMoveRight(this.Deity.Ship.X) && !this.IsDangerous(this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 5, this.Deity.Ship.X + 3)))
            {
                return CharonBot.Moves.MoveRight.ToString();
            }

            Row blockageMiddle = this.FindBlockage(this.Deity.Ship.Y, this.gameMap.Height / 2, this.Deity.Ship.X + 1);

            if (blockageMiddle == null && this.gameMap.Rows[this.Deity.Ship.Y + 1][this.Deity.Ship.X] == null && this.gameMap.Rows[this.Deity.Ship.Y + 1][this.Deity.Ship.X + 2] == null && this.gameMap.Rows[this.Deity.Ship.Y + 1][this.Deity.Ship.X + 3] == null)
            {
                return null;
            }

            Row blockageLeft = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 4, this.Deity.Ship.X);
            Row blockageRight = this.FindBlockage(this.Deity.Ship.Y, this.Deity.Ship.Y - 4, this.Deity.Ship.X + 2);
            bool leftBest = this.IsMoveLeftBest();


            // Check most open direction and two spaces
            if (leftBest)
            {
                if (blockageLeft == null)
                {
                    if (leftTwoBlockages == null && this.CanMoveLeft(Deity.Ship.X))
                    {
                        return CharonBot.Moves.MoveLeft.ToString();
                    }
                }
            }
            else
            {
                if (blockageRight == null)
                {
                    if (rightTwoBlockages == null && this.CanMoveRight(Deity.Ship.X))
                    {
                        return CharonBot.Moves.MoveRight.ToString();
                    }
                }
            }

            // Check most open direction and one space
            if (leftBest)
            {
                if (this.LookDeepLeft(blockageLeft, leftTwoBlockages))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }

                // Something is dangerous at left 2nd row, so check right
                if (this.LookDeepRight(blockageRight, rightTwoBlockages))
                {
                    return CharonBot.Moves.MoveRight.ToString();
                }
            }
            else
            {
                // Right is most open and something must be at 2nd right so see if it's dangerous
                if (this.LookDeepRight(blockageRight, rightTwoBlockages))
                {
                    return CharonBot.Moves.MoveRight.ToString();
                }
                if (this.LookDeepLeft(blockageLeft, leftTwoBlockages))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }
            }

            if (leftBest)
            {
                if (leftTwoBlockages == null && this.CanMoveLeft(Deity.Ship.X) && this.CanMoveLeft(Deity.Ship.X - 1))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }
                else if (rightTwoBlockages == null && this.CanMoveRight(this.Deity.Ship.X)
                         && this.CanMoveRight(this.Deity.Ship.X + 1))
                {
                    return CharonBot.Moves.MoveRight.ToString();
                }
            }
            else
            {
                if (rightTwoBlockages == null && this.CanMoveRight(this.Deity.Ship.X)
                    && this.CanMoveRight(this.Deity.Ship.X + 1))
                {
                    return CharonBot.Moves.MoveRight.ToString();
                }
                else if (leftTwoBlockages == null && this.CanMoveLeft(Deity.Ship.X) && this.CanMoveLeft(Deity.Ship.X - 1))
                {
                    return CharonBot.Moves.MoveLeft.ToString();
                }
            }

            if (leftBest)
            {
                if (!this.IsDangerDeep(blockageLeft, leftTwoBlockages))
                {
                    if (this.CanMoveLeft(this.Deity.Ship.X) && this.CanMoveLeft(this.Deity.Ship.X - 1) && this.WillEverOpenLeft(this.Deity.Ship.X))
                    {
                        return CharonBot.Moves.MoveLeft.ToString();
                    }
                }

                if (!this.IsDangerDeep(blockageRight, rightTwoBlockages))
                {
                    if (this.CanMoveRight(this.Deity.Ship.X) && this.CanMoveRight(this.Deity.Ship.X + 1) && this.WillEverOpenRight(this.Deity.Ship.X))
                    {
                        return CharonBot.Moves.MoveRight.ToString();
                    }
                }
            }
            else
            {
                if (!this.IsDangerDeep(blockageRight, rightTwoBlockages))
                {
                    if (this.CanMoveRight(this.Deity.Ship.X) && this.CanMoveRight(this.Deity.Ship.X + 1) && this.WillEverOpenRight(this.Deity.Ship.X))
                    {
                        return CharonBot.Moves.MoveRight.ToString();
                    }
                }

                if (!this.IsDangerDeep(blockageLeft, leftTwoBlockages))
                {
                    if (this.CanMoveLeft(this.Deity.Ship.X) && this.CanMoveLeft(this.Deity.Ship.X - 1) && this.WillEverOpenLeft(this.Deity.Ship.X))
                    {
                        return CharonBot.Moves.MoveLeft.ToString();
                    }
                }
            }

            // N.B. commented this out because it doesn't feel safe. What if we move to a side and boom, there's a missile in the way? Not safe to move without checking. Not safe to put the ship purposefully in the way of a missile.
            /*
            // Check best direction and simple ability
            if (leftBest && this.CanMoveLeft())
            {
                return CharonBot.Moves.MoveLeft.ToString();
            }
            else if (this.CanMoveRight())
            {
                return CharonBot.Moves.MoveRight.ToString();
            }

            // If a move in a direction is possible, just take it
            if (this.CanMoveLeft())
            {
                return CharonBot.Moves.MoveLeft.ToString();
            }
            else if (this.CanMoveRight())
            {
                return CharonBot.Moves.MoveRight.ToString();
            }
             * */
            return null;
        }

        private bool WillEverOpenLeft(int XPosition)
        {
            for (int x = XPosition; x > 1; x--)
            {
                Row blockage = this.FindBlockage(this.Deity.Ship.Y, this.gameMap.Height / 2, x);

                if (blockage == null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WillEverOpenRight(int XPosition)
        {
            for (int x = XPosition; x < this.gameMap.Width - 2; x++)
            {
                Row blockage = this.FindBlockage(this.Deity.Ship.Y, this.gameMap.Height / 2, x);

                if (blockage == null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDangerous(Row danger)
        {
            if (danger != null
                && (danger.Type.Equals("Bullet") || (danger.Type.Equals("Missile") && danger.PlayerNumber == 2)))
            {
                return true;
            }

            return false;
        }

        private bool CheckDangerDeep(int topY, int X)
        {
            bool inDanger = false;

            for (int y = topY; y < this.Deity.Ship.Y; y++)
            {
                Row rowToCheck = this.gameMap.Rows[y][X];

                if (rowToCheck != null && rowToCheck.Type == "Shield")
                {
                    inDanger = false;
                }

                if (this.IsDangerous(rowToCheck))
                {
                    inDanger = true;
                }
            }

            return inDanger;
        }

        private bool IsDangerDeep(Row blockageOne, Row blockageTwo)
        {
            if (this.IsDangerous(blockageOne))
            {
                return true;
            }

            if (this.IsDangerous(blockageTwo))
            {
                return true;
            }

            return false;
        }

    private bool LookDeepLeft(Row blockageLeft, Row leftTwoBlockages)
        {
            if (blockageLeft == null && this.CanMoveLeft(Deity.Ship.X))
            {
                if (leftTwoBlockages != null)
                {
                    if (!this.IsDangerous(leftTwoBlockages))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private bool LookDeepRight(Row blockageRight, Row rightTwoBlockages)
        {
            if (blockageRight == null && this.CanMoveRight(Deity.Ship.X))
            {
                if (rightTwoBlockages != null)
                {
                    if (!this.IsDangerous(rightTwoBlockages))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }



    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Row[][] Rows { get; set; }
    }

    public class Row
    {
        public int Id { get; set; }
        public bool Alive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public int PlayerNumber { get; set; }
        public int LivesCost { get; set; }
        public string Command { get; set; }
        public string CommandFeedback { get; set; }
    }
}