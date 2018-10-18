using System;
using Eto.Forms;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    public class TimerAdapter : RTimer
    {
        UITimer _timer;

        public TimerAdapter()
        {
            _timer = new UITimer();
            _timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, EventArgs e) => OnElapsed(e);

        public override TimeSpan Interval
        {
            get => TimeSpan.FromSeconds(_timer.Interval);
            set => _timer.Interval = value.TotalSeconds;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public override void Start() => _timer.Start();

        public override void Stop() => _timer.Stop();
    }
}
