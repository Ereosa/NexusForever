using System.Numerics;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Cinematic.Scene
{
    /// <summary>
    /// Data definition of a cinematic, played by <see cref="SceneCinematic"/>.
    /// </summary>
    /// <remarks>
    /// Most cinematics are baked into client models: every actor is a creature whose model (Art\Cinematics\...) holds the whole animation,
    /// all actors are spawned on the same scene origin and play the same model sequence, and the camera is attached in turn to the
    /// cinematic cameras of the camera actor model (ModelCamera Cinematic_01, Cinematic_02, ...).
    /// Older cinematics use Spline2 camera splines instead of a camera actor.
    /// </remarks>
    public sealed class CinematicScene
    {
        /// <summary>
        /// Visual effect that plays model sequence 7832 (Cinematic_Misc_00), the full timeline of a baked cinematic.
        /// </summary>
        public const uint DefaultSequenceVisual = 45237;

        /// <summary>
        /// Visual effect that mutes game audio while the cinematic plays (System_Cinematic_GameAudio_Mute).
        /// </summary>
        public const uint MuteGameAudioVisual = 21853;

        /// <summary>
        /// ModelCamera id of the first cinematic camera (Cinematic_01) in a camera actor model.
        /// </summary>
        public const uint FirstModelCamera = 7;

        public CinematicSceneId Id { get; }
        public ushort CinematicId { get; private set; }
        public uint Duration { get; private set; }
        public Vector3 Origin { get; private set; }
        public float Angle { get; private set; } = MathF.PI;
        public uint SequenceVisual { get; private set; } = DefaultSequenceVisual;

        public uint CameraCreatureId { get; private set; }
        public List<SceneCameraCut> CameraCuts { get; } = [];
        public List<SceneSplineShot> SplineShots { get; } = [];
        public ushort StartTransitionDuration { get; private set; } = 1500;
        public uint? EndTransition { get; private set; }

        public List<SceneActor> Actors { get; } = [];
        public List<SceneWorldActor> WorldActors { get; } = [];
        public ScenePlayer Player { get; private set; }
        public List<SceneVisual> PlayerVisuals { get; } = [];
        public List<SceneText> Texts { get; } = [];

        public CinematicScene(CinematicSceneId id)
        {
            Id = id;
            // the Cinematic table ids match the game cinematic numbers up to 102, later entries (103, 104) were appended for other scenes
            if ((ushort)id <= 102)
                CinematicId = (ushort)id;
        }

        /// <summary>
        /// Length of the cinematic, for baked cinematics the length of the camera model sequence.
        /// </summary>
        public CinematicScene WithDuration(uint duration)
        {
            Duration = duration;
            return this;
        }

        /// <summary>
        /// World position every baked actor is spawned on, the cinematic models are authored relative to it.
        /// </summary>
        public CinematicScene AtOrigin(float x, float y, float z)
        {
            Origin = new Vector3(x, y, z);
            return this;
        }

        public CinematicScene WithCinematicId(ushort cinematicId)
        {
            CinematicId = cinematicId;
            return this;
        }

        /// <summary>
        /// Visual effect played on every baked actor that starts the model sequence of the cinematic.
        /// </summary>
        public CinematicScene WithSequenceVisual(uint visualEffectId)
        {
            SequenceVisual = visualEffectId;
            return this;
        }

        /// <summary>
        /// Camera actor creature, the camera starts on <see cref="FirstModelCamera"/>.
        /// </summary>
        public CinematicScene WithCamera(uint creatureId)
        {
            CameraCreatureId = creatureId;
            return this;
        }

        /// <summary>
        /// Cut to another cinematic camera of the camera actor model.
        /// </summary>
        public CinematicScene CutTo(uint delay, uint modelCameraId, uint transitionType = 0)
        {
            CameraCuts.Add(new SceneCameraCut(delay, modelCameraId, transitionType));
            return this;
        }

        /// <summary>
        /// Camera follows a Spline2 camera spline.
        /// </summary>
        public CinematicScene SplineShot(uint delay, uint splineId, uint splineMode = 0, float speed = 1f)
        {
            SplineShots.Add(new SceneSplineShot(delay, splineId, splineMode, speed));
            return this;
        }

        public CinematicScene WithStartTransition(ushort duration)
        {
            StartTransitionDuration = duration;
            return this;
        }

        /// <summary>
        /// Delay of the white out at the end, defaults to 1.5 seconds before the end.
        /// </summary>
        public CinematicScene WithEndTransition(uint delay)
        {
            EndTransition = delay;
            return this;
        }

        /// <summary>
        /// Baked actors, spawned on the origin.
        /// </summary>
        public CinematicScene WithActors(params uint[] creatureIds)
        {
            foreach (uint creatureId in creatureIds)
                Actors.Add(new SceneActor(creatureId));
            return this;
        }

        /// <summary>
        /// Actor with its own position or race and sex filter.
        /// </summary>
        public CinematicScene WithActor(SceneActor actor)
        {
            Actors.Add(actor);
            return this;
        }

        /// <summary>
        /// Copies of world creatures near the origin, the cinematic hides every world unit while it plays.
        /// </summary>
        public CinematicScene WithWorldActors(float range, params uint[] creatureIds)
        {
            foreach (uint creatureId in creatureIds)
                WorldActors.Add(new SceneWorldActor(creatureId, range));
            return this;
        }

        /// <summary>
        /// A copy of the player attached to an attachment point of a baked platform actor.
        /// </summary>
        public CinematicScene WithPlayerOnPlatform(uint platformCreatureId, Func<Race, Sex, uint> attachment)
        {
            Player = new ScenePlayer(platformCreatureId, attachment, null);
            return this;
        }

        /// <summary>
        /// A copy of the player on the origin, the client plays the CinematicRace model of the player's race on it.
        /// </summary>
        public CinematicScene WithPlayerClone(Vector3? position = null, float? angle = null)
        {
            Player = new ScenePlayer(0, null, (position, angle));
            return this;
        }

        /// <summary>
        /// Visual effect on the player, usually the voice over or full sound mix of the cinematic.
        /// </summary>
        public CinematicScene WithPlayerVisual(uint visualEffectId, uint delay = 0, uint duration = 0)
        {
            PlayerVisuals.Add(new SceneVisual(visualEffectId, delay, duration));
            return this;
        }

        /// <summary>
        /// Subtitle line, shown from <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        public CinematicScene WithText(uint localisedTextId, uint start, uint end)
        {
            Texts.Add(new SceneText(localisedTextId, start, end));
            return this;
        }

        /// <summary>
        /// Subtitle lines spread over a time range, each shown for a share of the range matching its length.
        /// </summary>
        /// <remarks>
        /// Used when the line timing isn't known, the lines of one voice clip follow each other without pauses.
        /// </remarks>
        public CinematicScene WithTextsSpread(uint start, uint end, params (uint TextId, int Length)[] lines)
        {
            int total = lines.Sum(l => l.Length);
            double time = start;
            foreach ((uint textId, int length) in lines)
            {
                double next = time + (end - start) * (double)length / total;
                Texts.Add(new SceneText(textId, (uint)time, (uint)next - 33u));
                time = next;
            }
            return this;
        }
    }

    public record SceneCameraCut(uint Delay, uint ModelCameraId, uint TransitionType);

    public record SceneSplineShot(uint Delay, uint SplineId, uint SplineMode, float Speed);

    public record SceneVisual(uint VisualEffectId, uint Delay, uint Duration);

    public record SceneText(uint TextId, uint Start, uint End);

    public record SceneWorldActor(uint CreatureId, float Range);

    public record ScenePlayer(uint PlatformCreatureId, Func<Race, Sex, uint> Attachment, (Vector3? Position, float? Angle)? CloneAt);

    /// <summary>
    /// Baked or placed actor of a <see cref="CinematicScene"/>.
    /// </summary>
    public record SceneActor(uint CreatureId)
    {
        /// <summary>
        /// Position of the actor if it isn't baked relative to the origin.
        /// </summary>
        public Vector3? Position { get; init; }
        public float? Angle { get; init; }
        public uint Delay { get; init; }
        public Race? Race { get; init; }
        public Sex? Sex { get; init; }
        public List<SceneVisual> Visuals { get; init; }
        public List<(uint Delay, bool Hide)> Visibility { get; init; }
    }
}
