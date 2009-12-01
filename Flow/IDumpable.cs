using System;

namespace Flow
{
	public interface IDumpable<T>
	{
		T Dump();
	}
}
