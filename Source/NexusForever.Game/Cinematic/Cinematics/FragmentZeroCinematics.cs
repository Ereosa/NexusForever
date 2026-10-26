using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Message.Model.Cinematic;

namespace NexusForever.Game.Cinematic.Cinematics
{
    /// <summary>
    /// Fragment Zero (Expedition: They Came from Fragment Zero!) cinematics GC136 to GC140.
    /// </summary>
    /// <remarks>
    /// No packet captures exist, the cinematics are rebuilt from the client data the same way the captured Evil from the Ether ones work:
    /// - GC136, GC137 and GC138 are camera rigs: each actor (Creature2 "GCxxx - ...") plays its baked animation (visual effect 45237) at the
    ///   cinematic's origin (a world location with no facing, the actors face pi like Evil from the Ether), the camera cuts between the camera
    ///   model's attachments 7, 8, 9... at the start of each animation sequence of the camera model (m3 sequence table).
    /// - GC139 and GC140 have no camera model, their shots are the world 3180 camera splines (Spline2 with a field of view), whose lengths
    ///   match the shots of the rig cinematics where both exist (GC138).
    /// - Each plays its full mix sound (GCxxx_FragmentZero_..._FullMix) and mutes the game audio (visual effect 21853) like the captured ones.
    /// </remarks>
    public abstract class FragmentZeroCinematic : CinematicBase
    {
        private const uint VisualEffectActorAnimation = 45237;
        private const uint VisualEffectMuteGameAudio  = 21853;

        protected void SetupCommon(uint duration, uint fullMixVisualEffect)
        {
            Duration          = duration;
            InitialFlags      = CinematicFlags.EndImmediate | CinematicFlags.Unknown2 | CinematicFlags.NotifyServer;
            InitialCancelMode = CancelType.EndImmediate;
            CinematicId       = 0;

            StartTransition = new Transition(0, CameraAddFlags.AddCamera, 2, 1500, 0, 1500);
            EndTransition   = new Transition(duration - 1500, CameraAddFlags.WhiteOut, 0);

            Keyframes.AddRange(
            [
                new VisualEffect(VisualEffectMuteGameAudio, Player.Guid),
                new VisualEffect(fullMixVisualEffect, Player.Guid)
            ]);
        }

        /// <summary>
        /// Add actors that play their baked cinematic animation at the cinematic's origin.
        /// </summary>
        protected void AddRigActors(Vector3 origin, params uint[] creatureIds)
        {
            var position = new Position(origin);
            foreach (uint creatureId in creatureIds)
            {
                IActor actor = new Actor(creatureId, EntityCreateFlag.Immediate | EntityCreateFlag.HasInteractionPrereq, MathF.PI, position);
                AddActor(actor,
                [
                    new VisualEffect(VisualEffectActorAnimation)
                ]);
            }
        }

        /// <summary>
        /// Attach the camera to the camera actor, cutting to the next attachment at each of the supplied times.
        /// </summary>
        protected void AddRigCamera(uint cameraActorId, params uint[] cuts)
        {
            ICamera camera = new Camera(GetActor(cameraActorId), 7, 0, true, 0);
            for (uint i = 0; i < cuts.Length; i++)
            {
                camera.AddAttach(cuts[i], 8 + i);
                camera.AddTransition(cuts[i], 0);
            }

            AddCamera(camera);
        }

        /// <summary>
        /// Play the supplied camera splines one after the other, each for its length in milliseconds.
        /// </summary>
        protected void AddSplineCameras(params (uint Spline, uint Length)[] shots)
        {
            uint delay = 0;
            foreach ((uint spline, uint length) in shots)
            {
                AddCamera(new Camera(spline, 0, delay, 1f, useRotation: true));
                delay += length;
            }
        }

        /// <summary>
        /// Add an actor that plays its cinematic animation at a world position (the spline cinematics have no rig origin).
        /// </summary>
        protected void AddActorAt(uint creatureId, Vector3 position, float angle)
        {
            IActor actor = new Actor(creatureId, EntityCreateFlag.Immediate | EntityCreateFlag.HasInteractionPrereq, angle, new Position(position));
            AddActor(actor,
            [
                new VisualEffect(VisualEffectActorAnimation)
            ]);
        }

        protected override void Play()
        {
            base.Play();

            Player.Session.EnqueueMessageEncrypted(new ServerCinematicTransitionDurationSet
            {
                Type          = ScaleTransitionType.StartMinimized,
                DurationStart = 1500,
                DurationMid   = 0,
                DurationEnd   = 1500
            });

            foreach (IKeyframeAction keyframeAction in Keyframes)
                keyframeAction.Send(Player.Session);
        }
    }

    /// <summary>
    /// GC136 - Fragment Zero Arrival: Lola's ship flies in and lands, title logo.
    /// </summary>
    public class FragmentZeroOnArrival : FragmentZeroCinematic, IFragmentZeroOnArrival
    {
        // camera shots 0-6933, 6967-11800, 11833-27167 (Cine_FragmentZero_Arrival__camera.m3)
        protected override void Setup()
        {
            SetupCommon(27200, 46652); // Play_GC136_FragmentZero_Arrival_FullMix

            // camera, ship, lasers, logo; origin world location 49381
            AddRigActors(new Vector3(9868.743f, -765.50214f, -5836.857f), 69353, 69354, 69472, 69986);
            AddRigCamera(69353, 6967, 11833);
        }
    }

    /// <summary>
    /// GC137 - Fragment Zero Departure: the ship escapes the Skeech, title.
    /// </summary>
    public class FragmentZeroOnDeparture : FragmentZeroCinematic, IFragmentZeroOnDeparture
    {
        // one camera shot 33-18367 (Cine_FragmentZero_Departure__camera.m3)
        protected override void Setup()
        {
            SetupCommon(18400, 46653); // Play_GC137_FragmentZero_Departure_FullMix

            // camera, ship, skeech, title; origin world location 49396
            AddRigActors(new Vector3(9868.743f, -765.50214f, -5836.857f), 69368, 69369, 69370, 70025);
            AddRigCamera(69368);
        }
    }

    /// <summary>
    /// GC138 - Fragment Zero Hugo's Shield: Hugo gets the Life-Overseer's device working against the Skeech.
    /// </summary>
    public class FragmentZeroOnHugosShield : FragmentZeroCinematic, IFragmentZeroOnHugosShield
    {
        // eight camera shots (GC138_HugosShield__camera.m3), the same lengths as camera splines 22153 to 22160
        protected override void Setup()
        {
            SetupCommon(22033, 46654); // Play_GC138_FragmentZero_HugosShield_FullMix

            // camera, Hugo and device, Skeech 1-3, FX; origin world location 49395 (where splines 22153 to 22160 start)
            AddRigActors(new Vector3(9846.663f, -779.5f, -6272.428f), 69391, 69392, 69393, 69394, 69395, 69747);
            AddRigCamera(69391, 7367, 9600, 10700, 11567, 12900, 14567, 17700);
        }
    }

    /// <summary>
    /// GC139 - Fragment Zero Holo Pell Intro: the Life-Overseer's projection at the crash site wakes the Skeech.
    /// </summary>
    public class FragmentZeroOnPellIntro : FragmentZeroCinematic, IFragmentZeroOnPellIntro
    {
        protected override void Setup()
        {
            SetupCommon(24100, 46655); // Play_GC139_FragmentZero_PellIntro_FullMix

            AddActorAt(69326, new Vector3(9893f, -780.5f, -6031f), MathF.PI);
            AddSplineCameras((21993, 4167), (21994, 9967), (22016, 4967), (22017, 4967));
        }
    }

    /// <summary>
    /// GC140 - Fragment Zero Boss Intro: the Life-Overseer and his floating Skeech.
    /// </summary>
    public class FragmentZeroOnBossIntro : FragmentZeroCinematic, IFragmentZeroOnBossIntro
    {
        protected override void Setup()
        {
            SetupCommon(29900, 46656); // Play_GC140_FragmentZero_BossIntro_FullMix

            AddActorAt(69725, new Vector3(9617.5f, -815.4f, -6477f), 0f);
            AddActorAt(69726, new Vector3(9617f, -806f, -6476f), 0f);
            AddSplineCameras((22055, 4967), (22058, 4967), (22138, 9967), (22171, 9967));
        }
    }
}
