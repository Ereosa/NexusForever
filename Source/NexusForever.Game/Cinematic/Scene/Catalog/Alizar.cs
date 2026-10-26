using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Cinematic.Scene
{
    public static partial class CinematicSceneCatalog
    {
        // model sequence visuals (Cinematic_Misc_01, _03, _20)
        private const uint SequenceVisualMisc01 = 11096;
        private const uint SequenceVisualMisc03 = 11098;
        private const uint SequenceVisualMisc20 = 19480;

        /// <summary>
        /// Exile zones of Alizar: Northern Wilds (426), Everstar Grove (990), Celestion and Algoroc (51).
        /// </summary>
        private static void AddAlizar()
        {
            // GC002: the transport carrying the player is shot down and crashes in the Northern Wilds, camera splines 10824-10826
            // start on the origin at the crash site. No actor creatures exist for the scene, Cinematic 2 holds the CinematicRace player
            // models (Human, Aurin, Mordesh) played on the player copy. Voice clip order and subtitle timings guessed.
            Add(CinematicSceneId.NorthernWildsHumanIntro)
                .WithDuration(27000)
                .AtOrigin(3890.446f, -696.264f, -5313.697f)
                .SplineShot(0, 10824).SplineShot(7367, 10825).SplineShot(15033, 10826)
                .WithPlayerClone()
                .WithPlayerVisual(31265)
                .WithPlayerVisual(30554, 300)
                .WithPlayerVisual(30560, 15033)
                .WithPlayerVisual(30558, 21000)
                .WithTextsSpread(300, 7300, (578197, 25), (578198, 67), (578199, 50), (578200, 11), (578201, 33), (578202, 27))
                .WithTextsSpread(15100, 20900, (578203, 24), (578204, 54))
                .WithTextsSpread(21000, 26500, (578205, 36), (578206, 33));

            // GC174: Contact with Thayd, the rescue ship from Thayd lands at the flares and is shot down with Sadie aboard
            // Origin guessed: WorldLocation2 49637 at the landing site outside Settler's Reach, the camera path only fits the terrain there
            // and the entry is from the same build as the scene. Subtitle timings guessed from line lengths.
            Add(CinematicSceneId.NorthernWildsSadieDeath)
                .WithDuration(89333)
                .AtOrigin(4353.266f, -747.386f, -5594.562f)
                .WithCamera(70271)
                .CutTo(3633, 8).CutTo(6033, 9).CutTo(10867, 10).CutTo(17200, 11).CutTo(19733, 12).CutTo(21700, 13).CutTo(24433, 14)
                .CutTo(26100, 15).CutTo(28733, 16).CutTo(30733, 17).CutTo(35167, 18).CutTo(37100, 19).CutTo(40133, 20).CutTo(41067, 21)
                .CutTo(41800, 22).CutTo(42500, 23).CutTo(43033, 24).CutTo(44067, 25).CutTo(46867, 26).CutTo(47767, 28).CutTo(50167, 29)
                .CutTo(51200, 30).CutTo(54367, 31).CutTo(56533, 32).CutTo(57933, 33).CutTo(59300, 34).CutTo(61533, 35).CutTo(69667, 36)
                .CutTo(72833, 37).CutTo(81067, 38).CutTo(85167, 39)
                .WithActors(70272, 70273, 70274, 70275, 70276, 70277, 70278, 70279, 70280, 70283, 70284, 71761)
                .WithPlayerVisual(50826)
                .WithText(578497, 1000, 4000)
                .WithTextsSpread(6000, 46000, (578208, 15), (578209, 41), (578210, 51), (578211, 30), (578212, 18), (578213, 50), (578214, 35), (578215, 13), (578216, 23))
                .WithTextsSpread(47000, 62000, (578217, 26), (578218, 54), (578219, 28), (578220, 46), (578221, 18));

            // GC034: Mordesh arrival at the Vitalus Production Facility, Whisperwind Hollow
            // Camera splines 13654-13656 start on the origin (WorldLocation2 28117/40219), the actors play Cinematic_Misc_20.
            // The Male/Female actors are the Mordesh player models of the scene (CinematicRace 34). Subtitle timings guessed.
            Add(CinematicSceneId.EverstarGroveMordeshArrival)
                .WithDuration(20000)
                .AtOrigin(-317.064f, -940.324f, -2346.156f)
                .WithSequenceVisual(SequenceVisualMisc20)
                .SplineShot(0, 13654).SplineShot(7933, 13655).SplineShot(11267, 13656)
                .WithActors(65447)
                .WithActor(new SceneActor(50645) { Sex = Sex.Male })
                .WithActor(new SceneActor(50646) { Sex = Sex.Female })
                .WithPlayerVisual(29498)
                .WithPlayerVisual(30511, 1000)
                .WithPlayerVisual(30512, 11267)
                .WithTextsSpread(1000, 11000, (578484, 66), (578485, 54), (578486, 56))
                .WithText(578487, 11267, 16000);

            // GC068: the Greenbough Guardians are called at the Shrine of the Guardian and crush the Dominion at Point Firestorm
            // Origin guessed: WorldLocation2 43962 in Point Firestorm, best camera fit near the quest location from the same build.
            Add(CinematicSceneId.CelestionForestFightsBack)
                .WithDuration(39000)
                .AtOrigin(1542.752f, -874.872f, -3113.610f)
                .WithSequenceVisual(SequenceVisualMisc20)
                .WithCamera(42002)
                .CutTo(11500, 8).CutTo(18700, 9).CutTo(22533, 10).CutTo(24933, 11)
                .WithActors(42000, 42001, 42003, 42004, 42005, 42006, 42007, 42008, 42009)
                .WithPlayerVisual(29994);

            // GC070: the Junction Zeta Eldan weapon shoots down the ICI spy ship, camera spline 14817 starts on the origin
            Add(CinematicSceneId.CelestionCannonShootsDownShip)
                .WithDuration(21667)
                .AtOrigin(3207.678f, -984.182f, -1893.472f)
                // visual effect made for this scene, it plays Cinematic_Misc_01 for its 21666 ms length
                .WithSequenceVisual(34616)
                .SplineShot(0, 14817)
                .WithActors(48436, 54452)
                .WithPlayerVisual(27195)
                .WithPlayerVisual(30527, 1000)
                .WithPlayerVisual(39577, 12000);

            // Q3766: the Skull's Eye arrives over the Crystal Shards, camera spline 4335 starts on the origin
            Add(CinematicSceneId.AlgorocSkullsEyeArrival)
                .WithDuration(14333)
                .AtOrigin(3302.830f, -1027.970f, -4210.060f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 4335)
                .WithActors(13122, 22325)
                .WithPlayerVisual(11631);

            // Q3770: the Skull's Eye takes off from the Crystal Shards, camera spline 6238
            Add(CinematicSceneId.AlgorocSkullsEyeTakeoff)
                .WithDuration(7000)
                .AtOrigin(3302.830f, -1027.970f, -4210.060f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 6238)
                .WithActors(22329)
                .WithPlayerVisual(11632);

            // GC062: the escape pod leaves the exploding Skull's Eye over Tremor Ridge, camera spline 6237
            // Spline 11125 is a copy of the same shot on another origin (3700.91, -1019.74, -4657.43), the first one was picked.
            Add(CinematicSceneId.AlgorocSkullsEyeEscape)
                .WithDuration(4667)
                .AtOrigin(3825.070f, -1018.220f, -4630.700f)
                .WithSequenceVisual(SequenceVisualMisc03)
                .SplineShot(0, 6237)
                .WithActors(36964, 36965, 36970)
                .WithPlayerVisual(18350);

            // GC017: augmentors convert XAS scientists at Annihilator Silo 14, camera spline 10885
            Add(CinematicSceneId.AlgorocAugmentation)
                .WithDuration(14000)
                .AtOrigin(4523.337f, -1109.170f, -3912.425f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 10885)
                .WithActors(36184, 36185, 39956)
                .WithPlayerVisual(28888)
                .WithPlayerVisual(42196);

            // GC060: the Caretaker raises the Eldan beam cannon at Annihilator Control 14A, camera spline 9782
            Add(CinematicSceneId.AlgorocCannonSpawn)
                .WithDuration(8600)
                .AtOrigin(4619.260f, -1163.650f, -3734.350f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(33, 9782)
                .WithActors(33117, 33144)
                .WithPlayerVisual(30514);

            // GC018: the beam cannon destroys the Annihilator (World Destroyer), camera splines 9824-9827
            Add(CinematicSceneId.AlgorocWorldDestroyerDeath)
                .WithDuration(24500)
                .AtOrigin(4619.260f, -1163.650f, -3734.350f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(33, 9824).SplineShot(10066, 9825).SplineShot(13200, 9826).SplineShot(17266, 9827)
                .WithActors(33215, 33232)
                .WithPlayerVisual(18797)
                .WithPlayerVisual(30487, 17266);

            // GC267: Throg's attack on Kain's Camp, Judge Kain loses his arm
            // Origin guessed: WorldLocation2 51344 next to the Kain's Camp quest location, from the same build as the scene.
            Add(CinematicSceneId.AlgorocJudgeKainLosesAnArm)
                .WithDuration(45000)
                .AtOrigin(3861.090f, -996.010f, -3865.720f)
                .WithCamera(73047)
                .CutTo(2300, 8).CutTo(6167, 9).CutTo(12700, 10).CutTo(16400, 11).CutTo(18733, 12).CutTo(21133, 13).CutTo(25567, 14)
                .CutTo(27367, 15).CutTo(28267, 16).CutTo(29267, 17).CutTo(29900, 18).CutTo(33067, 19)
                .WithActors(73048, 73049, 73050, 73051, 73052, 75145)
                .WithPlayerVisual(49455);
        }
    }
}
