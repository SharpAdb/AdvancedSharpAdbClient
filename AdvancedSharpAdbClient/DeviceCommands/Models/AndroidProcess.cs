// <copyright file="AndroidProcess.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
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
        /// Gets or sets the signal to be sent to parent when we die.
        /// </summary>
        public int ExitSignal { get; set; }

        /// <summary>
        /// Gets or sets the CPU number last executed on.
        /// </summary>
        public int Processor { get; set; }

        /// <summary>
        /// Gets or sets the real-time scheduling priority, a number in the range 1 to 99 for processes scheduled
        /// under a real-time policy, or 0, for non-real-time processes (see <c>sched_setscheduler(2)</c>).
        /// </summary>
        public uint RealTimePriority { get; set; }

        /// <summary>
        /// Gets or sets the scheduling policy (see <c>sched_setscheduler(2)</c>).
        /// Decode using the <c>SCHED_*</c> constants in <c>linux/sched.h</c>.
        /// </summary>
        public uint Policy { get; set; }

        /// <summary>
        /// Gets or sets the aggregated block I/O delays, measured in clock ticks (centiseconds).
        /// </summary>
        public ulong IODelays { get; set; }

        /// <summary>
        /// Gets or sets the guest time of the process (time spent running a virtual CPU for a guest operating system),
        /// measured in clock ticks (divide by <c>sysconf(_SC_CLK_TCK)</c>).
        /// </summary>
        public ulong GuestTime { get; set; }

        /// <summary>
        /// Gets or sets the guest time of the process's children, measured in clock ticks (divide by <c>sysconf(_SC_CLK_TCK)</c>).
        /// </summary>
        public long ChildGuestTime { get; set; }

        /// <summary>
        /// Gets or sets the address above which program initialized and uninitialized(BSS) data are placed.
        /// </summary>
        public ulong StartData { get; set; }

        /// <summary>
        /// Gets or sets the address below which program initialized and uninitialized(BSS) data are placed.
        /// </summary>
        public ulong EndData { get; set; }

        /// <summary>
        /// Gets or sets the address above which program heap can be expanded with <c>brk(2)</c>.
        /// </summary>
        public ulong StartBrk { get; set; }

        /// <summary>
        /// Gets or sets the address above which program command-line arguments (<c>argv</c>) are placed.
        /// </summary>
        public ulong ArgStart { get; set; }

        /// <summary>
        /// Gets or sets the address below program command-line arguments (<c>argv</c>) are placed.
        /// </summary>
        public ulong ArgEnd { get; set; }

        /// <summary>
        /// Gets or sets the address above which program environment is placed.
        /// </summary>
        public ulong EnvStart { get; set; }

        /// <summary>
        /// Gets or sets the address below which program environment is placed.
        /// </summary>
        public ulong EnvEnd { get; set; }

        /// <summary>
        /// Gets or sets the thread's exit status in the form reported by <c>waitpid(2)</c>.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Creates a <see cref="AndroidProcess"/> from it <see cref="string"/> representation.
        /// </summary>
        /// <param name="line">A <see cref="string"/> which represents a <see cref="AndroidProcess"/>.</param>
        /// <param name="cmdLinePrefix">A value indicating whether the output of <c>/proc/{pid}/stat</c> is prefixed with <c>/proc/{pid}/cmdline</c> or not.
        /// Because <c>stat</c> does not contain the full process name, this can be useful.</param>
        /// <returns>The equivalent <see cref="AndroidProcess"/>.</returns>
        public static AndroidProcess Parse(string line, bool cmdLinePrefix = false)
        {
            ExceptionExtensions.ThrowIfNull(line);

            AndroidProcess process = new();

            // See http://man7.org/linux/man-pages/man5/proc.5.html,
            // section /proc/[pid]/stat, for more information about the file format

            // Space delimited, so normally we would just do a string.split
            // The process name may contain spaces but is wrapped within parentheses, all other values (we know of) are
            // numeric.
            // So we parse the pid & process name manually, to account for this, and do the string.split afterwards :-)
            int processNameStart = line.IndexOf('(');
            int processNameEnd = line.LastIndexOf(')');

            int pid;
            string comm;

            bool parsedCmdLinePrefix = false;

            if (cmdLinePrefix)
            {
#if HAS_INDEXRANGE
                string[] cmdLineParts = line[..processNameStart]
#else

                string[] cmdLineParts = line.Substring(0, processNameStart)
#endif
                    .Split(new char[] { '\0' });

                if (cmdLineParts.Length <= 1)
                {
                    parsedCmdLinePrefix = false;
                }
                else
                {
#if HAS_INDEXRANGE
                    pid = int.Parse(cmdLineParts[^1]);
#else
                    pid = int.Parse(cmdLineParts[cmdLineParts.Length - 1]);
#endif
                    process.ProcessId = pid;

                    comm = cmdLineParts[0];
                    process.Name = comm;

                    // All the other parts are the command line arguments, skip them.
                    parsedCmdLinePrefix = true;
                }
            }

            if (!parsedCmdLinePrefix)
            {
#if HAS_INDEXRANGE
                pid = int.Parse(line[..processNameStart]);
#else
                pid = int.Parse(line.Substring(0, processNameStart));
#endif
                process.ProcessId = pid;

                comm = line.Substring(processNameStart + 1, processNameEnd - processNameStart - 1);
                process.Name = comm;
            }

#if HAS_INDEXRANGE
            string[] parts = line[(processNameEnd + 1)..]
#else
            string[] parts = line.Substring(processNameEnd + 1)
#endif
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 35)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            // Only fields in Linux 2.1.10 and earlier are listed here,
            // additional fields exist in newer versions of Linux.
            string state = parts[0];
            process.State = (AndroidProcessState)Enum.Parse(typeof(AndroidProcessState), state, true);

            int ppid = ParseInt(parts[1]);
            process.ParentProcessId = ppid;

            int pgrp = ParseInt(parts[2]);
            process.ProcessGroupId = pgrp;

            int session = ParseInt(parts[3]);
            process.SessionID = session;

            int tty_nr = ParseInt(parts[4]);
            process.TTYNumber = tty_nr;

            int tpgid = ParseInt(parts[5]);
            process.TopProcessGroupId = tpgid;

            uint flags = ParseUInt(parts[6]);
            process.Flags = (PerProcessFlags)flags;

            ulong minflt = ParseULong(parts[7]);
            process.MinorFaults = minflt;

            ulong cminflt = ParseULong(parts[8]);
            process.ChildMinorFaults = cminflt;

            ulong majflt = ParseULong(parts[9]);
            process.MajorFaults = majflt;

            ulong cmajflt = ParseULong(parts[10]);
            process.ChildMajorFaults = cmajflt;

            ulong utime = ParseULong(parts[11]);
            process.UserScheduledTime = utime;

            ulong stime = ParseULong(parts[12]);
            process.ScheduledTime = stime;

            long cutime = ParseLong(parts[13]);
            process.ChildUserScheduledTime = cutime;

            long cstime = ParseLong(parts[14]);
            process.ChildScheduledTime = cstime;

            long priority = ParseLong(parts[15]);
            process.Priority = priority;

            long nice = ParseLong(parts[16]);
            process.Nice = nice;

            long num_threads = ParseLong(parts[17]);
            process.ThreadsNumber = num_threads;

            long itrealvalue = ParseLong(parts[18]);
            process.Interval = itrealvalue;

            ulong starttime = ParseULong(parts[19]);
            process.StartTime = starttime;

            ulong vsize = ParseULong(parts[20]);
            process.VirtualSize = vsize;

            int rss = int.Parse(parts[21]);
            process.ResidentSetSize = rss;

            ulong rsslim = ParseULong(parts[22]);
            process.ResidentSetSizeLimit = rsslim;

            ulong startcode = ParseULong(parts[23]);
            process.StartCode = startcode;

            ulong endcode = ParseULong(parts[24]);
            process.EndCode = endcode;

            ulong startstack = ParseULong(parts[25]);
            process.StartStack = startstack;

            ulong kstkesp = ParseULong(parts[26]);
            process.ESP = kstkesp;

            ulong kstkeip = ParseULong(parts[27]);
            process.EIP = kstkeip;

            ulong signal = ParseULong(parts[28]);
            process.Signal = signal;

            ulong blocked = ParseULong(parts[29]);
            process.Blocked = blocked;

            ulong sigignore = ParseULong(parts[30]);
            process.IgnoredSignals = sigignore;

            ulong sigcatch = ParseULong(parts[31]);
            process.CaughtSignals = sigcatch;

            ulong wchan = ParseULong(parts[32]);
            process.WChan = wchan;

            ulong nswap = ParseULong(parts[33]);
            process.SwappedPagesNumber = nswap;

            ulong cnswap = ParseULong(parts[34]);
            process.CumulativeSwappedPagesNumber = cnswap;

            if (parts.Length < 36)
            {
                return process;
            }

            // Linux 2.1.22
            int exit_signal = ParseInt(parts[35]);
            process.ExitSignal = exit_signal;

            if (parts.Length < 37)
            {
                return process;
            }

            // Linux 2.2.8
            int processor = ParseInt(parts[36]);
            process.Processor = processor;

            if (parts.Length < 39)
            {
                return process;
            }

            // Linux 2.5.19
            uint rt_priority = ParseUInt(parts[37]);
            process.RealTimePriority = rt_priority;

            uint policy = ParseUInt(parts[38]);
            process.Policy = policy;

            if (parts.Length < 40)
            {
                return process;
            }

            // Linux 2.6.18
            ulong delayacct_blkio_ticks = ParseULong(parts[39]);

            process.IODelays = delayacct_blkio_ticks;

            if (parts.Length < 42)
            {
                return process;
            }

            // Linux 2.6.24
            ulong guest_time = ParseULong(parts[40]);
            process.GuestTime = guest_time;

            long cguest_time = ParseLong(parts[41]);
            process.ChildGuestTime = cguest_time;

            if (parts.Length < 45)
            {
                return process;
            }

            // Linux 3.3
            ulong start_data = ParseULong(parts[42]);
            process.StartData = start_data;

            ulong end_data = ParseULong(parts[43]);
            process.EndData = end_data;

            ulong start_brk = ParseULong(parts[44]);
            process.StartBrk = start_brk;

            if (parts.Length < 50)
            {
                return process;
            }

            // Linux 3.5
            ulong arg_start = ParseULong(parts[45]);
            process.ArgStart = arg_start;

            ulong arg_end = ParseULong(parts[46]);
            process.ArgEnd = arg_end;

            ulong env_start = ParseULong(parts[47]);
            process.EnvStart = env_start;

            ulong env_end = ParseULong(parts[48]);
            process.EnvEnd = env_end;

            int exit_code = ParseInt(parts[49]);
            process.ExitCode = exit_code;

            return process;
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
