/*   Copyright 2009 Dykam (kramieb@gmail.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
