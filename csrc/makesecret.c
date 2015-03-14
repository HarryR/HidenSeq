#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <sys/time.h>

#include "sha1.h"
#include "base32.h"

int
main( int argc, char **argv ) {
	SHA1_INFO sha1_info;
	uint8_t randstr[20];
	struct timeval tv;
	gettimeofday(&tv,NULL);
	unsigned int randseed = tv.tv_sec + tv.tv_usec;
	uint8_t output[33];
	size_t i;
	for(i = 0; i < sizeof(randstr); i++) {
		randstr[i] = (uint8_t)(rand_r(&randseed) % 0xFF);
	}

	sha1_init(&sha1_info);
	sha1_update(&sha1_info, randstr, sizeof(randstr));
	sha1_final(&sha1_info, randstr);

	base32_encode(randstr, sizeof(randstr), output, sizeof(output));
	output[sizeof(output)-1] = 0;
	printf("%s\n", output);

	return 0;
}