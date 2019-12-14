using System;

namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Allows to specify a delegate that determines whether a feature can be used.
    /// </summary>
    public class Ask : IFeaturePolicy
    {
        private readonly Func<FeatureContext, FeatureState> _state;
        
        public Ask(Func<FeatureContext, FeatureState> state) => _state = state;

        public FeatureState State(FeatureContext context) => _state(context);
    }
}