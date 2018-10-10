namespace UnityStandardAssets.CinematicEffects
{
    using UnityEngine;

    public sealed class ME_MinAttribute : PropertyAttribute
    {
        public readonly float min;

        public ME_MinAttribute(float min)
        {
            this.min = min;
        }
    }
}
