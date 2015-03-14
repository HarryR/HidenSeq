#!/usr/bin/env python
"""
HidenSeq - Video download tagger

Copyright 2012 Gabriel Roberts. All Rights Reserved.
Copyright 2012 Derp Ltd. All Rights Reserved.
"""



import bottle, ConfigParser, urllib2, re, MySQLdb, eventlet, sys
import time, traceback
from hashlib import sha1
from eventlet.db_pool import ConnectionPool
from bottle import request, HTTPError, HTTPResponse
from struct import pack
from zlib import crc32
from optparse import OptionParser
from base64 import b32encode
from urlparse import urlparse

eventlet.monkey_patch()



##########################################################################



# Anonymous looking error codes :)
bottle.ERROR_PAGE_TEMPLATE = """
<!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML 2.0//EN">
<html><head>
<title>Error {{e.status}}</title>
</head><body>
<h1>Error {{e.status}}</h1>
</body></html>
"""



##########################################################################



def parse_content_range(range_str):
    """Parses a 'Content-Range' response header
       Returns (RangeStart,RangeEnd,TotalBytes)
    """
    match = re.match('bytes ([0-9]+)-([0-9]+)/([0-9]+)', range_str)
    if not match:
        return None
    if match:
        return (int(match.group(1)), int(match.group(2)), int(match.group(3)))

class Download:
    """Manages an in-progress download"""
    def __init__(self, environ, meta, dbpool):
        self.id = None
        self.dbpool = dbpool
        self.environ = environ
        self.meta = meta
        self.mime_type = meta.dict['content-type']
        if 'content-length' in meta.dict:
            self.bytes = int(meta.dict['content-length'])
            if 'content-range' in meta.dict:
                server_range = parse_content_range(meta.dict['content-range'])
                self.offset = int(server_range[0])
                self.total_bytes = int(server_range[2])
            else:
                self.offset = 0
                self.total_bytes = self.bytes
        else:
            # When no content-length in Response we cannot do any logging
            # And are basically a limited pass-through proxy.
            self.bytes = None
            self.offset = None
            self.total_bytes = None            
    
    def is_partial(self):
        """Is the Download a partial range or subsection of file?"""
        if self.offset is not None and self.offset > 0:
            return True
        if self.bytes is not None and self.bytes != self.total_bytes:
            return True
        return False

    def finish(self):        
        """Records that a download has finished"""
        if self.bytes is None:
            return False
        conn = self.dbpool.get()
        try:
            sql = """
            UPDATE downloads SET is_finished = 1, finish_time = NOW() WHERE id = %s
            """
            params = [self.id]
            conn.cursor().execute(sql, params)
            conn.commit()
        finally:
            self.dbpool.put(conn)
        return True

    def start(self):
        """Records request information for a started download"""
        if self.bytes is None:
            return False
        download_id = None
        conn = self.dbpool.get()
        try:
            sql = """
            INSERT INTO downloads
            (path_info, remote_addr, server_addr, user_agent, mime_type, file_offset, chunk_size, file_size, query_string, start_time)
     VALUES (%s,        %s,          %s,          %s,         %s,        %s,          %s,         %s,        %s,           NOW())
            """
            if "HTTP_X_FORWARDED_FOR" in self.environ:
                client_ip = self.environ["HTTP_X_FORWARDED_FOR"]
            else:
                client_ip = self.environ["REMOTE_ADDR"]
            params = [self.environ['PATH_INFO'],        # path_info
                      client_ip,                        # remote_addr
                      self.environ['HTTP_HOST'],        # server_addr
                      self.environ['HTTP_USER_AGENT'],  # user_agent
                      self.mime_type,                   # mime_type
                      self.offset,                      # file_offset
                      self.bytes,                       # chunk_size
                      self.total_bytes,                 # file_size
                      self.environ.get("QUERY_STRING")] # query_string                 
            cursor = conn.cursor()
            cursor.execute(sql, params)

            cursor.execute('SELECT last_insert_id()')
            download_id = cursor.fetchone()[0]
            conn.commit()
        finally:
            self.dbpool.put(conn)
        self.id = download_id
        return download_id is not None

    def log_chunk(self, offset, hash_str):
        """Logs that a hash has been embedded at `offset` within the file"""
        if self.bytes is None:
            return False
        conn = self.dbpool.get()
        try:
            sql = """
            INSERT INTO download_chunks
            (download_id, offset, hash_str, stamp)
     VALUES (%s,          %s,     %s,       NOW())
            """
            params = [self.id,
                      offset,
                      hash_str.encode('hex')]
            conn.cursor().execute(sql, params)
            conn.commit()
        finally:
            self.dbpool.put(conn)
        return True



##########################################################################


def speed_limiter(handle, speed_limit):
    """Restrict speed to X bytes per second"""
    start_time = time.time()
    total_bytes = 0
    for part in handle:
        current_time = time.time()
        speed = total_bytes / (current_time - start_time)
        if speed > speed_limit:
            delay = speed / speed_limit
            time.sleep(delay)
        total_bytes += len(part)
        yield part


class ProxyHandler:
    """Proxies requests and embeds secret codes inside Video files"""
    def __init__(self, config):
        self.config = config

    def hostname(self):
        """Current requests hostname"""
        hostname = request.environ["HTTP_HOST"].lower()
        if ":" in hostname:
            hostname = hostname.split(":")[0]
        return hostname

    def param(self, name):
        """Retrieve a parameter for the current HOST"""
        hostname = self.hostname()
        if hostname not in self.config:
            return None
        if name in self.config[hostname]:
            return self.config[hostname][name]
        return None

    def stego_part(self, download, part, offset, log_chunk):        
        """Inserts [4b null + 4b offset + 20b SHA1 + 4b offset + 4b null]"""
        if len(part) < (1024 * 128):
            return part
        replace_sz = 36
        after = part[replace_sz:]
        replaced = part[:replace_sz]

        hasher = sha1()
        hasher.update(replaced)
        hasher.update(request.environ['REMOTE_ADDR'])
        hasher.update(request.environ['PATH_INFO'])
        
        hashed = hasher.digest()
        offset_1 = offset
        offset_2 = crc32(hashed) & 0xffffffff

        if log_chunk:
            download.log_chunk(offset_1, hashed)

        hidden = pack('II', 0, offset_1) + hashed + pack('II', offset_2, 0)
        return hidden + after

    def passthru(self, file_handle):
        """Runs the I/O pump, without knowing the size of the file"""
        cont = True
        maxread = int(self.param('pump_bytes'))
        while cont:
            part = file_handle.read(maxread)
            if not part:
                cont = False
                break
            if len(part) != maxread:
                cont = False
            yield part

    def pump(self, file_handle, download):
        """Runs the I/O pump copying data from Server to Client"""        
        maxread = int(self.param('pump_bytes'))
        sent = 0
        remaining = download.bytes
        inject = download.mime_type.split("/")[0] == "video"
        if inject and "content-encoding" in download.meta.dict:
            inject = False

        stego_start_bytes = int(self.param('stego_start_bytes'))
        stego_interval_bytes = int(self.param('stego_interval_bytes'))
        stego_stop_pct = int(self.param('stego_stop_pct'))

        next_insert = stego_start_bytes
        last_insert = (download.total_bytes / 100) * stego_stop_pct

        has_db_con = True
        try:
            download.start()
        except:
            has_db_con = False

        try:
            while remaining >= 0:
                part = file_handle.read(min(remaining, maxread))
                if not part:
                    break
                if inject:
                    if (download.offset+sent) <= last_insert and (download.offset+sent) >= next_insert:
                        next_insert += stego_interval_bytes
                        part = self.stego_part(download, part, sent, has_db_con)
                remaining -= len(part)
                sent += len(part)
                yield part
        except e:
            traceback.print_exc()
        finally:            
            if has_db_con:
                download.finish()

    def build_url(self, filename):
        """Constructs URL to query based on Request"""        
        # sanitize url to prevent multiple slashes        
        remote_url = urlparse(self.param('url'))
        url = "%s://%s%s" % ( remote_url.scheme, remote_url.netloc, filename)
        if 'QUERY_STRING' in request.environ:
            query_string = request.environ['QUERY_STRING']
            rxremove = self.param('rxremove')
            if rxremove is not None:
                query_string = re.sub(rxremove, '', query_string)
            url = url + '?' + query_string
        return url

    def validate_meta(self, meta):
        """Checks if Request can be handled"""
        return 'content-type' in meta.dict

    def response_headers(self, meta):
        """Return all headers to be sent to client in the Response"""
        header = {}
        for k in meta.dict:
            header[k.title()] = meta.dict[k]
        if 'Content-Length' in header:
            header['Content-Length'] = int(header['Content-Length'])
        header['Cache-Control'] = 'private'
        return header

    def get_client_headers(self):
        """Request headers to proxy from client to server"""
        headers = {}
        copy_headers = {'HTTP_RANGE': 'Range',
                        'HTTP_ACCEPT': 'Accept',
                        'HTTP_CACHE_CONTROL': 'Cache-Control',
                        'HTTP_ACCEPT_LANGUAGE': 'Accept-Language',
                        'HTTP_USER_AGENT': 'User-Agent',
                        'HTTP_ACCEPT_CHARSET': 'Accept-Charset',
                        'HTTP_ACCEPT_ENCODING': 'Accept-Encoding'}
        for env_key in copy_headers:
            if env_key in request.environ:
                headers[copy_headers[env_key]] = request.environ.get(env_key)
        return headers    

    def validate_access_key(self, key, filename):
        """Checks if the given access key is valid for the current request"""
        secret = self.param("secret")
        plaintext_key = secret + filename
        if "QUERY_STRING" in request.environ:
            plaintext_key += "?" + request.environ["QUERY_STRING"]
        hasher = sha1()
        hasher.update(plaintext_key)
        hashed_key = hasher.digest()

        if len(key) == 40:
            hashed = hashed_key.encode("hex")
        else:
            hashed = b32encode(hashed_key)
        return hashed == key

    def handle(self, method, access_key, filename):
        """Handles a request"""
        if filename[0] != '/':
            filename = '/' + filename
        if self.hostname() not in self.config:
            return HTTPError(400, "Hostname Unknown")

        if not self.validate_access_key(access_key, filename):
            return HTTPError(403, "Access Denied")

        url = self.build_url(filename)
        download = None
        try:
            if method != 'GET' and method != 'HEAD':
                return HTTPError(503, "Not Implemented")                

            client_headers = self.get_client_headers()
            req = urllib2.Request(url, headers = client_headers)
            req.get_method = lambda : method

            remote = urllib2.urlopen(req)
            meta = remote.info()
            if not self.validate_meta(meta):
                return HTTPError(503, "Not Implemented")

            status = 200
            if method == 'GET':
                download = Download(request.environ, meta, self.config['db'])
                if download.bytes is None:
                    body = self.passthru(remote)
                else:
                    body = self.pump(remote, download)

                speed_limit = int(self.param('speed_limit'))
                if speed_limit:
                    body = speed_limiter(body, speed_limit)

                if download.is_partial():
                    status = 206
            else:
                body = ''

            header = self.response_headers(meta)
            return HTTPResponse(body, header=header, status=status)
        except urllib2.HTTPError, fault:
            meta = fault.info()
            header = self.response_headers(meta)
            return HTTPResponse(fault.read(), header=header, status=fault.code)
        except e:
            traceback.print_exc()

    def handle_head(self, access_key, filename):
        """Handles a HEAD request"""
        return self.handle('HEAD', access_key, filename)

    def handle_get(self, access_key, filename):
        """Handles a GET request"""
        return self.handle('GET', access_key, filename)



##########################################################################



class HidenSeq:
    """HidenSeq proxy server main class"""
    def __init__(self, config_file = 'proxy.cfg'):
        defaults = {'pump_bytes': str(1024 * 32),
                    'stego_start_bytes': str(1024 * 1024),
                    'stego_interval_bytes': str(1024 * 1024 * 4),
                    'stego_stop_pct': str(30),
                    'speed_limit': str(1024 * 1024)}

        config = ConfigParser.ConfigParser(defaults)
        config.read(config_file)
        self.config = config
        self.database = self.init_db()
        self.app = self.init_app()

    def init_db(self):
        """Setup Database Connection Pool"""
        db_host = self.config.get('*database', 'host')
        db_port = self.config.getint('*database', 'port')
        db_user = self.config.get('*database', 'username')
        db_pass = self.config.get('*database', 'password')
        db_schema = self.config.get('*database', 'schema')

        return ConnectionPool(MySQLdb,
            host=db_host,
            port=db_port,
            user=db_user,
            passwd=db_pass,
            db=db_schema)

    def init_app(self):
        """Setup application"""
        app = bottle.Bottle()
        config = {}
        for section in self.config.sections():
            if section[0] == '*':
                continue            
            config[section] = {}
            for key, val in self.config.items(section):
                config[section][key] = val

            if "secret" not in config[section]:
                print "Error: [" + section + "] must have `secret` access key"
                sys.exit(1)

            # Ensure pump bytes is reasonable in comparison to speed limit
            if config[section]['speed_limit'] != '0':
                if int(config[section]['pump_bytes']) > int(config[section]['speed_limit']):
                    config[section]['pump_bytes'] = int(config[section]['speed_limit'])

        mountpoint = '/<access_key>/<filename:path>'
        config['db'] = self.database
        handler = ProxyHandler(config)
        app.route(mountpoint, 'GET', handler.handle_get)
        app.route(mountpoint, 'HEAD', handler.handle_head)
        return app

    def run(self, **kwargs):
        """Starts Bottle.py Server and waits forever"""
        cfg_host = self.config.get('*listen','host')
        cfg_port = self.config.getint('*listen','port')
        cfg_server = self.config.get('*listen','server')
        cfg_debug = self.config.getboolean('*listen','debug')

        if "port" in kwargs and kwargs["port"] is not None:
            cfg_port = int(kwargs["port"])

        bottle.debug(cfg_debug)
        bottle.run(self.app, server=cfg_server, host=cfg_host, port=cfg_port)



##########################################################################



if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option("-p", "--port", dest="port", help="HTTP Listen Port", metavar="HTTP_PORT")
    parser.add_option("-c", "--cfg", dest="config", help="Config Filename", metavar="CONFIG_FILE")
    (options, args) = parser.parse_args()
    if options.config is None:
        options.config = "proxy.cfg"
        
    SERVER = HidenSeq(options.config)
    SERVER.run(**options.__dict__)
else:
    SERVER = HidenSeq()
    application = SERVER.app
    
