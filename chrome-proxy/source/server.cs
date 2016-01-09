using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;

namespace Buttercup.Proxy {

	public class Server {

		private readonly HttpListener _listener = new HttpListener();
		private readonly Func<HttpListenerRequest, string> _responderMethod;

		// Adapted from the example here:
		//   https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server.aspx

		public Server(string[] prefixes, Func<HttpListenerRequest, string> method) {
			if (!HttpListener.IsSupported) {
				throw new NotSupportedException("HTTP listener not supported");
			}
 
			// URI prefixes are required, for example 
			// "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Length == 0) {
				throw new ArgumentException("Prefixes required");
			}
 
			// A responder method is required
			if (method == null) {
				throw new ArgumentException("No method specified");
			}
 
			foreach (string s in prefixes) {
				_listener.Prefixes.Add(s);
			}
 
			_responderMethod = method;
			_listener.Start();
		}

		public Server(Func<HttpListenerRequest, string> method, params string[] prefixes)
			: this(prefixes, method) { }

		public void run() {
			ThreadPool.QueueUserWorkItem((o) => {
				Console.WriteLine("Webserver running...");
				try {
					while (_listener.IsListening) {
						ThreadPool.QueueUserWorkItem((c) => {
							var ctx = c as HttpListenerContext;
							try {
								string rstr = _responderMethod(ctx.Request);
								byte[] buf = Encoding.UTF8.GetBytes(rstr);
								ctx.Response.ContentLength64 = buf.Length;
								ctx.Response.OutputStream.Write(buf, 0, buf.Length);
							} catch { } // suppress exceptions
							finally {
								// always close the stream
								ctx.Response.OutputStream.Close();
							}
						}, _listener.GetContext());
					}
				} catch { } // suppress exceptions
			});
		}
 
		public void stop() {
			_listener.Stop();
			_listener.Close();
		}

	}

}