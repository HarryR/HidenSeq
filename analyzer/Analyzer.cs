/* Copyright (C) 2012, Derp Ltd. All Rights Reserved. */

using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace HidenSeq
{
    public class HidenMatch {
        public uint offset1;
        public uint offset2;
        public string hash;

        public HidenMatch(uint offset1, uint offset2, string hash) {
            this.offset1 = offset1;
            this.offset2 = offset2;
            this.hash = hash;
        }
    }

    public class HidenSeqFile
    {
        public Dictionary<uint, HidenMatch> matches;
        public string filename;
        public long length;

        public HidenSeqFile(string filename)
        {
            this.filename = filename;
        }
    }
}
