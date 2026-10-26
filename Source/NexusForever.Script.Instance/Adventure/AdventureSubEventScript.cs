using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure
{
    /// <summary>
    /// Sub-events of the adventures driven by an <see cref="AdventureEventScript"/>: they spawn their entities and report their objectives.
    /// </summary>
    [ScriptFilterOwnerId(
        418, 420, 421, 422, 423, 424, 425, 426, 427, 428, 429, 430, 431, 432, 433, 434,  // The Hycrest Insurrection
        57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, // The Siege of Tempest Refuge
        78, 83, 84, 90, 91, 204,
        672, 674, 675, 676, 695,                                                         // Levian Bay
        109, 111, 112, 113, 114, 151, 281, 282, 283, 284, 285, 286, 288, 289, 290, 291,  // Crimelords of Whitevale
        292, 293, 294, 295, 296, 298, 299, 300, 301, 302, 310, 315, 317, 367, 368, 369,
        28, 29, 33, 34, 35, 36, 37, 38, 40, 41, 45, 46, 47, 48, 50, 51, 54, 55, 56, 77,  // The Malgrave Trail
        79, 82, 101, 102, 103, 104, 219, 220, 234, 239, 513, 514, 515,
        578, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 618, 619, 620,       // Battlebots (Farside)
        622, 624, 625, 626, 640,
        170, 171, 175, 176)]                                                                       // War of the Wilds
    public class AdventureSubEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;

        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(0u);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            foreach (IPublicEvent adventure in publicEvent.Map.PublicEventManager.GetEvents().ToList())
                adventure.InvokeScriptCollection<AdventureEventScript>(s => s.OnSubObjective(publicEvent, objective));
        }
    }
}
