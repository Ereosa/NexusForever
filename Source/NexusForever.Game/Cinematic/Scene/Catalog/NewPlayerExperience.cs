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
        /// Subtitle timings are guessed from the line lengths and camera cuts, only the Exile combat transition was captured.
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
                .WithText(749305, 1000, 5400)
                .WithText(749306, 5500, 9300);

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
            Add(CinematicSceneId.NpeHousingExile)
                .WithDuration(24167)
                .AtOrigin(1177.160f, -493.389f, 223.459f)
                .WithCamera(73560)
                .CutTo(7700, 8).CutTo(10333, 9).CutTo(12767, 10).CutTo(15267, 11).CutTo(18100, 12).CutTo(19600, 13)
                .WithActors(73561, 73562, 73730)
                .WithPlayerVisual(50914)
                .WithText(749307, 300, 2600)
                .WithText(749308, 2700, 7600)
                .WithText(749309, 7700, 12700)
                .WithText(749310, 12767, 18000)
                .WithText(749311, 18100, 21500)
                .WithText(749312, 21600, 23900);

            Add(CinematicSceneId.NpeHousingDominion)
                .WithDuration(24167)
                .AtOrigin(1177.160f, -493.389f, 223.459f)
                .WithCamera(73588)
                .CutTo(7700, 8).CutTo(10333, 9).CutTo(12767, 10).CutTo(15267, 11).CutTo(18100, 12).CutTo(19600, 13)
                .WithActors(73589, 73590, 73729)
                .WithPlayerVisual(50908)
                .WithText(749297, 300, 2600)
                .WithText(749298, 2700, 7600)
                .WithText(749299, 7700, 12700)
                .WithText(749300, 12767, 18000)
                .WithText(749301, 18100, 21500)
                .WithText(749302, 21600, 23900);

            // GC299: Annihilator shown in the mission simulation, played once Servicing Circuitry is done
            // Origin guessed: the single WorldLocation2 51610 of the NPE cinematic block in Solitude of Secrets,
            // it puts the player model of the scene next to the Primal Manifestation rings. Trigger guessed.
            Add(CinematicSceneId.NpeAnnihilator)
                .WithDuration(6100)
                .AtOrigin(611.551f, -398.782f, 294.673f)
                .WithCamera(74747)
                .WithActors(74748, 74749, 74750)
                .WithPlayerVisual(50836);

            // GC298/GC293: leaving the simulation through the cryo projector, waking up on the Arkship, Drusera's vision, Departures Terminal
            // The Dominion origin is the first node of camera spline 22859 (the exported first camera of GC293), which is WorldLocation2 51657,
            // the Dominion cryo arrival. The Exile origin is guessed as its counterpart 51658. The player copy is attached to the player
            // platform like the captured intro, Cinematic 104/103 are the CinematicRace entries of the NPE outro. Subtitle timings guessed.
            Add(CinematicSceneId.NpeDepartureBayExile)
                .WithCinematicId(104)
                .WithDuration(63000)
                .AtOrigin(-4338.952f, -445.600f, -4104.568f)
                .WithCamera(73527)
                .CutTo(47000, 8).CutTo(53067, 9).CutTo(57734, 10)
                .WithActors(73528, 73529, 73530, 73531, 73532, 73533, 73572)
                .WithPlayerOnPlatform(73533, NpePlatformAttachment)
                .WithPlayerVisual(50917)
                .WithText(750201, 1000, 5000)
                .WithText(750202, 5100, 9000)
                .WithText(750203, 18000, 20000)
                .WithText(750204, 21000, 24000)
                .WithText(750205, 24100, 27500)
                .WithText(750206, 28500, 35000)
                .WithText(750207, 47200, 49000)
                .WithText(750208, 49100, 53000)
                .WithText(750209, 53100, 57600)
                .WithText(750210, 57800, 62000);

            Add(CinematicSceneId.NpeDepartureBayDominion)
                .WithCinematicId(103)
                .WithDuration(63033)
                .AtOrigin(-3689.684f, -436.790f, -4356.004f)
                .WithCamera(73515)
                .CutTo(47000, 8).CutTo(53067, 9).CutTo(57733, 10)
                .WithActors(73516, 73517, 73518, 73519, 73571, 73593, 73594)
                .WithPlayerOnPlatform(73519, NpePlatformAttachment)
                .WithPlayerVisual(50911)
                .WithText(750189, 1000, 5000)
                .WithText(750190, 5100, 9000)
                .WithText(750191, 18000, 20000)
                .WithText(750192, 21000, 24000)
                .WithText(750193, 24100, 27500)
                .WithText(750194, 28500, 35000)
                .WithText(750195, 47200, 49000)
                .WithText(750196, 49100, 53000)
                .WithText(750197, 53100, 57600)
                .WithText(750198, 57800, 62000);
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
