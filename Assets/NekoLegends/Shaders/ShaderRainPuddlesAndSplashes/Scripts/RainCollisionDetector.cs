using System.Collections.Generic;
using UnityEngine;

namespace NekoLegends
{
    
    // New child script to detect collisions
    public class RainCollisionDetector : MonoBehaviour
    {
        private RainArea _parentSplash;

        void Awake()
        {
            _parentSplash = GetComponentInParent<RainArea>();
            if (_parentSplash == null)
            {
                Debug.LogError("RainCollisionDetector requires a RainArea script on a parent object!");
            }
        }

        void OnParticleCollision(GameObject other)
        {
            if (_parentSplash != null)
            {
                _parentSplash.HandleCollisionFromChild(other);
            }
        }
    }

}