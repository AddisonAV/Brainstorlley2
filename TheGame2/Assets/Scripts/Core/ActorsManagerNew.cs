using System.Collections.Generic;
using UnityEngine;

namespace TheGame.Game
{
    public class ActorsManagerNew : MonoBehaviour
    {
        public List<ActorNew> Actors { get; private set; }
        public GameObject Player { get; private set; }

        public void SetPlayer(GameObject player) => Player = player;

        void Awake()
        {
            Actors = new List<ActorNew>();
        }
    }
}