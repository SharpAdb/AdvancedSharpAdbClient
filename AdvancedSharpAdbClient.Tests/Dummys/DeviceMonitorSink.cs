using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DeviceMonitorSink
    {
        public DeviceMonitorSink(DeviceMonitor monitor)
        {
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            Monitor.DeviceChanged += OnDeviceChanged;
            Monitor.DeviceConnected += OnDeviceConnected;
            Monitor.DeviceDisconnected += OnDeviceDisconnected;

            ChangedEvents = new Collection<DeviceDataEventArgs>();
            DisconnectedEvents = new Collection<DeviceDataEventArgs>();
            ConnectedEvents = new Collection<DeviceDataEventArgs>();
        }

        public Collection<DeviceDataEventArgs> DisconnectedEvents { get; private set; }

        public Collection<DeviceDataEventArgs> ConnectedEvents { get; private set; }

        public Collection<DeviceDataEventArgs> ChangedEvents { get; private set; }

        public DeviceMonitor Monitor { get; private set; }

        public ManualResetEvent CreateEventSignal()
        {
            ManualResetEvent signal = new ManualResetEvent(false);
            Monitor.DeviceChanged += (sender, e) => signal.Set();
            Monitor.DeviceConnected += (sender, e) => signal.Set();
            Monitor.DeviceDisconnected += (sender, e) => signal.Set();
            return signal;
        }

        protected virtual void OnDeviceDisconnected(object sender, DeviceDataEventArgs e) => DisconnectedEvents.Add(e);

        protected virtual void OnDeviceConnected(object sender, DeviceDataEventArgs e) => ConnectedEvents.Add(e);

        protected virtual void OnDeviceChanged(object sender, DeviceDataEventArgs e) => ChangedEvents.Add(e);
    }
}
