using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIController.Serial {
    public class AnalogDataDispatcher {
        private SerialDevice serial;

        private readonly ConcurrentCircularBuffer<AnalogueData> buffer;

        private readonly Action<AnalogueData> onDataReceived;

        public AnalogDataDispatcher(string port, int baud, Action<AnalogueData> onDataReceived) {
            if (onDataReceived == null) {
                throw new NullReferenceException("callback cannot be null");
            }

            this.onDataReceived = onDataReceived;
            this.buffer = new ConcurrentCircularBuffer<AnalogueData>(200);
            this.serial = new SerialDevice(this.OnDataReceivedMTA, port, baud);
        }

        public void Open() {
            this.serial.Connect();
        }

        public void Close() {
            this.serial.Disconnect();
        }

        public bool TryGetNextData(out AnalogueData data) {
            data = this.buffer.Pop();
            if (data.A0 == 0 && data.A1 == 0 && data.A2 == 0 && data.A3 == 0) {
                return false;
            }

            return true;
        }

        public void PopQueue() {
            if (TryGetNextData(out AnalogueData data)) {
                this.onDataReceived(data);
            }
        }

        private void OnDataReceivedMTA(AnalogueData data) {
            this.buffer.Push(data);
        }
    }
}
