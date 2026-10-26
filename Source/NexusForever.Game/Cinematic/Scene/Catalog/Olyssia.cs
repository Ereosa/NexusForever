using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Cinematic.Scene
{
    public static partial class CinematicSceneCatalog
    {
        /// <summary>
        /// Dominion zones of Olyssia: Levian Bay (1387), Crimson Isle (870), Deradune and Ellevar (22).
        /// </summary>
        private static void AddOlyssia()
        {
            // GC029: Mechari arrival, the second scene of the models of the captured Cassian arrival (GC028),
            // Cinematic_Misc_20 and cameras Cinematic_05 to _08 (model cameras 11-14). Both scenes are authored on the captured GC028 origin,
            // from there the Mechari camera ends exactly on WorldLocation2 40189 at Jagged Cove, the Mechari landing site.
            // Sound as in the GC028 capture, subtitle timings copied from it.
            Add(CinematicSceneId.LevianBayMechariArrival)
                .WithDuration(36000)
                .AtOrigin(-3784.270f, -988.663f, -6188.072f)
                .WithSequenceVisual(SequenceVisualMisc20)
                .WithCamera(50444, 11)
                .CutTo(6333, 12).CutTo(23600, 13).CutTo(28700, 14)
                .WithActors(50441, 50443)
                .WithPlayerVisual(30667)
                .WithPlayerVisual(29743)
                .WithPlayerVisual(27775)
                .WithPlayerVisual(30490, 4367)
                .WithText(578469, 4500, 8000)
                .WithText(578470, 8100, 10200)
                .WithText(578471, 10300, 13600)
                .WithText(578472, 13700, 17000);

            // GC030: Star-Comm Station powers up after the station is activated
            // Origin guessed: WorldLocation2 49825, a copy of the Star-Comm Station location from the same build as the scene.
            Add(CinematicSceneId.LevianBayStarCommStation)
                .WithDuration(28333)
                .AtOrigin(-2682.487f, -1027.083f, -5874.458f)
                .WithCamera(70360)
                .CutTo(4467, 8).CutTo(6800, 9).CutTo(8800, 10).CutTo(10967, 11).CutTo(12967, 12).CutTo(15300, 13).CutTo(17633, 14).CutTo(22567, 15)
                .WithActors(70370)
                .WithPlayerVisual(46946);

            // GC031: Artemis unlocks the Elder Cube in the Elder Vault for her audience, origin from camera splines 11377-11385
            // Voice clip and subtitle timings guessed, one Artemis clip per beat of the scene.
            Add(CinematicSceneId.LevianBayElderCube)
                .WithDuration(51467)
                .AtOrigin(-2712.346f, -1326.340f, -5982.118f)
                .WithSequenceVisual(SequenceVisualMisc20)
                .WithCamera(40877)
                .CutTo(6000, 8).CutTo(10867, 9).CutTo(20167, 10).CutTo(22834, 11).CutTo(28167, 12).CutTo(33134, 13).CutTo(35434, 14).CutTo(44367, 15)
                .WithActors(37845, 37846, 37847, 37848)
                .WithPlayerVisual(24613)
                .WithPlayerVisual(30495, 1000)
                .WithPlayerVisual(30500, 10867)
                .WithPlayerVisual(30505, 22834)
                .WithPlayerVisual(30506, 35434)
                .WithPlayerVisual(30507, 44367)
                .WithTextsSpread(1000, 9500, (578473, 37), (578474, 50), (578475, 15))
                .WithText(578476, 10867, 16000)
                .WithText(578477, 17000, 20000)
                .WithText(578478, 22834, 27000)
                .WithText(578479, 35434, 42000)
                .WithText(578480, 44367, 48000);

            // GC032: Artemis flies the stolen Exile transport out of Greyshore, origin from camera spline 10827
            Add(CinematicSceneId.LevianBayEscape)
                .WithDuration(15000)
                .AtOrigin(-2517.065f, -958.967f, -4931.453f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .WithCamera(60319)
                .WithPlayerVisual(25127)
                .WithPlayerVisual(30559, 1000)
                .WithTextsSpread(1000, 14000, (578481, 90), (578482, 49), (578483, 25));

            // GC007: the Megatech shield generator goes down, camera splines 9879 and 9880 start on the origin
            Add(CinematicSceneId.CrimsonIsleShieldGenerator)
                .WithDuration(14533)
                .AtOrigin(-7753.993f, -945.562f, -391.492f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 9879).SplineShot(4667, 9880)
                .WithActors(33458)
                .WithPlayerVisual(22513);

            // GC008: Dominion attack ships strike Megatech Station, camera spline 9894
            Add(CinematicSceneId.CrimsonIsleDominionInvasion)
                .WithDuration(15000)
                .AtOrigin(-7856.595f, -941.216f, -1357.637f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(33, 9894)
                .WithActors(33566)
                .WithPlayerVisual(22517)
                .WithPlayerVisual(30478, 500)
                .WithPlayerVisual(30479, 4000)
                .WithPlayerVisual(30480, 9000)
                .WithText(578425, 500, 3000)
                .WithText(578426, 4000, 7500)
                .WithText(578427, 7600, 8900)
                .WithText(578428, 9000, 14000);

            // GC009: the Eldan Annihilator fires on the fleeing Exile transport, camera splines 10033-10036 on Megatech Station
            // Mondo's lines are guessed to belong to this scene.
            Add(CinematicSceneId.CrimsonIsleEldanAnnihilator)
                .WithDuration(25166)
                .AtOrigin(-7856.595f, -941.216f, -1357.637f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 10033).SplineShot(7033, 10034).SplineShot(11500, 10035).SplineShot(16633, 10036)
                .WithActors(58358, 50986, 58357, 33913, 33901)
                .WithPlayerVisual(23140)
                .WithPlayerVisual(30481, 16633)
                .WithTextsSpread(16633, 24500, (578436, 44), (578437, 44), (578438, 40));

            // GC010: Draken arrival, Kezrek briefs the arriving Draken, camera splines 9886 and 9887.
            // Cinematic 10 has the Draken CinematicRace player models, the player copy stands on the origin.
            Add(CinematicSceneId.CrimsonIsleDrakenArrival)
                .WithDuration(19334)
                .AtOrigin(-7658.834f, -995.092f, 133.984f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 9886).SplineShot(9033, 9887)
                .WithActors(56999, 57000)
                .WithPlayerClone()
                .WithPlayerVisual(30483, 1000)
                .WithTextsSpread(1000, 17000, (578439, 30), (578440, 81), (578441, 37), (578442, 32));

            // GC011: Chua arrival on Mondo's Beachhead, camera splines 9859-9861, subtitle timings from the existing capture stub
            Add(CinematicSceneId.CrimsonIsleChuaArrival)
                .WithDuration(20500)
                .AtOrigin(-8246.130f, -994.471f, -235.956f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 9859).SplineShot(7733, 9860).SplineShot(13833, 9861)
                .WithActors(33420, 70325)
                .WithPlayerVisual(22516)
                .WithPlayerVisual(30484, 950)
                .WithText(578443, 1000, 2500)
                .WithText(578444, 2600, 8000)
                .WithText(578445, 8100, 12600)
                .WithText(578446, 12700, 16500);

            // Deradune arrival (camera creature 70267 is labelled GC030), the transport lands in Bloodfire Village
            Add(CinematicSceneId.DeraduneArrival)
                .WithDuration(26333)
                .AtOrigin(-5750.820f, -971.530f, -623.310f)
                .WithCamera(70267)
                .CutTo(8567, 8).CutTo(15633, 9).CutTo(18867, 10)
                .WithActors(26925)
                .WithPlayerVisual(14577)
                .WithTextsSpread(1000, 15000, (578447, 36), (578448, 44), (578449, 28), (578450, 48));

            // Zonaga appears in its lair, camera splines 7545-7547
            Add(CinematicSceneId.DeraduneZonagaIntro)
                .WithDuration(14666)
                .AtOrigin(-4845.729f, -962.984f, -732.910f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(33, 7545).SplineShot(4966, 7546).SplineShot(10600, 7547)
                .WithActors(26313)
                .WithPlayerVisual(14667);

            // GC016: the Bloodfeast at the altar of Bloodfire Village, Metal Maw breaks out of its cage, camera spline 7492
            // The cage model holds its own two sequences (250000-278333), the scene start was matched to the spline. Subtitle timings guessed.
            Add(CinematicSceneId.DeraduneMetalMawIntro)
                .WithDuration(26434)
                .AtOrigin(-5595.510f, -977.592f, -706.921f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 7492)
                .WithActors(26018)
                .WithPlayerVisual(14578)
                .WithPlayerVisual(15063, 500)
                .WithPlayerVisual(14824, 13000)
                .WithTextsSpread(500, 12500, (578452, 19), (578453, 34), (578454, 46), (578455, 30))
                .WithTextsSpread(13000, 25000, (578456, 31), (578458, 31), (578459, 24));

            // GC239: the Falkrin answer the ceremonial horn at Bloodtalon Perch and the negotiations go wrong
            // Origin guessed: WorldLocation2 51478 at Bloodtalon Perch, from the same build as the scene.
            Add(CinematicSceneId.DeraduneFalkrinDiplomacy)
                .WithDuration(45000)
                .AtOrigin(-4818.940f, -895.890f, -1881.370f)
                .WithCamera(73183)
                .CutTo(2967, 8).CutTo(4900, 9).CutTo(6900, 10).CutTo(13500, 11).CutTo(14833, 12).CutTo(16133, 13).CutTo(18000, 14)
                .CutTo(21333, 15).CutTo(23767, 16).CutTo(27867, 17).CutTo(31300, 18).CutTo(37567, 19)
                .WithActors(73184, 73185, 73186, 73187, 73188, 73189, 73190)
                .WithPlayerVisual(50703);

            // GC142: the stolen Exile transport (Levian Bay escape, Crimson Isle launch) lands at Lightreach Mission. Generic ship arrival on
            // WorldLocation2 49517 (identity facing copy of 20541, from the same build as the scene), unrotated. Engine sound none in data.
            Add(CinematicSceneId.EllevarArrival)
                .WithDuration(10000)
                .AtOrigin(-2601.710f, -789.645f, -3586.000f)
                .WithCamera(69831)
                .WithActors(69832);

            // GC066: the Stormseeker ritual in the Vein of the Flow ends with the High Priest dead, camera spline 14859
            Add(CinematicSceneId.EllevarHighShamanRitual)
                .WithDuration(16333)
                .AtOrigin(-3409.721f, -759.495f, -3455.009f)
                .WithSequenceVisual(SequenceVisualMisc01)
                .SplineShot(0, 14859)
                .WithActors(48586)
                .WithPlayerVisual(30674)
                .WithPlayerVisual(30557, 1000);
        }
    }
}
