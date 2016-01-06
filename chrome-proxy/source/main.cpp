#include <iostream>
 
extern "C" {
	#include "../deps/mongoose-6.1/mongoose.h"
}

static const char *s_http_port = "8000";
static struct mg_serve_http_opts s_http_server_opts;

static void ev_handler(struct mg_connection *nc, int ev, void *p) {
	if (ev == MG_EV_HTTP_REQUEST) {
		mg_serve_http(nc, (struct http_message *) p, s_http_server_opts);
	}
}

int main() {
	struct mg_mgr mgr;
	struct mg_connection *nc;

	mg_mgr_init(&mgr, NULL);
	nc = mg_bind(&mgr, s_http_port, ev_handler);

	mg_set_protocol_http_websocket(nc);
	//s_http_server_opts.document_root = ".";  // Serve current directory
	//s_http_server_opts.dav_document_root = ".";  // Allow access via WebDav
	//s_http_server_opts.enable_directory_listing = "yes";

	printf("Starting web server on port %s\n", s_http_port);
	for (;;) {
		mg_mgr_poll(&mgr, 1000);
	}
	mg_mgr_free(&mgr);

	std::cout << "Hello World!!!!" << std::endl;
	return 0;
}