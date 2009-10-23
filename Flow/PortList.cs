using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	public class PortList : IEnumerable<int>
	{
		LinkedList<int> ports;
		public PortList(IEnumerable<int> ports)
		{
			this.ports = new LinkedList<int>(ports);
		}

		public event EventHandler<PortListEventArgs> PortAdded;
		protected void OnPortAdded(int port)
		{
			var e = PortAdded;
			if (e != null) {
				e(this, new PortListEventArgs(port));
			}
		}
		public event EventHandler<PortListEventArgs> PortRemoved;
		protected void OnPortRemoved(int port)
		{
			var e = PortRemoved;
			if (e != null) {
				e(this, new PortListEventArgs(port));
			}
		}

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

		public class PortListEventArgs : EventArgs
		{
			public int Port { get; private set; }
			public PortListEventArgs(int port)
			{
				Port = port;
			}
		}

		public IEnumerator<int> GetEnumerator()
		{
			return ports.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ports.GetEnumerator();
		}
	}
}
