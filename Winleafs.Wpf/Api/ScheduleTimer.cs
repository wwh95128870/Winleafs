﻿using System;
using System.Threading.Tasks;
using System.Timers;

using NLog;
using Winleafs.Api;
using Winleafs.Models.Enums;
using Winleafs.Models.Models;
using Winleafs.Wpf.Api.Effects;

namespace Winleafs.Wpf.Api
{
    public class ScheduleTimer
    {
        private static readonly string _defaultPreviouslyActivatedEffect = "_NO_PREVIOUSLY_ACTIVATED_EFFECT_";

        private readonly Timer _timer;
        private readonly Orchestrator _orchestrator;
        private string _previouslyActivatedEffect;

        public ScheduleTimer(Orchestrator orchestrator)
        {
            _orchestrator = orchestrator;

            _timer = new Timer(60000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Starts the timer and fires the event to select an effect.
        /// </summary>
        public void StartTimer(bool sync = false)
        {
            _previouslyActivatedEffect = _defaultPreviouslyActivatedEffect; //Reset the previously activated effect

            _timer.Start();

            if (sync)
            {
                Task.Run(() => SetEffectsForDevices()).Wait();
            }
            else
            {
                Task.Run(() => SetEffectsForDevices());
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            _timer.Stop();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Task.Run(() => SetEffectsForDevices());
        }

        private async Task SetEffectsForDevices()
        {
            if (_orchestrator.Device.OperationMode == OperationMode.Schedule)
            {
                var activeTrigger = UserSettings.Settings.GetActiveTimeTriggerForDevice(_orchestrator.Device.Name);

                //Only switch effects if the effect is different from the previously activated effect
                if ((activeTrigger == null && _previouslyActivatedEffect != null) || (activeTrigger != null && activeTrigger.EffectName != _previouslyActivatedEffect))
                {
                    if (activeTrigger == null)
                    {
                        var client = NanoleafClient.GetClientForDevice(_orchestrator.Device);

                        //There are no triggers so the lights can be turned off if it is not off already
                        await client.StateEndpoint.SetStateWithStateCheckAsync(false);
                    }
                    else
                    {
                        await _orchestrator.ActivateEffect(activeTrigger.EffectName, activeTrigger.Brightness);
                    }

                    _previouslyActivatedEffect = activeTrigger?.EffectName;
                }
            }
        }
    }
}
