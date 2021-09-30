using System.Collections.Generic;
using UnityEngine;

namespace TheGame.Game
{
    public class ObjectiveManagerNew : MonoBehaviour
    {
        List<ObjectiveNew> m_Objectives = new List<ObjectiveNew>();
        bool m_ObjectivesCompleted = false;

        void Awake()
        {
            ObjectiveNew.OnObjectiveCreated += RegisterObjective;
        }

        void RegisterObjective(ObjectiveNew objective) => m_Objectives.Add(objective);

        void Update()
        {
            if (m_Objectives.Count == 0 || m_ObjectivesCompleted)
                return;

            for (int i = 0; i < m_Objectives.Count; i++)
            {
                // pass every objectives to check if they have been completed
                if (m_Objectives[i].IsBlocking())
                {
                    // break the loop as soon as we find one uncompleted objective
                    return;
                }
            }

            m_ObjectivesCompleted = true;
            EventManagerNew.Broadcast(EventsNew.AllObjectivesCompletedEvent);
        }

        void OnDestroy()
        {
            ObjectiveNew.OnObjectiveCreated -= RegisterObjective;
        }
    }
}