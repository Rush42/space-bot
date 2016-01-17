using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharonBot
{
    class Tests
    {
        private static StateEngine s;
        private static string result;

        public static void RunTests()
        {
            TestHolder();
            TestDanger();
            TestShooting();
            //TestBuilding();
            TestPlacing();
        }

        private static void TestHolder()
        {
            s = new StateEngine(@"../../output/Tests/Test/state.json");

            string move = s.TryDodge();
            Console.WriteLine("Dodge: " + move);
            move = s.TryShoot();
            Console.WriteLine("Shoot: " + move);
            move = s.TryBuild();
            Console.WriteLine("Build: " + move);
            move = s.TryPlace();
            Console.WriteLine("Place: " + move);
            move = s.TryShift();
            Console.WriteLine("Shift: " + move);
        }

        private static void TestPlacing()
        {
            s = new StateEngine(@"../../output/Tests/TryPlaceNoBlockages/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, no need (Expected: null) - " + result);
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceInFrontOfMC/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, standing in front of missilecontroller (Expected: not null) - " + result);
            if (result == null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceLeftBest/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, no blockages on two left blocks (Expected: MoveLeft) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveLeft.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceRightBest/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, no blockages on two right blocks (Expected: MoveRight) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveRight.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceLeftGood/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, no blockages on 1 left block (Expected: MoveLeft) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveLeft.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceLeftBestLeftTwoBlocked/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, left is best, bullet on 2nd left row (Expected: MoveRight) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveRight.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceRightBestSecondNotDangerous/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, shield on 2nd right row (Expected: MoveRight) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveRight.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceRightBestSecondDangerous/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, but missile and shields in the way. (Expected: MoveLeft) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveLeft.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceInMiddleShieldsLeftRight/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, shields are above. (Expected: MoveRight) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveRight.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceInMiddleShieldsLeftRightanotherRight/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, shields are above and another on the right. (Expected: MoveLeft) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveLeft.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceAtRightShieldsLeftRight/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, shields are above and another on the right. (Expected: MoveLeft) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveLeft.ToString())) Debugger.Break();

            // Weird situation. No point moving right because we'd never really reach an opening
            s = new StateEngine(@"../../output/Tests/TryPlaceAtRightShieldsLeftRightAnotherLeftWallAtRight/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, shields are above and another on the left. Can't really move right. (Expected: MoveLeft) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveLeft.ToString())) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceRightBestSecondDangerousLeftBlocked/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, but missile and shields in the way, left is blocked. (Expected: null) - " + result);
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryPlaceNowhereForAWhile/state.json");
            result = s.TryPlace();
            Console.WriteLine("Tried placing ship, right is best, but lots of shields for a long way. (Expected: MoveRight) - " + result);
            if (!result.Equals(CharonBot.Moves.MoveRight.ToString())) Debugger.Break();
        }

        public static void TestShooting()
        {
            s = new StateEngine(@"../../output/Tests/TryShootSuccessfully/state.json");
            result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, shot moving right (Expected: Shoot) - " + result); //Shoot, aliens move right, success
           if (!result.Equals("Shoot")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootFail/state.json");
            result = s.TryShoot();
            Console.WriteLine("Shoot, aliens move right, fail (Expected: null) - " + result); // Shoot, aliens move right, fail
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootFailWhenMoveDown/state.json");
            result = s.TryShoot();
            Console.WriteLine("Shoot, aliens move down, fail - " + result); // Shoot, aliens move down, fail
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootFailWhenMoveLeft/state.json");
            result = s.TryShoot();
            Console.WriteLine("Shoot, aliens move left, fail - " + result); // Shoot, aliens move left, fail
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootSuccessfullyWhenMoveDown/state.json");
            result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, shot moving right - " + result); // Shoot, aliens move down, success
            if (!result.Equals("Shoot")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootSuccessfullyMoveLeft/state.json");
            result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, shot moving right (Expected: Shoot) - " + result);
            if (!result.Equals("Shoot")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootWaitWouldBeBest/state.json");
             result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, but rather wait for another one (Expected: null) - " + result);
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootWaitWouldBeBestButDanger/state.json");
            result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, wait for another, but danger (Expected: Shoot) - " + result);
            if (!result.Equals("Shoot")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TryShootWaitWouldBeBestTwoRounds/state.json");
            result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, but rather wait for another two rounds (Expected: null) - " + result);
            if (result != null) Debugger.Break();
            

            s = new StateEngine(@"../../output/Tests/TryShootWaitWouldBeBad/state.json");
            result = s.TryShoot();
            Console.WriteLine("Try shoot successfully, but rather wait for another one, but shouldn't because that's worse  (Expected: Shoot) - " + result);
            if (!result.Equals("Shoot")) Debugger.Break();
        }

        public static void TestBuilding()
        {
            // No tests now as this will change a lot

            s = new StateEngine(@"../../output/Tests/TestBuildFullLivesStandardShields/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build, first round of game (Expected: BuildShield) - " + result);
            if (!result.Equals("BuildShield")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildShieldBlockage1/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build, stuff in way (Expected: null - " + result);
            if (result != null && result.Equals("BuildShield")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildShieldBlockage2/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build, stuff in way (Expected: null - " + result);
            if (result != null && result.Equals("BuildShield")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildShieldBlockage3/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build, stuff in way (Expected: null - " + result);
            if (result != null && result.Equals("BuildShield")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildMissileControllerBlockage/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build, stuff in way (Expected: null - " + result);
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildNotBehindShields/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build, not behind shields. Should never be MC or AF - " + result);
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildBehindShieldNoMC/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build behind shield (Expected: BuildMissileController) - " + result);
            if (!result.Equals("BuildMissileController")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestBuildBehindShieldHaveMC/state.json");
            result = s.TryBuild();
            Console.WriteLine("Try build behind shield (Expected: BuildAlienFactory) - " + result);
            if (!result.Equals("BuildAlienFactory")) Debugger.Break();
        }

        public static void TestDanger()
        {
            // N.B. Due to incorrect logic at the beginning, the names of some test cases are wrong. Test one above is really two above, and test two above is really three above.
            s = new StateEngine(@"../../output/Tests/TestDangerOneAboveBulletLeft/state.json");
            result = s.TryDodge();
            Console.WriteLine("Bullet one above left (Expected: MoveRight) - " + result); //Testing when danger is one above through a bullet to left. Expected: MoveRight
            if (!result.Equals("MoveRight")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerOneAboveMissileRight/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile one above right (Expected: MoveLeft) - " + result); //Testing when danger is one above through a missile to right. Expected: MoveLeft
            if (!result.Equals("MoveLeft")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerOneAboveMissileMiddle/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile one above middle (Expected: Shoot) - " + result); //Testing when danger is one above through a missile. Expected: Shoot
            if (!result.Equals("Shoot")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerOneAboveMissileMiddleCantShoot/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile one above middle can't shoot (Expected: null) - " + result); //Testing when danger is one above through a missile. Expected: null
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerTwoAboveBulletLeft/state.json");
            result = s.TryDodge();
            Console.WriteLine("Bullet two above left (Expected: null) - " + result); //Testing when danger is two above through a bullet to left. Expected: null
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerTwoAboveMissileRight/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile two above right (Expected: null) - " + result); //Testing when danger is two above through a missile to right. Expected: null
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerTwoAboveMissileMiddle/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile two above middle (Expected: MoveRight) - " + result); //Testing when danger is two above through a missile. Expected: MoveRight
            if (!result.Equals("MoveRight")) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerTwoAboveMissileMiddleCantShoot/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile two above middle can't shoot (Expected: null) - " + result); //Testing when danger is two above through a missile. Expected: null
            if (result != null) Debugger.Break();

            s = new StateEngine(@"../../output/Tests/TestDangerTwoAboveMissileMiddleCantMove/state.json");
            result = s.TryDodge();
            Console.WriteLine("Missile two above middle can't move (Expected: Shoot) - " + result); //Testing when danger is two above through a missile. Expected: shoot
            if (!result.Equals("Shoot")) Debugger.Break();
        }
    }
}
