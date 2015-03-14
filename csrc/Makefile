CFLAGS=-Wall -Wextra -O0
OBJS_COMMON=base32.o sha1.o url_parser.o
OBJS_MAKEURL=makeurl.o $(OBJS_COMMON)
OBJS_MAKESECRET=makesecret.o $(OBJS_COMMON)

all: hs-makeurl hs-makesecret

hs-makeurl: $(OBJS_MAKEURL)
	$(CC) -o $@ $(CFLAGS) $+ $(LIBS)

hs-makesecret: $(OBJS_MAKESECRET)
	$(CC) -o $@ $(CFLAGS) $+ $(LIBS)

clean:
	-rm *.o hs-makeurl hs-makesecret