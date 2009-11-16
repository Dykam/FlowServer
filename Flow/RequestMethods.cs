using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
	/// <remarks>
	/// The HTTP Request Methods.
	/// </remarks>
	public enum RequestMethods
	{
		None = 0,
		Head = 1 << 0,
		Get = 1 << 1,
		Post = 1 << 2,
		Put = 1 << 3,
		Delete = 1 << 4,
		Trace = 1 << 5,
		Options = 1 << 6,
		Connect = 1 << 7,
		All = Head | Get | Post | Put | Delete | Trace | Options | Connect
	}
}
