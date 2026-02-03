using System;
using wraithspire.engine.components;

namespace wraithspire.engine
{
    public abstract class Component
    {
        public GameObject GameObject { get; set; }
        public Transform Transform => GameObject?.Transform;

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
    }
}
