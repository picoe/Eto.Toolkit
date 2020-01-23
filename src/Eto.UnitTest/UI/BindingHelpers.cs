using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Eto.UnitTest.UI
{
    static class BindingHelpers
    {
        internal interface ICastItems<T>
            where T : IBindable
        {
            BindableBinding<T, IEnumerable<TTo>> To<TTo>();
        }

        class CastItemsHelper<T, TFrom> : ICastItems<T>
            where T : IBindable
        {
            public BindableBinding<T, IEnumerable<TFrom>> Binding { get; set; }

            public BindableBinding<T, IEnumerable<TTo>> To<TTo>()
            {
                return Binding.Convert(source => source.Cast<TTo>(), list => list.Cast<TFrom>());
            }
        }

        internal static ICastItems<T> CastItems<T, TFrom>(this BindableBinding<T, IEnumerable<TFrom>> binding)
            where T : Eto.Forms.IBindable
        {
            return new CastItemsHelper<T, TFrom> { Binding = binding };
        }
    }
}
