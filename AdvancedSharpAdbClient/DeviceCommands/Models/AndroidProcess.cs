// <copyright file="AndroidProcess.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents a process running on an Android device.
    /// </summary>
    public class AndroidProcess
    {
        /// <summary>
        /// Gets or sets the state of the process.
        /// </summary>
        public AndroidProcessState State { get; set; }

        /// <summary>
        /// Gets or sets the name of the process, including arguments, if any.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent Process ID number.
        /// </summary>
        public int ParentProcessId { get; set; }

        /// <summary>
        /// Gets or sets the Process Group ID number.
        /// </summary>
        public int ProcessGroupId { get; set; }

        /// <summary>
        /// Gets or sets the session ID of the process number.
        /// </summary>
        public int SessionID { get; set; }

        /// <summary>
        /// Gets or sets the Process ID number.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the controlling terminal of the process.
        /// </summary>
        /// <value>The minor device number is contained in the combination of bits
        /// <see langword="31"/> to <see langword="20"/> and <see langword="7"/> to <see langword="0"/>;
        /// the major device number is in bits <see langword="15"/> to <see langword="8"/>.</value>
        public int TTYNumber { get; set; }

        /// <summary>
        /// Gets or sets the ID of the foreground process group of the controlling terminal of the process.
        /// </summary>
        public int TopProcessGroupId { get; set; }

        /// <summary>
        /// Gets or sets The kernel flags word of the process. For bit meanings, see the <c>PF_*</c> defines
        /// in the Linux kernel source file <c>include/linux/sched.h</c>. Details depend on the kernel version.
        /// </summary>
        public PerProcessFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the number of minor faults the process has made
        /// which have not required loading a memory page from disk.
        /// </summary>
        public ulong MinorFaults { get; set; }

        /// <summary>
        /// Gets or sets the number of minor faults that the process's waited-for children have made.
        /// </summary>
        public ulong ChildMinorFaults { get; set; }

        /// <summary>
        /// Gets or sets the number of major faults the process has made
        /// which have required loading a memory page from disk.
        /// </summary>
        public ulong MajorFaults { get; set; }

        /// <summary>
        /// Gets or sets the number of major faults that the process's waited-for children have made.
        /// </summary>
        public ulong ChildMajorFaults { get; set; }

        /// <summary>
        /// Gets or sets the amount of time that this process has been scheduled in user mode,
        /// measured in clock ticks (divide by <c>sysconf(_SC_CLK_TCK)</c>). This includes guest time,
        /// guest_time (time spent running a virtual CPU, see below), so that applications
        /// that are not aware of the guest time field do not lose that time from their calculations.
        /// </summary>
        public ulong UserScheduledTime { get; set; }

        /// <summary>
        /// Gets or sets the amount of time that this process has been scheduled in kernel mode,
        /// measured in clock ticks (divide by <c>sysconf(_SC_CLK_TCK)</c>).
        /// </summary>
        public ulong ScheduledTime { get; set; }

        /// <summary>
        /// Gets or sets the amount of time that this process's waited-for children have been scheduled in user mode,
        /// measured in clock ticks (divide by <c>sysconf(_SC_CLK_TCK)</c>). (See also <c>times(2)</c>.)
        /// This includes guest time, cguest_time (time spent running a virtual CPU, see below).
        /// </summary>
        public long ChildUserScheduledTime { get; set; }

        /// <summary>
        /// Gets or sets the Amount of time that this process's waited-for children have been scheduled in kernel mode,
        /// measured in clock ticks (divide by <c>sysconf(_SC_CLK_TCK)</c>).
        /// </summary>
        public long ChildScheduledTime { get; set; }

        /// <summary>
        /// Gets or sets the value for processes running a real-time scheduling policy
        /// (policy below; see sched_setscheduler(2)), this is the negated scheduling priority,
        /// minus one; that is, a number in the range <see langword="-2"/> to <see langword="-100"/>,
        /// corresponding to real-time priorities <see langword="1"/> to <see langword="99"/>.
        /// For processes running under a non-real-time scheduling policy, this is the raw nice
        /// value (<c>setpriority(2)</c>) as represented in the kernel. The kernel stores nice values as numbers
        /// in the range <see langword="0"/> (high) to <see langword="39"/> (low),
        /// corresponding to the user-visible nice range of <see langword="-20"/> to <see langword="19"/>.
        /// </summary>
        public long Priority { get; set; }

        /// <summary>
        /// Gets or sets the nice value (see <c>setpriority(2)</c>), a value in the range
        /// <see langword="19"/> (low priority) to <see langword="-20"/> (high priority).
        /// </summary>
        public long Nice { get; set; }

        /// <summary>
        /// Gets or sets the number of threads in this process (since Linux 2.6).
        /// </summary>
        /// <remarks>Before kernel 2.6, this field was hard coded to 0 as a placeholder for an earlier removed field.</remarks>
        public long ThreadsNumber { get; set; }

        /// <summary>
        /// Gets or sets the time in jiffies before the next <c>SIGALRM</c> is sent to the process due to an interval timer.
        /// </summary>
        /// <remarks>Since kernel 2.6.17, this field is no longer maintained, and is hard coded as 0.</remarks>
        public long Interval { get; set; }

        /// <summary>
        /// Gets or sets The time the process started after system boot. In kernels before Linux 2.6,
        /// this value was expressed in jiffies.Since Linux 2.6, the value is expressed in clock ticks
        /// (divide by <c>sysconf(_SC_CLK_TCK)</c>).
        /// </summary>
        /// <remarks>The format for this field was %lu before Linux 2.6.</remarks>
        public ulong StartTime { get; set; }

        /// <summary>
        /// Gets or sets total virtual memory size in bytes.
        /// </summary>
        public ulong VirtualSize { get; set; }

        /// <summary>
        /// Gets or sets the process resident set size.
        /// </summary>
        /// <value>Number of pages the process has in real memory.
        /// This is just the pages which count toward text, data, or stack space.
        /// This does not include pages which have not been demand-loaded in, or which are swapped out.
        /// This value is inaccurate; see <c>/proc/[pid]/statm</c> below.</value>
        public int ResidentSetSize { get; set; }

        /// <summary>
        /// Gets or sets current soft limit in bytes on the rss of the process;
        /// See the description of RLIMIT_RSS in <c>getrlimit(2)</c>.
        /// </summary>
        public ulong ResidentSetSizeLimit { get; set; }

        /// <summary>
        /// Gets or sets the address above which program text can run.
        /// </summary>
        public ulong StartCode { get; set; }

        /// <summary>
        /// Gets or sets the address below which program text can run.
        /// </summary>
        public ulong EndCode { get; set; }

        /// <summary>
        /// Gets or sets the address of the start (i.e., bottom) of the stack.
        /// </summary>
        public ulong StartStack { get; set; }

        /// <summary>
        /// Gets or sets the current value of ESP (stack pointer), as found in the kernel stack page for the process.
        /// </summary>
        public ulong ESP { get; set; }

        /// <summary>
        /// Gets or sets the current EIP (instruction pointer).
        /// </summary>
        public ulong EIP { get; set; }

        /// <summary>
        /// Gets or sets the bitmap of pending signals, displayed as a decimal number. Obsolete,
        /// because it does not provide information on real-time signals; Use <c>/proc/[pid]/status</c> instead.
        /// </summary>
        public ulong Signal { get; set; }

        /// <summary>
        /// Gets or sets the bitmap of blocked signals, displayed as a decimal number. Obsolete,
        /// because it does not provide information on real-time signals; Use <c>/proc/[pid]/status</c> instead.
        /// </summary>
        public ulong Blocked { get; set; }

        /// <summary>
        /// Gets or sets the bitmap of ignored signals, displayed as a decimal number. Obsolete,
        /// because it does not provide information on real-time signals; Use <c>/proc/[pid]/status</c> instead.
        /// </summary>
        public ulong IgnoredSignals { get; set; }

        /// <summary>
        /// Gets or sets the bitmap of caught signals, displayed as a decimal number. Obsolete,
        /// because it does not provide information on real-time signals; Use <c>/proc/[pid]/status</c> instead.
        /// </summary>
        public ulong CaughtSignals { get; set; }

        /// <summary>
        /// Gets or sets the memory address of the event the process is waiting for.
        /// </summary>
        /// <value>This is the "channel" in which the process is waiting.
        /// It is the address of a location in the kernel where the process is sleeping.
        /// The corresponding symbolic name can be found in <c>/proc/[pid]/wchan</c>.</value>
        public ulong WChan { get; set; }

        /// <summary>
        /// Gets or sets the number of pages swapped (not maintained).
        /// </summary>
        public ulong SwappedPagesNumber { get; set; }

        /// <summary>
        /// Gets or sets the cumulative number of pages swapped for child processes (not maintained).
        /// </summary>
        public ulong CumulativeSwappedPagesNumber { get; set; }

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
                string[] cmdLineParts = line.Substring(0, processNameStart).Split(new char[] { '\0' });

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

            string[] parts = line.Substring(processNameEnd + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 35)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            // Only fields in Linux 2.1.10 and earlier are listed here,
            // additional fields exist in newer versions of linux.
            string state = parts[0];
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

            return new AndroidProcess
            {
                State = (AndroidProcessState)Enum.Parse(typeof(AndroidProcessState), state, true),
                Name = comm,
                ParentProcessId = ppid,
                ProcessGroupId = pgrp,
                SessionID = session,
                TTYNumber = tty_nr,
                TopProcessGroupId = tpgid,
                Flags = (PerProcessFlags)flags,
                MinorFaults = minflt,
                ChildMinorFaults = cminflt,
                MajorFaults = majflt,
                ChildMajorFaults = cmajflt,
                UserScheduledTime = utime,
                ScheduledTime = stime,
                ChildUserScheduledTime = cutime,
                ChildScheduledTime = cstime,
                Priority = priority,
                Nice = nice,
                ThreadsNumber = num_threads,
                Interval = itrealvalue,
                StartTime = starttime,
                ProcessId = pid,
                VirtualSize = vsize,
                ResidentSetSize = rss,
                ResidentSetSizeLimit = rsslim,
                StartCode = startcode,
                EndCode = endcode,
                StartStack = startstack,
                ESP = kstkesp,
                EIP = kstkeip,
                Signal = signal,
                Blocked = blocked,
                IgnoredSignals = sigignore,
                CaughtSignals = sigcatch,
                WChan = wchan,
                SwappedPagesNumber = nswap,
                CumulativeSwappedPagesNumber = cnswap
            };
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents this <see cref="AndroidProcess"/>,
        /// in the format of "<see cref="Name"/> (<see cref="ProcessId"/>)".
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="AndroidProcess"/>.</returns>
        public override string ToString() => $"{Name} ({ProcessId})";

        private static int ParseInt(string value) => value == "-" ? 0 : int.Parse(value);

        private static uint ParseUInt(string value) => value == "-" ? 0 : uint.Parse(value);

        private static long ParseLong(string value) => value == "-" ? 0 : long.Parse(value);

        private static ulong ParseULong(string value) => value == "-" ? 0 : ulong.Parse(value);
    }
}
