#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>
#include <string.h>

#include "sha1.h"
#include "url_parser.h"
#include "base32.h"

static void
usage (void) {
	fprintf(stderr, "Usage: hs-makeurl [options]\n");
	fprintf(stderr, " -d <domain> : Proxy Domain Name\n");
	fprintf(stderr, " -u <url>    : URL of protected content\n");
	fprintf(stderr, " -s <secret> : Secret Access Key\n");
	fprintf(stderr, "\n");
}

static char*
hidenseq_makeurl (const char *url, const char *secret, const char *domain) {
	SHA1_INFO sha1_info;
	char *hash_str = NULL;
	uint8_t hash_digest[20];
	uint8_t secret_b32[33];
	char *output_url = NULL;
	struct parsed_url *parsed = parse_url(url);
	if( parsed == NULL ) {
		return NULL;
	}

	if( parsed->scheme == NULL || parsed->host == NULL || parsed->path == NULL ) {
		parsed_url_free(parsed);		
		return NULL;
	}

	if( parsed->query ) {
		asprintf(&hash_str, "%s/%s?%s", secret, parsed->path, parsed->query);
	}
	else {
		asprintf(&hash_str, "%s/%s", secret, parsed->path);
	}

	sha1_init(&sha1_info);
	sha1_update(&sha1_info, (const uint8_t*)&hash_str[0], strlen(hash_str));
	sha1_final(&sha1_info, hash_digest);
	free(hash_str);
	hash_str = NULL;

	base32_encode(&hash_digest[0], sizeof(hash_digest), &secret_b32[0], sizeof(secret_b32));
	secret_b32[ sizeof(secret_b32)-1 ] = 0;

	if( parsed->query ) {
		asprintf(&output_url, "http://%s/%s/%s?%s", domain, secret_b32, parsed->path, parsed->query);
	}
	else {
		asprintf(&output_url, "http://%s/%s/%s", domain, secret_b32, parsed->path);	
	}

	parsed_url_free(parsed);
	parsed = NULL;
	return output_url;
}

int
main (int argc, char **argv) {
	int ch;
	const char *domain = "c201.example.com";
	const char *url = "http://cdn1.example.com/videos/test.mpg";
	const char *secret = "D93kf83hbs82md04ng64bf904ng74bd";
	char *output_url = NULL;

	static struct option longopts[] = {
		{ "domain", required_argument, NULL, 'd' },
		{ "url",    required_argument, NULL, 'u' },
		{ "secret", required_argument, NULL, 's' }
	};

	while ( (ch = getopt_long(argc, argv, "d:u:s", longopts, NULL)) != -1 ) {
		switch (ch) {
		case 'd':
			domain = optarg;
			break;

		case 'u':
			url = optarg;
			break;

		case 's':
			secret = optarg;
			break;

		default:
			usage();
			exit(1);
		}
	}
	argc -= optind;
	argv += optind;

	output_url = hidenseq_makeurl(url, secret, domain);
	if( output_url ) {
		printf("%s\n", output_url);
		free(output_url);
	}
	return 0;
}
