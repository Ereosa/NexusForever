using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Message.Model.Cinematic;

namespace NexusForever.Game.Cinematic.Scene
{
    /// <summary>
    /// Plays a <see cref="CinematicScene"/> definition.
    /// </summary>
    public class SceneCinematic : CinematicBase
    {
        private const EntityCreateFlag ActorCreateFlags = EntityCreateFlag.Immediate | EntityCreateFlag.HasInteractionPrereq;

        public CinematicScene Scene { get; }

        public SceneCinematic(CinematicScene scene)
        {
            Scene = scene;
        }

        protected override void Setup()
        {
            Duration          = Scene.Duration;
            CinematicId       = Scene.CinematicId;
            InitialFlags      = CinematicFlags.EndImmediate | CinematicFlags.Unknown2 | CinematicFlags.NotifyServer;
            InitialCancelMode = CancelType.EndImmediate;
            StartTransition   = new Transition(0, CameraAddFlags.AddCamera, 2, Scene.StartTransitionDuration, 0, 1500);
            EndTransition     = new Transition(Scene.EndTransition ?? (Scene.Duration > 1500u ? Scene.Duration - 1500u : 0u), CameraAddFlags.WhiteOut, 0);

            SetupActors();
            SetupPlayer();
            SetupCamera();

            foreach (SceneText text in Scene.Texts)
                AddText(text.TextId, text.Start, text.End);

            Keyframes.Add(new VisualEffect(CinematicScene.MuteGameAudioVisual, Player.Guid));
            foreach (SceneVisual visual in Scene.PlayerVisuals)
                Keyframes.Add(new VisualEffect(visual.VisualEffectId, Player.Guid, delay: visual.Delay, duration: visual.Duration));
        }

        private Position OriginPosition => new(Scene.Origin);

        private void SetupActors()
        {
            foreach (SceneActor sceneActor in Scene.Actors)
            {
                if (sceneActor.Race.HasValue && sceneActor.Race != Player.Race)
                    continue;
                if (sceneActor.Sex.HasValue && sceneActor.Sex != Player.Sex)
                    continue;

                var actor = new Actor(sceneActor.CreatureId, ActorCreateFlags, sceneActor.Angle ?? Scene.Angle,
                    sceneActor.Position.HasValue ? new Position(sceneActor.Position.Value) : OriginPosition, sceneActor.Delay);

                List<IVisualEffect> visuals = sceneActor.Visuals != null
                    ? sceneActor.Visuals.Select(v => (IVisualEffect)new VisualEffect(v.VisualEffectId, delay: v.Delay)).ToList()
                    : [new VisualEffect(Scene.SequenceVisual)];
                AddActor(actor, visuals);

                if (sceneActor.Visibility != null)
                    foreach ((uint delay, bool hide) in sceneActor.Visibility)
                        actor.AddVisibility(delay, hide);
            }

            // every world unit is hidden while the cinematic plays, units that are part of the scene are copied
            foreach (SceneWorldActor worldActor in Scene.WorldActors)
            {
                foreach (IWorldEntity entity in Player.GetVisibleCreature<IWorldEntity>(worldActor.CreatureId))
                {
                    if (Vector3.Distance(entity.Position, Scene.Origin) > worldActor.Range)
                        continue;

                    AddActor(new Actor(worldActor.CreatureId, ActorCreateFlags, entity.Rotation.X, new Position(entity.Position)), []);
                }
            }
        }

        private void SetupPlayer()
        {
            ScenePlayer scenePlayer = Scene.Player;
            if (scenePlayer == null)
                return;

            if (scenePlayer.CloneAt.HasValue)
            {
                (Vector3? position, float? angle) = scenePlayer.CloneAt.Value;
                AddActor(new Actor(0, EntityCreateFlag.UseDefaultBirthSequence | ActorCreateFlags, angle ?? Scene.Angle,
                    position.HasValue ? new Position(position.Value) : OriginPosition), []);
                return;
            }

            IActor platform = GetActor(scenePlayer.PlatformCreatureId);
            if (platform == null)
            {
                platform = new Actor(scenePlayer.PlatformCreatureId, ActorCreateFlags, Scene.Angle, OriginPosition);
                AddActor(platform, [new VisualEffect(Scene.SequenceVisual)]);
            }

            SetAsPlayerActor(platform, OriginPosition, scenePlayer.Attachment(Player.Race, Player.Sex));
        }

        private void SetupCamera()
        {
            if (Scene.CameraCreatureId != 0u)
            {
                IActor cameraActor = GetActor(Scene.CameraCreatureId);
                if (cameraActor == null)
                {
                    cameraActor = new Actor(Scene.CameraCreatureId, ActorCreateFlags, Scene.Angle, OriginPosition);
                    AddActor(cameraActor, [new VisualEffect(Scene.SequenceVisual)]);
                }

                var camera = new Camera(cameraActor, Scene.FirstCamera, 0, true, 0);
                foreach (SceneCameraCut cut in Scene.CameraCuts)
                {
                    camera.AddAttach(cut.Delay, cut.ModelCameraId);
                    camera.AddTransition(cut.Delay, cut.TransitionType);
                }

                AddCamera(camera);
            }

            foreach (SceneSplineShot shot in Scene.SplineShots)
                AddCamera(new Camera(shot.SplineId, shot.SplineMode, shot.Delay, shot.Speed, useRotation: true));
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
}
