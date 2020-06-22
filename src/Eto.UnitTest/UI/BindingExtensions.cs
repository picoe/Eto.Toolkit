using Eto.Forms;

namespace Eto.UnitTest.UI
{
    static class BindingExtensions
	{
		public static IndirectBinding<T> Invoke<T>(this IndirectBinding<T> binding)
		{
			return new DelegateBinding<object, T>(
				m => Application.Instance.Invoke(() => binding.GetValue(m)),
				(m, val) => Application.Instance.Invoke(() => binding.SetValue(m, val)),
				addChangeEvent: (m, ev) => binding.AddValueChangedHandler(m, ev),
				removeChangeEvent: binding.RemoveValueChangedHandler
			);
		}
	}
}
