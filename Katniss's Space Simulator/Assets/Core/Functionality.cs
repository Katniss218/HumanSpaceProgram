using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    [System.Serializable]
    public abstract class Functionality
    {
        // Functionality is our generalized equivalent to PartModule.

        // Functionality is *NOT* a MonoBehaviour to allow easier implementation of unloaded vessel processing (we would process each part without instantiating it).

        // Functionalities will be just normal classes that are added to the part.



        // This is kinda bad, because we ultimately want it generalized if possible.
        public Part Part { get; set; }

        public abstract void Start();
        public abstract void Update();
        public abstract void FixedUpdate();
    }
}