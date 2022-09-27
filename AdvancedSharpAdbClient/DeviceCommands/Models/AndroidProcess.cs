﻿//-----------------------------------------------------------------------
// <copyright file="AndroidProcess.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents a process running on an Android device.
    /// </summary>
    public class AndroidProcess
    {
        /// <summary>
        /// Gets or sets the Process ID number.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the parent Process ID number.
        /// </summary>
        public int ParentProcessId { get; set; }

        /// <summary>
        /// Gets or sets total VM size in bytes.
        /// </summary>
        public ulong VirtualSize { get; set; }

        /// <summary>
        /// Gets or sets the process resident set size.
        /// </summary>
        public int ResidentSetSize { get; set; }

        /// <summary>
        /// Gets or sets the memory address of the event the process is waiting for
        /// </summary>
        public ulong WChan { get; set; }

        /// <summary>
        /// Gets or sets the name of the process, including arguments, if any.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the state of the process.
        /// </summary>
        public AndroidProcessState State { get; set; }

        /// <summary>
        /// Creates a <see cref="AndroidProcess"/> from it <see cref="string"/> representation.
        /// </summary>
        /// <param name="line">A <see cref="string"/> which represents a <see cref="AndroidProcess"/>.</param>
        /// <param name="cmdLinePrefix">A value indicating whether the output of <c>/proc/{pid}/stat</c> is prefixed with <c>/proc/{pid}/cmdline</c> or not.
        /// Becuase <c>stat</c> does not contain the full process name, this can be useful.</param>
        /// <returns>The equivalent <see cref="AndroidProcess"/>.</returns>
        public static AndroidProcess Parse(string line, bool cmdLinePrefix = false)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            // See http://man7.org/linux/man-pages/man5/proc.5.html,
            // section /proc/[pid]/stat, for more information about the file format

            // Space delimited, so normally we would just do a string.split
            // The process name may contain spaces but is wrapped within parenteses, all other values (we know of) are
            // numeric.
            // So we parse the pid & process name manually, to account for this, and do the string.split afterwards :-)
            int processNameStart = line.IndexOf('(');
            int processNameEnd = line.LastIndexOf(')');

            int pid = 0;
            string comm = string.Empty;

            bool parsedCmdLinePrefix = false;

            if (cmdLinePrefix)
            {
                string[]? cmdLineParts = line.Substring(0, processNameStart).Split(new char[] { '\0' });

                if (cmdLineParts.Length <= 1)
                {
                    parsedCmdLinePrefix = false;
                }
                else
                {
                    pid = int.Parse(cmdLineParts[cmdLineParts.Length - 1]);
                    comm = cmdLineParts[0];

                    // All the other parts are the command line arguments, skip them.
                    parsedCmdLinePrefix = true;
                }
            }

            if (!parsedCmdLinePrefix)
            {
                pid = int.Parse(line.Substring(0, processNameStart));
                comm = line.Substring(processNameStart + 1, processNameEnd - processNameStart - 1);
            }

            string[]? parts = line.Substring(processNameEnd + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 35)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            // Only fields in Linux 2.1.10 and earlier are listed here,
            // additional fields exist in newer versions of linux.
            string? state = parts[0];
            int ppid = ParseInt(parts[1]);
            int pgrp = ParseInt(parts[2]);
            int session = ParseInt(parts[3]);
            int tty_nr = ParseInt(parts[4]);
            int tpgid = ParseInt(parts[5]);
            uint flags = ParseUInt(parts[6]);
            ulong minflt = ParseULong(parts[7]);
            ulong cminflt = ParseULong(parts[8]);
            ulong majflt = ParseULong(parts[9]);
            ulong cmajflt = ParseULong(parts[10]);
            ulong utime = ParseULong(parts[11]);
            ulong stime = ParseULong(parts[12]);
            long cutime = ParseLong(parts[13]);
            long cstime = ParseLong(parts[14]);
            long priority = ParseLong(parts[15]);
            long nice = ParseLong(parts[16]);
            long num_threads = ParseLong(parts[17]);
            long itrealvalue = ParseLong(parts[18]);
            ulong starttime = ParseULong(parts[19]);
            ulong vsize = ParseULong(parts[20]);
            int rss = int.Parse(parts[21]);
            ulong rsslim = ParseULong(parts[22]);
            ulong startcode = ParseULong(parts[23]);
            ulong endcode = ParseULong(parts[24]);
            ulong startstack = ParseULong(parts[25]);
            ulong kstkesp = ParseULong(parts[26]);
            ulong kstkeip = ParseULong(parts[27]);
            ulong signal = ParseULong(parts[28]);
            ulong blocked = ParseULong(parts[29]);
            ulong sigignore = ParseULong(parts[30]);
            ulong sigcatch = ParseULong(parts[31]);
            ulong wchan = ParseULong(parts[32]);
            ulong nswap = ParseULong(parts[33]);
            ulong cnswap = ParseULong(parts[34]);

            return new AndroidProcess()
            {
                Name = comm,
                ParentProcessId = ppid,
                State = (AndroidProcessState)Enum.Parse(typeof(AndroidProcessState), state, true),
                ProcessId = pid,
                ResidentSetSize = rss,
                VirtualSize = vsize,
                WChan = wchan
            };
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents this <see cref="AndroidProcess"/>,
        /// in the format of "<see cref="Name"/> (<see cref="ProcessId"/>)".
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="AndroidProcess"/>.</returns>
        public override string ToString()
        {
            return $"{Name} ({ProcessId})";
        }

        /// <summary>
        /// Gets the index of the first value of a set of values that is part of a list.
        /// </summary>
        /// <param name="list">The list in which to search for the value.</param>
        /// <param name="values">The values to search for.</param>
        /// <returns>The index of the first element in <paramref name="values"/> that is present in the list, or <c>-1</c>.</returns>
        private static int IndexOf(List<string> list, params string[] values)
        {
            foreach (string? value in values)
            {
                int index = list.IndexOf(value);

                if (index != -1)
                {
                    return index;
                }
            }

            return -1;
        }

        private static int ParseInt(string value)
        {
            return value == "-" ? 0 : int.Parse(value);
        }

        private static uint ParseUInt(string value)
        {
            return value == "-" ? 0 : uint.Parse(value);
        }

        private static long ParseLong(string value)
        {
            return value == "-" ? 0 : long.Parse(value);
        }

        private static ulong ParseULong(string value)
        {
            return value == "-" ? 0 : ulong.Parse(value);
        }
    }
}
