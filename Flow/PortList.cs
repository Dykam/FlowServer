using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	/// <remarks>
	/// A list containing the ports to listen to. Automatically handles addition and removal of TcpListeners.
	/// </remarks>
	public class PortList : IEnumerable<int>
	{
		LinkedList<int> ports;
		internal PortList(IEnumerable<int> ports)
		{
			this.ports = new LinkedList<int>(ports);
		}

		internal event EventHandler<PortListEventArgs> PortAdded;
		protected void OnPortAdded(int port)
		{
			var e = PortAdded;
			if (e != null) {
				e(this, new PortListEventArgs(port));
			}
		}
		internal event EventHandler<PortListEventArgs> PortRemoved;
		protected void OnPortRemoved(int port)
		{
			var e = PortRemoved;
			if (e != null) {
				e(this, new PortListEventArgs(port));
			}
		}

		/// <value>
		/// When Get, returns if the router listens to <paramref name="index"/>.
		/// When Set, enables or disables listening to <paramref name="index"/>.
		/// </value>
		public bool this[int index]
		{
			get
			{
				return ports.Contains(index);
			}
			set
			{
				if (ports.Contains(index) == value) return;
				if (value) {
					ports.AddLast(index);
					OnPortAdded(index);
				} else {
					ports.Remove(index);
					OnPortRemoved(index);
				}
			}
		}

		internal class PortListEventArgs : EventArgs
		{
			public int Port { get; private set; }
			public PortListEventArgs(int port)
			{
				Port = port;
			}
		}

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> to iterate through the ports listened to.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/> to iterate through the ports listened to.
		/// </returns>
		public IEnumerator<int> GetEnumerator()
		{
			return ports.GetEnumerator();
		}
		
		/// <summary>
		/// Returns an <see cref="IEnumerator"/> to iterate through the ports listened to.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/> to iterate through the ports listened to.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ports.GetEnumerator();
		}
	}
}
