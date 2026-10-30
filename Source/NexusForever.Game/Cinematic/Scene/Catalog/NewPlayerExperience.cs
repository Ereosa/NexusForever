using System.Numerics;
using NexusForever.Game.Static.Cinematic;

namespace NexusForever.Game.Cinematic.Scene
{
    public static partial class CinematicSceneCatalog
    {
        // NPE simulation full screen effects, from the captured Exile combat transition
        private const uint NpeSimulationScreenEffect      = 50760;
        private const uint NpeSimulationScreenEffectFade  = 50761;

        /// <summary>
        /// New Player Experience (world 3460) transitions between the simulations.
        /// </summary>
        /// <remarks>
        /// Origins are the WorldLocation2 pairs (Exile, Dominion) of each simulation zone, the captured intro (51613/51619) and Exile combat
        /// transition (51615/51620) sit on the first two pairs. The Dominion transitions use the same origins as the Exile ones.
        /// Only the Exile combat transition was captured. Exile subtitle timings come from retail footage of the 2016 tutorial
        /// (youtube.com/watch?v=1OBtOZP4cd4, lines read with speech recognition, about +-300 ms). The Dominion scenes share the
        /// Exile structure, their subtitles use the Exile timings (guessed, the only Dominion footage is a narrated review).
        /// </remarks>
        private static void AddNewPlayerExperience()
        {
            // GC290: entering Warfare Woods, the camera flies back from the housing projector to the Dominion start (52853)
            // Placed actors stand on the Dominion side WorldLocation2 entries matching the captured Exile scene (beasts 52908-52916,
            // mines 52899-52901, turrets 52902-52903, projectors 52904/52809, fights 52920/52810 and 52921/52922).
            // Guessed: which creature stands on which fight location, angles of beasts (copied from the Exile capture), turrets (face the start).
            Add(CinematicSceneId.NpeCombatDominion)
                .WithDuration(10000)
                .AtOrigin(-50.53350067138672f, -861.4010009765625f, 307.8080139160156f)
                .WithCamera(73550)
                .WithActors(73551, 74972)
                .WithActor(Placed(74879, 142.515f, -849.479f, 544.768f, 0.00001f))
                .WithActor(Placed(74880, 147.070f, -849.463f, 554.445f, 0.00001f))
                .WithActor(Placed(74890, 152.677f, -855.104f, 512.440f, -2.25f))
                .WithActor(Placed(74891, 154.910f, -855.089f, 514.243f, 0.89f))
                .WithActor(Placed(74890, 133.236f, -854.964f, 479.153f, 0.85f))
                .WithActor(Placed(74891, 130.463f, -854.964f, 476.735f, -2.29f))
                .WithActor(Placed(74874, 55.174f, -874.981f, 269.695f, 0.2617994248867035f))
                .WithActor(Placed(74874, 36.936f, -874.746f, 269.574f, 5.7072272300720215f))
                .WithActor(Placed(74874, 68.764f, -872.225f, 268.012f, 1.2566372156143188f))
                .WithActor(Placed(74874, 73.900f, -870.675f, 280.169f, 3.263766050338745f))
                .WithActor(Placed(74874, 51.165f, -875.193f, 298.340f, 1.0646508932113647f))
                .WithActor(Placed(74874, 26.093f, -871.982f, 301.157f, 1.29154372215271f))
                .WithActor(Placed(74874, 23.453f, -872.695f, 329.130f, 5.724680423736572f))
                // WorldLocation2 52915 has a broken height (303.7), the height of its neighbours is used
                .WithActor(Placed(74874, 49.322f, -873.500f, 317.703f, 3.5953786373138428f))
                .WithActor(Placed(74874, 78.666f, -872.905f, 301.429f, 4.031711101531982f))
                .WithActor(Placed(74875, 72.987f, -872.089f, 357.634f, 0f))
                .WithActor(Placed(74876, 53.515f, -872.089f, 374.479f, 0f))
                .WithActor(Placed(74877, 43.117f, -872.089f, 395.189f, 0f))
                .WithActor(Placed(74878, 80.834f, -864.214f, 426.115f, 0.27f))
                .WithActor(Placed(74878, 114.160f, -864.214f, 428.627f, 0.42f))
                .WithPlayerVisual(50909)
                .WithPlayerVisual(NpeSimulationScreenEffect, duration: 3000)
                .WithPlayerVisual(NpeSimulationScreenEffectFade, delay: 3000)
                .WithText(749288, 1500, 6400)
                .WithText(749289, 6500, 9900);

            // GC296/GC291: arriving in the mission simulation (Solitude of Secrets) from the mission projector
            Add(CinematicSceneId.NpeQuestingExile)
                .WithDuration(10000)
                .AtOrigin(476.463f, -393.812f, 254.228f)
                .WithCamera(73587)
                .WithActors(74798, 74799)
                .WithPlayerVisual(50916)
                .WithPlayerVisual(NpeSimulationScreenEffect, duration: 3000)
                .WithPlayerVisual(NpeSimulationScreenEffectFade, delay: 3000)
                .WithText(749305, 750, 5300)
                .WithText(749306, 5400, 8600);

            Add(CinematicSceneId.NpeQuestingDominion)
                .WithDuration(10000)
                .AtOrigin(476.463f, -393.812f, 254.228f)
                .WithCamera(73586)
                .WithActors(74794, 74795)
                .WithPlayerVisual(50910)
                .WithPlayerVisual(NpeSimulationScreenEffect, duration: 3000)
                .WithPlayerVisual(NpeSimulationScreenEffectFade, delay: 3000)
                // only one subtitle line exists for the Dominion transition
                .WithText(749293, 1000, 6000);

            // GC297/GC292: leaving Warfare Woods through the housing projector to the Protostar skyplot, Dorian/Artemis then the Protostar advert
            // Camera cuts from the shot sequences (Cinematic_Misc_01..07) of the camera model, the sequence starts at 3000 ms.
            // The footage confirms the cuts. Its first 6.5 s are music only, the Dorian/Artemis lead-in lines (749307/749308 and
            // 749297/749298) stay on that stretch, the Protostar advert starts with the goblin at 6.6 s.
            Add(CinematicSceneId.NpeHousingExile)
                .WithDuration(24167)
                .AtOrigin(1177.160f, -493.389f, 223.459f)
                .WithCamera(73560)
                .CutTo(7700, 8).CutTo(10333, 9).CutTo(12767, 10).CutTo(15267, 11).CutTo(18100, 12).CutTo(19600, 13)
                .WithActors(73561, 73562, 73730)
                .WithPlayerVisual(50914)
                .WithText(749307, 300, 2600)
                .WithText(749308, 2700, 6500)
                .WithText(749309, 6600, 11800)
                .WithText(749310, 12900, 18200)
                .WithText(749311, 18250, 21000)
                .WithText(749312, 21000, 23400);

            Add(CinematicSceneId.NpeHousingDominion)
                .WithDuration(24167)
                .AtOrigin(1177.160f, -493.389f, 223.459f)
                .WithCamera(73588)
                .CutTo(7700, 8).CutTo(10333, 9).CutTo(12767, 10).CutTo(15267, 11).CutTo(18100, 12).CutTo(19600, 13)
                .WithActors(73589, 73590, 73729)
                .WithPlayerVisual(50908)
                .WithText(749297, 300, 2600)
                .WithText(749298, 2700, 6500)
                .WithText(749299, 6600, 11800)
                .WithText(749300, 12900, 18200)
                .WithText(749301, 18250, 21000)
                .WithText(749302, 21000, 23400);

            // GC299: Annihilator shown in the mission simulation. Retail footage (both factions) plays it when the player activates the
            // Cryo Projector at the end of Deconstruct the Construct, right before the departure bay scene: the Annihilator rises from
            // the ice behind the player and swings its arm, cut to black.
            // Origin guessed: the single WorldLocation2 51610 of the NPE cinematic block in Solitude of Secrets,
            // it puts the player model of the scene next to the Primal Manifestation rings.
            Add(CinematicSceneId.NpeAnnihilator)
                .WithDuration(6100)
                .AtOrigin(611.551f, -398.782f, 294.673f)
                .WithCamera(74747)
                .WithActors(74748, 74749, 74750)
                .WithPlayerVisual(50836);

            // GC298/GC293: leaving the simulation through the cryo projector, waking up on the Arkship, Drusera's vision, Departures Terminal
            // The Dominion origin is the first node of camera spline 22859 (the exported first camera of GC293), which is WorldLocation2 51657,
            // the Dominion cryo arrival. The Exile origin is guessed as its counterpart 51658. The player copy is attached to the player
            // platform like the captured intro, Cinematic 104/103 are the CinematicRace entries of the NPE outro.
            // Footage: Dorian's hologram and the cryo pod (0-14 s), Drusera's vision (14-35 s), body scan (35-44 s), black, the cryo pod
            // from above (47 s), the player's face (53 s) and the Departures Terminal (57.6 s). Dorian opens with "Stars! That was a
            // close one!" (753322, an id added outside the scene block).
            Add(CinematicSceneId.NpeDepartureBayExile)
                .WithCinematicId(104)
                .WithDuration(63000)
                .AtOrigin(-4338.952f, -445.600f, -4104.568f)
                .WithCamera(73527)
                .CutTo(47000, 8).CutTo(53067, 9).CutTo(57734, 10)
                .WithActors(73528, 73529, 73530, 73531, 73532, 73533, 73572)
                .WithPlayerOnPlatform(73533, NpePlatformAttachment)
                .WithPlayerVisual(50917)
                .WithText(753322, 1300, 3800)
                .WithText(750201, 4600, 9600)
                .WithText(750202, 9700, 13300)
                .WithText(750203, 18300, 20000)
                .WithText(750204, 20300, 22100)
                .WithText(750205, 22100, 26000)
                .WithText(750206, 27000, 33400)
                .WithText(750207, 35000, 38200)
                .WithText(750208, 38600, 47900)
                .WithText(750209, 48100, 53300)
                .WithText(750210, 54000, 60400);

            Add(CinematicSceneId.NpeDepartureBayDominion)
                .WithCinematicId(103)
                .WithDuration(63033)
                .AtOrigin(-3689.684f, -436.790f, -4356.004f)
                .WithCamera(73515)
                .CutTo(47000, 8).CutTo(53067, 9).CutTo(57733, 10)
                .WithActors(73516, 73517, 73518, 73519, 73571, 73593, 73594)
                .WithPlayerOnPlatform(73519, NpePlatformAttachment)
                .WithPlayerVisual(50911)
                .WithText(750189, 4600, 9600)
                .WithText(750190, 9700, 13300)
                .WithText(750191, 18300, 20000)
                .WithText(750192, 20300, 23200)
                .WithText(750193, 24200, 26000)
                .WithText(750194, 27000, 33400)
                .WithText(750195, 35000, 38200)
                .WithText(750196, 38600, 47900)
                .WithText(750197, 48100, 53300)
                .WithText(750198, 54000, 60400);
        }

        // GC071: launch from the Exile Arkship to the Northern Wilds, played at the Departures Terminal before the drop pod lands
        // (NorthernWildsOnCreate). The ALLSHIPS models hold five 20.7 s launch scenes of three static shots each, the Northern Wilds
        // one is the third (Cinematic_Misc_12, cameras Cinematic_09-11), matching retail footage of the 2016 tutorial
        // (youtube.com/watch?v=uhdjrrVe354, 13:44): hull close-up, the ship far away, the planet with the pod, cuts at 5.9 and 12.3 s.
        // Origin guessed: WorldLocation2 51603, unreferenced and 112 m below the Exile Docking Bay, its twin 51604 sits below the
        // Dominion one. Dispatcher voice (ExileDispatcherNW_01/02) and subtitle timings from the footage, voice offsets guessed.
        private const uint NpeLaunchSequenceNorthernWilds = 18574;

        private static void AddNewPlayerExperienceLaunches()
        {
            Add(CinematicSceneId.NpeLaunchNorthernWilds)
                .WithDuration(20667)
                .AtOrigin(-4128.19f, -557.94f, -4046.78f)
                .WithSequenceVisual(NpeLaunchSequenceNorthernWilds)
                .WithCamera(51514, 15)
                .CutTo(5900, 16).CutTo(12333, 17)
                .WithActors(51519, 51520, 51521, 51526, 74813)
                .WithPlayerVisual(27971)
                .WithPlayerVisual(30537, 4400)
                .WithPlayerVisual(30540, 11900)
                .WithText(585004, 4500, 9000)
                .WithText(585005, 9000, 12000)
                .WithText(585006, 12000, 14500)
                .WithText(754541, 14500, 18700);
        }

        private static SceneActor Placed(uint creatureId, float x, float y, float z, float angle)
        {
            return new SceneActor(creatureId)
            {
                Position = new Vector3(x, y, z),
                Angle    = angle,
                Visuals  = []
            };
        }
    }
}
