﻿using System;
using System.Net;
using System.Linq;
using Flow.Responders;
using System.Text;
using System.IO;
using System.Threading;
using Flow.Handlers;
using System.Collections.Generic;

namespace Flow.FileServer
{
	static class Program
	{
		static void Main(string[] args)
		{
			pads = new Dictionary<int,string>();
			var router = new Router();
			router
				.AddRest<string, int>(getNotepadItem)
				.AddRest<string, int>(putNotepadItem)
				.AddRest<string>(getNotepad)
				.AddRest<string>(postNotepad);
			router.Start();
		}

		[RestMethod(Method = RequestMethods.Get, Pattern = "/notepad/[0-9]+")]
		static void getNotepadItem(Request request, string beNotepad, int nr)
		{
			string text;
			int status = 200;
			lock (pads) {
				if (!pads.TryGetValue(nr, out text)) {
					status = 404;
				}
			}
			if (status == 404) {
				text = "";
			}
			request
				.Respond(status)
				.StreamText(text);
		}

		[RestMethod(Method = RequestMethods.Put, Pattern = "/notepad/[0-9]+")]
		static void putNotepadItem(Request request, string beNotepad, int nr)
		{
			var text = ((TextReader)new StreamReader(request.Body)).ReadToEnd();
			bool error = false;
			lock (pads) {
				if (pads.ContainsKey(nr)) {
					pads[nr] = text;
				} else {
					error = true;
				}
			}
			if (error) {
				request.Respond(404).Finish().Dispose();
			}

			request.Respond(200).Finish().Dispose();
		}

		[RestMethod(Method = RequestMethods.Get, Pattern = "/notepad/")]
		static void getNotepad(Request request, string beNotepad)
		{
			string text;
			int length;
			lock (pads) {
				text =
					pads
					.Select(pair => string.Format(@"<li><a href=""/notepad/{0}"">{1}</a></li>", pair.Key, pair.Value))
					.Aggregate(new StringBuilder(), (builder, sect) => { builder.AppendLine(sect); return builder; })
					.ToString();
				length = pads.Count;
			}

			request
				.Respond(200)
				.StreamText(string.Format("<html><head><title>{0} found</title></head><body><ul>{1}</ul></body></html>", length, text), "text/html");
		}

		[RestMethod(Method = RequestMethods.Post, Pattern = "/notepad/")]
		static void postNotepad(Request request, string beNotepad)
		{
			var text = ((TextReader)new StreamReader(request.Body)).ReadToEnd();
			lock (pads) {
				var key = pads.Count;
				pads.Add(key, text);
			}

			request.Respond(200).Finish().Dispose();
		}

		static Dictionary<int, string> pads;
	}
}
