using System;
using System.Collections.Generic;
using System.Threading;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DeviceMonitorSink
    {
        public DeviceMonitorSink(DeviceMonitor monitor)
        {
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            Monitor.DeviceChanged += OnDeviceChanged;
            Monitor.DeviceNotified += OnDeviceNotified;
            Monitor.DeviceConnected += OnDeviceConnected;
            Monitor.DeviceListChanged += OnDeviceListChanged;
            Monitor.DeviceDisconnected += OnDeviceDisconnected;

            ChangedEvents = [];
            NotifiedEvents = [];
            ConnectedEvents = [];
            ListChangedEvents = [];
            DisconnectedEvents = [];
        }

        public void ResetSignals()
        {
            ChangedEvents.Clear();
            NotifiedEvents.Clear();
            ConnectedEvents.Clear();
            ListChangedEvents.Clear();
            DisconnectedEvents.Clear();
        }

        public List<DeviceDataConnectEventArgs> DisconnectedEvents { get; init; }

        public List<DeviceDataNotifyEventArgs> ListChangedEvents { get; init; }

        public List<DeviceDataConnectEventArgs> ConnectedEvents { get; init; }

        public List<DeviceDataNotifyEventArgs> NotifiedEvents { get; init; }

        public List<DeviceDataChangeEventArgs> ChangedEvents { get; init; }

        public DeviceMonitor Monitor { get; init; }

        public ManualResetEvent CreateEventSignal()
        {
            ManualResetEvent signal = new(false);
            Monitor.DeviceNotified += (sender, e) => signal.Set();
            return signal;
        }

        protected virtual void OnDeviceDisconnected(object sender, DeviceDataConnectEventArgs e) => DisconnectedEvents.Add(e);

        protected virtual void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e) => ListChangedEvents.Add(e);

        protected virtual void OnDeviceConnected(object sender, DeviceDataConnectEventArgs e) => ConnectedEvents.Add(e);

        protected virtual void OnDeviceNotified(object sender, DeviceDataNotifyEventArgs e) => NotifiedEvents.Add(e);

        protected virtual void OnDeviceChanged(object sender, DeviceDataChangeEventArgs e) => ChangedEvents.Add(e);
    }
}
