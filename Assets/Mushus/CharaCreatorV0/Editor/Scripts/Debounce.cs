using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Mushus.CharaCreatorV0
{
    internal class Debounce
    {
        int counter = 0;
        Action _callback = null;
        Coroutine _corountine = null;

        internal async void Run(Action callback, float interval)
        {
            _callback = callback;
            Reset();
            await WaitAndRun(interval);
        }

        internal void Reset()
        {
            counter++;
        }

        private async Task WaitAndRun(float time)
        {
            var myCounter = counter;
            await Task.Delay(TimeSpan.FromSeconds(time));
            if (myCounter != counter) return;
            _callback?.Invoke();
        }
    }
}