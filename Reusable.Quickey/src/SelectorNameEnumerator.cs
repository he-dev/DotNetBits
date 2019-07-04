using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    public interface ISelectorNameEnumerator
    {
        /// <summary>
        /// Enumerates selector-names for each member from last to first.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<IImmutableList<SelectorName>> EnumerateSelectorNames(Selector selector);
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class SelectorNameEnumeratorAttribute : Attribute, ISelectorNameEnumerator
    {
        public IEnumerable<IImmutableList<SelectorName>> EnumerateSelectorNames(Selector selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return
                from m in selector.Members()
                where m.IsDefined(typeof(SelectorNameFactoryAttribute))
                let selectorNames = m.GetCustomAttributes<SelectorNameFactoryAttribute>().Select(f => f.CreateSelectorName(selector)).ToImmutableList()
                where selectorNames.Any()
                select selectorNames;
        }
    }
}