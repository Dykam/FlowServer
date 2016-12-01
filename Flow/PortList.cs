using System;
using System.Collections;
using System.Collections.Generic;

namespace Flow
{
    /// <remarks>
    ///     A list containing the ports to listen to. Automatically handles addition and removal of TcpListeners.
    /// </remarks>
    public class PortList : IEnumerable<int>
    {
        private readonly LinkedList<int> _ports;

        internal PortList(IEnumerable<int> ports)
        {
            _ports = new LinkedList<int>(ports);
        }

        /// <value>
        ///     When Get, returns if the router listens to <paramref name="index" />.
        ///     When Set, enables or disables listening to <paramref name="index" />.
        /// </value>
        public bool this[int index]
        {
            get { return _ports.Contains(index); }
            set
            {
                if (_ports.Contains(index) == value) return;
                if (value)
                {
                    _ports.AddLast(index);
                    OnPortAdded(index);
                }
                else
                {
                    _ports.Remove(index);
                    OnPortRemoved(index);
                }
            }
        }

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> to iterate through the ports listened to.
        /// </summary>
        /// <returns>
        ///     A <see cref="IEnumerator" /> to iterate through the ports listened to.
        /// </returns>
        public IEnumerator<int> GetEnumerator()
        {
            return _ports.GetEnumerator();
        }

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> to iterate through the ports listened to.
        /// </summary>
        /// <returns>
        ///     A <see cref="IEnumerator" /> to iterate through the ports listened to.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _ports.GetEnumerator();
        }

        internal event EventHandler<PortListEventArgs> PortAdded;

        protected void OnPortAdded(int port)
        {
            PortAdded?.Invoke(this, new PortListEventArgs(port));
        }

        internal event EventHandler<PortListEventArgs> PortRemoved;

        protected void OnPortRemoved(int port)
        {
            PortRemoved?.Invoke(this, new PortListEventArgs(port));
        }

        internal class PortListEventArgs : EventArgs
        {
            public PortListEventArgs(int port)
            {
                Port = port;
            }

            public int Port { get; private set; }
        }
    }
}