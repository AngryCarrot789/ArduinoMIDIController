using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MIDIController.Serial {
    public class SerialReader : IDisposable {
        private static int THREAD_COUNT = 0;
        private readonly Thread Thread;
        private bool _threadCanLive = true;
        private volatile bool _canRun;

        /// <summary>
        /// The serial port reference
        /// </summary>
        public SerialPort Port { get; }

        private Action<AnalogueData> _onDataReceived;

        public SerialReader(SerialPort port, Action<AnalogueData> dataReceivedCallback) {
            if (port == null) {
                throw new ArgumentNullException(nameof(port), "Port cannot be null");
            }
            if (dataReceivedCallback == null) {
                throw new ArgumentNullException(nameof(dataReceivedCallback), "Callback cannot be null");
            }

            this.Port = port;
            this._onDataReceived = dataReceivedCallback;
            this._canRun = false;
            this.Thread = new Thread(this.ReaderMain) {
                Name = "Serial Port Reader " + (THREAD_COUNT++),
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };

            this.Thread.Start();
        }

        public SerialReader(SerialPort serialPort) {
            if (serialPort == null) {
                throw new ArgumentNullException(nameof(serialPort), "Serial Port cannot be null");
            }

            this.Port = serialPort;
            this._canRun = false;
            this.Thread = new Thread(this.ReaderMain) {
                Name = "Serial Port Reader " + (THREAD_COUNT++),
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };

            this.Thread.Start();
        }

        /// <summary>
        /// Enabled data to be received from the serial port
        /// </summary>
        public void Enable() {
            if (this._canRun == true) {
                throw new Exception("Already enabled");
            }
            // if (this._onLineReceived == null) {
            //     throw new NullReferenceException("Callback cannot be null");
            // }

            this._canRun = true;
        }

        /// <summary>
        /// Disabled data being received from the serial port
        /// </summary>
        public void Disable() {
            if (!this._canRun) {
                throw new Exception("Already disabled");
            }

            this._canRun = false;
        }

        private void ReaderMain() {
            byte[] buffer = new byte[4];
            int index = 0;
            while (this._threadCanLive) {
                if (this._canRun) {
                    SerialPort port = this.Port;
                    if (port.IsOpen) {
                        int readable = port.BytesToRead;
                        if (readable > 0) {
                            while (readable > 0) {
                                if (this._canRun) {
                                    byte read = (byte)port.ReadByte();
                                    readable = port.BytesToRead;

                                    if (read == 129) {
                                        this._onDataReceived(new AnalogueData() { A0 = buffer[0], A1 = buffer[1], A2 = buffer[2], A3 = buffer[3] });
                                        index = 0;
                                        continue;
                                    }

                                    // will usually only happen if the 5th signal character gets lost
                                    if (index == 4) {
                                        continue;
                                    }

                                    buffer[index++] = read;
                                    continue;
                                }

                                readable = 0;
                            }
                        }

                        Thread.Sleep(1);
                    }
                    else {
                        Thread.Sleep(1);
                    }
                }
                else {
                    Thread.Sleep(1);
                }
            }
        }

        public void Dispose() {
            this._threadCanLive = false;
            this._canRun = false;
            this.Thread.Join();
            this._onDataReceived = null;
        }
    }
}
