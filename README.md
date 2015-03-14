HidenSeq
========
The 'Hidden Sequence' proxy embeds user-identifying markers into files when they are downloaded.
It enables us to play 'Hide N Seek' with the pirates.

The service is deployed as a HTTP proxy, intercepting all traffic between the sites end-users
and the customers download servers. It will only modify the contents of files when little to
no noticeable corruption can be incurred when inserting the markers.

It also protects downloads by requiring specially encoded URLs to be made for the proxy to accept
and forward requests.


Features
--------

 * Multiple domain names
 * URL tamper protection
 * Embed hidden markers in videos
 * Standalone and WSGI operation.
 * MySQL database log.


Deployment
----------
The `proxy.py` script reads `proxy.cfg` from the current working directory.
All configuration variables are explained in `example.cfg`

There are two modes of operation, 1) Built-In webserver and 2) WSGI Application.

1) The HTTP server details are specified in the `[*listen]` section of the config file, by default it will use Eventlet and Bottle.py.

2) The application will run under any standard WSGI container, like `uwsgi` and `mod_wsgi`. We run it with `uwsgi` using the commandline: `uwsgi --http 0.0.0.0:8080 -w proxy -p 4 -M -t 20 -m -T`

3) Optionally use the deployment.sh script as an exmaple of how to deploy multiple proxy servers behind an nginx load balancer.


Access URLs
-----------
All URLs are required to include an 'access key' prefix which protects URLs from manipulation by users.
It relys on a shared secret between the Web Application and Proxy Server to hash the parameters. A Python module to generate secret keys is included in `generate_url.py`.

```
access_key = sha1(secret ++ path ++ query_string)
url = "http://" ++ proxy_host ++ "/" ++ base16(access_key) ++ path ++ query_string
```

Examples:
```
$ python generate_url.py -u 'http://example.com/derp'
http://c201.example.com/ee20fb714256cf327f98dcf1f07f285e68355f5d/derp

$ python generate_url.py -u 'http://example.com/derp' -b32
http://c201.example.com/5YQPW4KCK3HTE74Y3TY7A7ZILZUDKX25/derp
```

Configuration
-------------

Sections that start with an asterisk (`*`) are internal configuration sections.

**Built-in HTTP Server**
```
[*listen]
host=0.0.0.0
port=8080
server=eventlet
debug=True
```

**Database Connection**
```
[*database]
host=localhost
port=3306
username=hidenseq
password=hidenseq
schema=hidenseq
```

**Tuning:**
The tunables can be adjusted globally in the `[DEFAULT]` section, or specified per mountpoint.
```
[DEFAULT]
# Speed limit, in bytes per second
speed_limit=1048576 # 1mb per second

# I/O copy size
pump_bytes=524288 # 512k

# Start embedding hidden tokens at X bytes into the file
stego_start_bytes=1048576 # 1mb

# Embed tokens every X bytes from there on
stego_interval_bytes=4194304 # 4mb

# Stop embedding at X percent, leaving the remaining untouched
stego_stop_pct=30
```

**Mount Points:**
A mount point redirects a request for a specific hostname to a remote URL.
For example accessing `http://derp.local/` could be redirected to `http://google.com/test/`:
```
[derp.local]
url=http://google.com/test/
secret=SECRETKEY12384ewhfweuihewg
```


**Query String Pruning:**
Items can be removed from the query string using a regular expression.
Example:
```
[cdn.local]
url=http://localhost/downloads/
secret=SECRETKEY12384ewhfweuihewg
rxremove=&derp=.*$
```
Will transform `?lol=123&derp=fwefw` to `?lol=123`


Cryptographic Strength
----------------------
Embedded markers use the SHA1 hashing algorithm to uniquely identify a single download of a specific
part of a file. They are embedded within the file in a way which is easy to search for and verify.

The markers alone don't contain any user identifiable information, and must be stored on the server
to associate a marker with a specific user or IP address.
