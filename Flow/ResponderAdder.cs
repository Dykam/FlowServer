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
	public partial class Router
	{
		/// <remarks>
		/// Adds responders to the router.
		/// </remarks>
		public class ResponderAdder
		{
			LinkedList<Predicate<Request>> conditions;
			Router router;

			bool used;

			internal ResponderAdder(Router router, Predicate<Request> condition)
			{
				used = false;

				conditions = new LinkedList<Predicate<Request>>();
				conditions.AddLast(condition);

				this.router = router;
			}

			/// <summary>
			/// Adds another condition to which the responder may respond.
			/// </summary>
			/// <param name="condition">
			/// A <see cref="Predicate"/> which determines if the responder should respond to this request.
			/// </param>
			/// <returns>
			/// A <see cref="ResponderAdder"/> to add the responder or another condition.
			/// </returns>
			public ResponderAdder OrIf(Predicate<Request> condition)
			{
				usedChecker();
				conditions.AddLast(condition);
				return this;
			}

			/// <summary>
			/// Adds the responder to the router.
			/// </summary>
			/// <param name="responder">
			/// A <see cref="Action"/> to respond with.
			/// </param>
			/// <returns>
			/// A <see cref="Router"/> to which more listeners can be added.
			/// </returns>
			public Router RespondWith(Action<Request> responder)
			{
				usedChecker();

				used = true;
				router.processors.Add(new Responder(responder, conditions));

				return router;
			}

			void usedChecker()
			{
				if (used) throw new InvalidOperationException("You can only add a responder once.");
			}
		}
	}
}
