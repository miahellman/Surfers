using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tricks
{
    public class TrickManager : MonoBehaviour
    {
        public static TrickManager instance;

        [SerializeField] public Trick kickflip;
        [SerializeField] public Trick grind;
        [SerializeField] public Trick airTime;
        [SerializeField] public Trick wallride;

        private void Start()
        {
            instance = this;
        }
    }

    [System.Serializable]
    public class Trick
    {
        [SerializeField] public string trickName;
        [SerializeField] public int baseScore;
        // for single tricks like flips, this is how many one rotation is worth
        // for duration tricks like grind, this is how many points per second
        [SerializeField] public bool durationTrick;
    }
}

