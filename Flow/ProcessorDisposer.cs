using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	class ProcessorDisposer : IDisposable
	{
		Action<Action<Request>> detach;
		Action<Request> processor;
		public ProcessorDisposer(Action<Action<Request>> detach, Action<Request> processor)
		{
			this.detach = detach;
			this.processor = processor;
		}

		public void Dispose()
		{
			detach(processor);
		}
	}
}
