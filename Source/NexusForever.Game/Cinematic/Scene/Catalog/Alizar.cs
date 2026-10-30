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
            // models (Human, Aurin, Mordesh) played on the player copy. This is the launch (2014) arrival, from 2016 on every Exile
            // leaving the Arkship for the Northern Wilds drops in by pod instead (footage of an Aurin). Subtitle timings from retail
            // footage (youtube.com/watch?v=iMSWhvau5mI, 0:40), lines read with speech recognition, about +-300 ms.
            Add(CinematicSceneId.NorthernWildsHumanIntro)
                .WithDuration(27000)
                .AtOrigin(3890.446f, -696.264f, -5313.697f)
                .SplineShot(0, 10824).SplineShot(7367, 10825).SplineShot(15033, 10826)
                .WithPlayerClone()
                .WithPlayerVisual(31265)
                .WithPlayerVisual(30554, 300)
                .WithPlayerVisual(30560, 15033)
                .WithPlayerVisual(30558, 21000)
                .WithText(578197, 1600, 2900)
                .WithText(578198, 2900, 6300)
                .WithText(578199, 6600, 9200)
                .WithText(578200, 9800, 10600)
                .WithText(578201, 10800, 13000)
                .WithText(578202, 13500, 15000)
                .WithText(578203, 18600, 19900)
                .WithText(578204, 19900, 23500)
                .WithText(578205, 23900, 25400)
                .WithText(578206, 25400, 26900);

            // Leaving Camp Icefury in a borrowed Dominion shuttle (More Important Than Revenge). No scene of its own exists in the client
            // beyond old camera splines, the generic Dominion transport departure of GC197 (the same shuttle leaving Tremor Ridge) is played
            // on the Camp Icefury landing site WorldLocation2 49749 with the departure lines. Guessed reconstruction.
            // Retail footage of the launch build (youtube.com/watch?v=dPFnwNUoWcE, 14:04) shows a 25 s scene: a static wide shot of the
            // parked shuttle while the radio lines play, lift off at 12.8 s, then the shuttle flies off into the sunset. The generic
            // departure is shorter, so the lines keep their footage timing and the countdown is pulled in before its end.
            Add(CinematicSceneId.NorthernWildsDeparture)
                .WithDuration(17967)
                .AtOrigin(4548.010f, -700.209f, -5204.670f)
                .WithCamera(70036)
                .WithActors(70037)
                .WithText(578229, 3400, 4900)
                .WithText(578230, 4900, 6600)
                .WithText(578231, 7300, 9600)
                .WithText(578232, 10400, 11400)
                .WithText(578233, 12000, 15600);

            // GC174: Contact with Thayd, the rescue ship from Thayd lands at the flares and is shot down with Sadie aboard
            // Origin guessed: WorldLocation2 49637 at the landing site outside Settler's Reach, the camera path only fits the terrain there
            // and the entry is from the same build as the scene.
            // Subtitles are the 2016 Sadie and Deadeye Brightland lines (755538-755545), timed from retail footage
            // (youtube.com/watch?v=QScgJtxNp0M, 37:02), which starts when the last of the three signal flares is lit.
            // The pilot's lines (755544/755545) weren't audible in the footage, they're placed on the landing shot (guessed).
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
                .WithText(755544, 6500, 10500)
                .WithText(755545, 10600, 12500)
                .WithText(755542, 13100, 16300)
                .WithText(755538, 17800, 20600)
                .WithText(755543, 21000, 26800)
                .WithText(755539, 27800, 34200)
                .WithText(755540, 37400, 39500)
                .WithText(755541, 84100, 88900);

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

            // GC036/GC037: Elderoot is a map prop (active prop 1616496, Cine_EverstarGrove_ElderrootAwakened_EXPORT_elderoot.m3) placed on the
            // origin of both scenes, its model holds the scene animation as active prop states. The scenes add an actor bound to the prop
            // (an invisible creature, ActivePropId) and play the state transition on it, untested whether the client animates the
            // prop this way. Camera splines 12356-12373 start on the origin.
            Add(CinematicSceneId.EverstarGroveElderootAwakens)
                .WithDuration(37200)
                .AtOrigin(-338.654f, -909.635f, -3263.055f)
                .SplineShot(0, 12358).SplineShot(6100, 12367).SplineShot(12300, 12368).SplineShot(15067, 12369).SplineShot(18067, 12370)
                .SplineShot(19633, 12371).SplineShot(23400, 12372).SplineShot(30367, 12373)
                // AP_State1_State2 (27600-64800 on the model timeline)
                .WithActor(new SceneActor(PropActorCreature) { ActivePropId = 1616496, Visuals = [new SceneVisual(445, 0, 0)] })
                .WithPlayerVisual(27299);

            // GC037: after Elderoot shares the Eldan's secrets the Chua blimp burns it, AP_State0_State1 (1600-21600) on the prop.
            // Voice clip timings and subtitle timings guessed.
            Add(CinematicSceneId.EverstarGroveElderootBurns)
                .WithDuration(20000)
                .AtOrigin(-338.654f, -909.635f, -3263.055f)
                .SplineShot(0, 12356).SplineShot(15667, 12357)
                .WithActor(new SceneActor(PropActorCreature) { ActivePropId = 1616496, Visuals = [new SceneVisual(33855, 0, 0)] })
                .WithActor(new SceneActor(41217) { Visuals = [new SceneVisual(SequenceVisualMisc01, 0, 0), new SceneVisual(11097, 15667, 0)] })
                .WithPlayerVisual(27194)
                .WithPlayerVisual(30586, 500)
                .WithPlayerVisual(40945, 6000)
                .WithPlayerVisual(40946, 9000)
                .WithPlayerVisual(36776, 10000)
                .WithPlayerVisual(40947, 15667)
                .WithTextsSpread(500, 5900, (578492, 24), (578493, 48))
                .WithText(578494, 6000, 8500)
                .WithText(578495, 9000, 12000)
                .WithText(578496, 15667, 19500);

            // GC175: Myala sends the player through the Aurin portal at Fort Firestorm to Celestion. Origin WorldLocation2 49648 is the portal
            // (Portal to Celestion 32269 and Queen Myala stand on it), from the same build as the scene.
            Add(CinematicSceneId.EverstarGroveThroughThePortal)
                .WithDuration(66533)
                .AtOrigin(656.083f, -957.618f, -3295.840f)
                .WithCamera(70665)
                .CutTo(6233, 8).CutTo(8500, 9).CutTo(11867, 10).CutTo(14400, 11).CutTo(16133, 12).CutTo(18600, 13).CutTo(21267, 14)
                .CutTo(24367, 15).CutTo(25400, 16).CutTo(27000, 17).CutTo(29533, 18).CutTo(30033, 19).CutTo(31867, 20).CutTo(38167, 21)
                .CutTo(40033, 22).CutTo(43200, 23).CutTo(45500, 24).CutTo(50100, 25).CutTo(52267, 26).CutTo(54433, 28).CutTo(60267, 29)
                .WithActors(70666, 70667, 70668, 70669, 70670, 70671, 70672, 70673, 70674, 72238, 72458, 73021)
                .WithPlayerVisual(49221);

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
                // the cannon is a map prop on the origin (active prop 4496119)
                .WithActor(PropActor(4496119, 34616))
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
            // Followed by "Tremor Ridge saved" (sound Play_Cinematic_TremorRidgeSaved_Stereo): the town camera splines 5891 and 5892 share
            // the escape origin. Guessed that they follow the escape, no model sequence covers them.
            Add(CinematicSceneId.AlgorocSkullsEyeEscape)
                .WithDuration(24967)
                .AtOrigin(3825.070f, -1018.220f, -4630.700f)
                .WithSequenceVisual(SequenceVisualMisc03)
                .SplineShot(0, 6237).SplineShot(4667, 5891).SplineShot(13600, 5892)
                .WithActors(36964, 36965, 36970)
                .WithPlayerVisual(18350)
                .WithPlayerVisual(11545, 4667);

            // GC141: the borrowed Dominion transport from Camp Icefury lands at the Tremor Ridge pad. Generic ship arrival (10 s, one camera)
            // placed on WorldLocation2 49496 and rotated by its facing: pi + 76 degrees is the facing of the parked transport world spawn
            // (69781 "Dominion Transport - Algoroc - Cinematic Spawn") on the same spot. Engine sound guessed.
            Add(CinematicSceneId.AlgorocArrival)
                .WithDuration(10000)
                .AtOrigin(3814.840f, -1001.468f, -4486.068f)
                .WithAngle(-1.81479f)
                .WithCamera(69782)
                .WithActors(69783)
                .WithPlayerVisual(7647);

            // GC197: the Dominion transport leaves the Tremor Ridge pad, generic departure on WorldLocation2 49687 (same spot, the transport's
            // facing). No trigger: how the player boards for Thayd isn't known.
            Add(CinematicSceneId.AlgorocDeparture)
                .WithDuration(17967)
                .AtOrigin(3814.840f, -998.086f, -4486.066f)
                .WithAngle(1.29275f)
                .WithCamera(70036)
                .WithActors(70037);

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
