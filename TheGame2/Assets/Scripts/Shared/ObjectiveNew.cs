using System;
using UnityEngine;

namespace TheGame.Game
{
    public abstract class ObjectiveNew : MonoBehaviour
    {
        [Tooltip("Name of the objective that will be shown on screen")]
        public string Title;

        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Description;

        [Tooltip("Whether the objective is required to win or not")]
        public bool IsOptional;

        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;

        public bool IsCompleted { get; private set; }
        public bool IsBlocking() => !(IsOptional || IsCompleted);

        public static event Action<ObjectiveNew> OnObjectiveCreated;
        public static event Action<ObjectiveNew> OnObjectiveCompleted;

        protected virtual void Start()
        {
            OnObjectiveCreated?.Invoke(this);

            DisplayMessageEvent displayMessage = EventsNew.DisplayMessageEvent;
            displayMessage.Message = Title;
            displayMessage.DelayBeforeDisplay = 0.0f;
            EventManagerNew.Broadcast(displayMessage);
        }

        public void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            ObjectiveUpdateEvent evt = EventsNew.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManagerNew.Broadcast(evt);
        }

        public void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            IsCompleted = true;

            ObjectiveUpdateEvent evt = EventsNew.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManagerNew.Broadcast(evt);

            OnObjectiveCompleted?.Invoke(this);
        }
    }
}