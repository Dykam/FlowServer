using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	class ProcessorDisposer : IDisposable
	{
		Action<Predicate<Request>> detach;
		Predicate<Request> processor;
		public ProcessorDisposer(Action<Predicate<Request>> detach, Predicate<Request> processor)
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
