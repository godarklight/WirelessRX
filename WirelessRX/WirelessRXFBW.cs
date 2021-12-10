using System;
using FSControl;
using WirelessRXLib;

namespace WirelessRX
{
    public class WirelessRXFBW : IFlyByWire
    {
        Message channelData;
        long expireTime;

        public void OnRegistered(FBWHostBase host)
        {

        }

        public void OnUnregistered(FBWHostBase host)
        {

        }

        public void OnProcessCtrlState(ref FSInputState state, Vehicle v)
        {
            //Don't do anything if the receiver isn't connected.
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime > expireTime)
            {
                return;
            }
            if (channelData.failsafe)
            {
                return;
            }
            int channelsToCopy = channelData.channels.Length;
            if (state.axes.Length < channelsToCopy)
            {
                channelsToCopy = state.axes.Length;
            }
            for (int i = 0; i < channelsToCopy; i++)
            {
                state.axes[i] = channelData.channels[i];
                //Clamp
                if (state.axes[i] < -1f)
                {
                    state.axes[i] = -1f;
                }
                if (state.axes[i] > 1f)
                {
                    state.axes[i] = 1f;
                }
            }
            //AETR
            state.roll = channelData.channels[0];
            state.pitch = -channelData.channels[1];
            state.throttle = (channelData.channels[2] + 1f) / 2f;
            state.yaw = channelData.channels[3];
        }

        public void SetChannels(Message channelData)
        {
            this.channelData = channelData;
            expireTime = DateTime.UtcNow.Ticks + TimeSpan.TicksPerSecond;
        }
    }
}