using Eto.Forms;
using Eto.Drawing;

namespace Eto.UnitTest.UI
{
    class UnitTestProgressBar : Drawable
	{
		float progress;
		Color color = Colors.Green;
		public float Progress
		{
			get => progress;
			set
			{
				if (progress != value)
				{
					progress = value;
					Invalidate();
				}
			}
		}

		public Color Color
		{
			get => color;
			set
			{
				if (color != value)
				{
					color = value;
					Invalidate();
				}
			}
		}

		public UnitTestProgressBar()
		{
			Size = new Size(200, 5);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var size = new SizeF(Width * progress, Height);
			e.Graphics.FillRectangle(Color, 0, 0, size.Width, size.Height);
		}
	}
}
