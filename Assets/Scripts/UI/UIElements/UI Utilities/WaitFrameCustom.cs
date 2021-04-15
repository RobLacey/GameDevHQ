using UnityEngine;

namespace UIElements
{
    public class WaitFrameCustom : CustomYieldInstruction
    {
        private int _frameTarget;
        private int _frameCount;

        public CustomYieldInstruction SetFrameTarget(int target)
        {
            _frameTarget = target;
            return this;
        }

        public override bool keepWaiting => Counter();

        private bool Counter()
        {
            if (_frameCount == _frameTarget)
            {
                _frameCount = 0;
                return false;
            }

            _frameCount++;
            return true;
        }
    }
}