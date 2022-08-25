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
    public static class EventHelpers
    {
        public struct EventAnnounceInfo
        {
            public EventCard card;
            public float eventWarningDuration;
            public bool isEventStart;
            public bool fadeOnStart;

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
            tmp.fontSize = MSUConfig.eventMessageFontSize.Value;

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
                instance.transform.SetParent(hudInstance.mainContainer.transform, false);
                instance.GetComponent<EventTextController>().BeginFade();

                yield break;
            }
            orig(self, familySelectionChatString);
        }

        public static GameObject AnnounceEvent(EventAnnounceInfo announceInfo)
        {
            GameObject eventAnnouncerInstance = UnityEngine.Object.Instantiate(EventAnnouncer, hudInstance.mainContainer.transform);

            HGTextMeshProUGUI hgText = eventAnnouncerInstance.GetComponent<HGTextMeshProUGUI>();
            string token = announceInfo.isEventStart ? announceInfo.card.startMessageToken : announceInfo.card.endMessageToken;
            Color messageColor = announceInfo.card.messageColor;
            hgText.text = Language.GetString(token);
            hgText.color = messageColor;
            hgText.outlineColor = GetOutlineColor(messageColor);
            hgText.autoSizeTextContainer = false;

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