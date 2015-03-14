#!/usr/bin/env python

from urlparse import urlparse
from hashlib import sha1
from base64 import b32encode
from optparse import OptionParser

class HSProtect(object):
    def generate_url(self, url, secret, domain, base):
        o  = urlparse(url)
        if o.query:
            _s = "%s%s?%s" % ( secret, o.path, o.query)
        else:
            _s = "%s%s" % ( secret, o.path)
        hasher = sha1()
        hasher.update(_s)
        raw_hash = hasher.digest()
        if base == 16:
            hashed = raw_hash.encode("hex")
        elif base == 32:
            hashed = b32encode(raw_hash)
        if o.query:
            return "http://%s/%s%s?%s" % ( domain, hashed, o.path, o.query)
        else:
            return "http://%s/%s%s" % ( domain, hashed, o.path)

if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option("-d", "--domain", dest="domain", help="Proxy Domain Name", metavar="HIDENSEQ_DOMAIN")
    parser.add_option("-u", "--url", dest="url", help="HTTP Listen Port", metavar="HIDENSEQ_URL")
    parser.add_option("-s", "--secret", dest="secret", help="Access Key Secret", metavar="HIDENSEQ_SECRET")
    parser.add_option('-b', '--base', dest='base', help='Access Key Encoding Base', metavar="HIDENSEQ_BASE")
    (options, args) = parser.parse_args()

    if options.secret is None:
        options.secret = "D93kf83hbs82md04ng64bf904ng74bd"

    if options.url is None:
        options.url = "http://cdn1.example.com/videos/test.mpg"

    if options.domain is None:
        options.domain = "c201.example.com"

    if options.base is None:
        options.base = 16
    options.base = int(options.base)

    acceptable_bases = [16, 32]
    if options.base not in acceptable_bases:
        print "Unacceptable Access Key Encoding Base %d" %(options.base)    

    hsp = HSProtect()
    print hsp.generate_url(options.url, options.secret, options.domain, options.base)

