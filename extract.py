#!/usr/bin/env python
##########################################################################
#
# HidenSeq - Video tag extractor
#
# Copyright 2012 Gabriel Roberts. All Rights Reserved.
# Copyright 2012 Derp Ltd. All Rights Reserved.
#
##########################################################################
"""Inserts [4b null + 4b offset + 20b SHA1 + 4b offset + 4b null]"""

import sys, re
from struct import unpack
from zlib import crc32

if len(sys.argv) < 2:
	print "Usage: extract.py <filename>"
	sys.exit(1)

filename = sys.argv[1]
print "Parsing '%s'..." % (filename,)

fp = open(filename,"rb")
contents = fp.read()
fp.close()

print "%.1f megabytes" % (len(contents) / 1024 / 1024,)

rx = re.compile(b"\0\0\0\0(.{4})(.{20})(.{4})\0\0\0\0", re.DOTALL)
for m in rx.findall(contents):
	offset_1 = unpack('I', m[0])[0]
	hashed = m[1]
	offset_2 = unpack('I', m[2])[0]
	if offset_1 == 0: continue
	if offset_2 != (crc32(hashed) & 0xffffffff): continue
	print "%d %s %d" % (offset_1, hashed.encode('hex'), offset_2)