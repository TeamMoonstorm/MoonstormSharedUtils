using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using RoR2.UI;
using Moonstorm.Components;

namespace Moonstorm
{
    /// <summary>
    /// Class for handling Event Messages
    /// </summary>
    public static class EventHelpers
    {
        /// <summary>
        /// Represents a Data structure for announcing an event
        /// </summary>
        public struct EventAnnounceInfo
        {
            /// <summary>
            /// The EventCard, the text and colour are taken from this
            /// </summary>
            public EventCard card;
            /// <summary>
            /// How long the message lasts, if <see cref="isEventStart"/> is false, the duration is cut in half
            /// </summary>
            public float eventWarningDuration;
            /// <summary>
            /// Wether to display the start message or the end message
            /// </summary>
            public bool isEventStart;
            /// <summary>
            /// Wether to begin the fade in as soon as the event message becomes instantiated
            /// </summary>
            public bool fadeOnStart;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="card">The EventCard, the text and colour are taken from this</param>
            /// <param name="warningDuration">How long the message lasts, if <see cref="isEventStart"/> is false, the duration is cut in half</param>
            /// <param name="isEventStart">Wether to display the start message or the end message</param>
            public EventAnnounceInfo(EventCard card, float warningDuration, bool isEventStart)
            {
                this.card = card;
                eventWarningDuration = warningDuration;
                this.isEventStart = isEventStart;
                fadeOnStart = true;
            }
        }

        private static GameObject EventAnnouncer = MoonstormSharedUtils.MSUAssetBundle.LoadAsset<GameObject>("EventAnnouncer");
        private static HUD hudInstance;
        [SystemInitializer(typeof(EventCatalog))]
        private static void SystemInit()
        {
            On.RoR2.UI.HUD.Awake += (orig, self) =>
            {
                hudInstance = self;
                orig(self);
            };

            HGTextMeshProUGUI tmp = EventAnnouncer.GetComponent<HGTextMeshProUGUI>();
            float size = MSUConfig.eventMessageFontSize.Value;

            tmp.fontSize = size;
            tmp.fontSizeMax = size;

            //ClassicStageInfo.monsterFamilyChance = 1000;
            if(MSUConfig.familyEventUsesEventAnnouncementInsteadOfChatMessage.Value)
            {
                //Make the family event message use an EventAnnouncement
                On.RoR2.ClassicStageInfo.BroadcastFamilySelection += ShowAnnouncement;
            }

        }

        private static System.Collections.IEnumerator ShowAnnouncement(On.RoR2.ClassicStageInfo.orig_BroadcastFamilySelection orig, ClassicStageInfo self, string familySelectionChatString)
        {
            GameObject instance = AnnounceFamilyEvent(familySelectionChatString, Color.white, 9);
            if(instance)
            {
                yield return new WaitForSeconds(4);
                Transform transform = instance.transform;
                transform.SetParent(hudInstance.mainContainer.transform, false);
                instance.GetComponent<RectTransform>().anchoredPosition = new Vector3(MSUConfig.eventMessageXOffset.Value, MSUConfig.eventMessageYOffset.Value);
                instance.GetComponent<EventTextController>().BeginFade();

                yield break;
            }
            orig(self, familySelectionChatString);
        }

        /// <summary>
        /// Announces a new event, using the Event Message system
        /// </summary>
        /// <param name="announceInfo">The announcement info</param>
        /// <returns>The EventMessage GameObject</returns>
        public static GameObject AnnounceEvent(EventAnnounceInfo announceInfo)
        {
            GameObject eventAnnouncerInstance = UnityEngine.Object.Instantiate(EventAnnouncer, hudInstance.mainContainer.transform, false);
            eventAnnouncerInstance.GetComponent<RectTransform>().anchoredPosition = new Vector3(MSUConfig.eventMessageXOffset.Value, MSUConfig.eventMessageYOffset.Value);

            HGTextMeshProUGUI hgText = eventAnnouncerInstance.GetComponent<HGTextMeshProUGUI>();
            string token = announceInfo.isEventStart ? announceInfo.card.startMessageToken : announceInfo.card.endMessageToken;
            Color messageColor = announceInfo.card.messageColor;
            hgText.text = Language.GetString(token);
            hgText.color = messageColor;
            hgText.outlineColor = GetOutlineColor(messageColor);
            hgText.autoSizeTextContainer = false;
            hgText.enabled = true;

            EventTextController textController = eventAnnouncerInstance.GetComponent<EventTextController>();
            textController.warningDuration = announceInfo.isEventStart ? announceInfo.eventWarningDuration : announceInfo.eventWarningDuration / 2;
            textController.fadeOnStart = announceInfo.fadeOnStart;
            return eventAnnouncerInstance;
        }

        private static GameObject AnnounceFamilyEvent(string token, Color color, float duration)
        {
            GameObject eventAnnouncerInstance = null;
            try
            {
                eventAnnouncerInstance = UnityEngine.Object.Instantiate(EventAnnouncer);

                HGTextMeshProUGUI hgText = eventAnnouncerInstance.GetComponent<HGTextMeshProUGUI>();
                string formattedToken = Language.GetString(token);
                string[] splitString = formattedToken.Split(']');
                hgText.text = $"{splitString[0]}]\n{splitString[1]}";
                hgText.color = color;
                hgText.outlineColor = GetOutlineColor(color);
                hgText.autoSizeTextContainer = false;

                EventTextController textController = eventAnnouncerInstance.GetComponent<EventTextController>();
                textController.warningDuration = duration;
                textController.fadeOnStart = false;
                return eventAnnouncerInstance;
            }
            catch (Exception ex)
            {
                MSULog.Error($"Cannot announce FamilyEvent! reason: {ex}");

                if (eventAnnouncerInstance)
                    UnityEngine.Object.Destroy(eventAnnouncerInstance);

                return null;
            }
        }

        private static Color32 GetOutlineColor(Color messageColor)
        {
            Color.RGBToHSV(messageColor, out float hue, out float sat, out float light);

            if(light > 0.5)
            {
                float newSat = Mathf.Clamp01(sat - 0.5f);
                float newLight = Mathf.Clamp01(light - 0.5f);
                return Color.HSVToRGB(hue, newSat, newLight);
            }
            else
            {
                float newSat = Mathf.Clamp01(sat + 0.5f);
                float newLight = Mathf.Clamp01(light + 0.5f);
                return Color.HSVToRGB(hue, newSat, newLight);
            }
        }
    }
}