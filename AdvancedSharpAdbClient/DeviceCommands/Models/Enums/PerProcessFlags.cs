// <copyright file="PerProcessFlags.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.DeviceCommands.Models
{
    /// <summary>
    /// Per process flags.
    /// </summary>
    [Flags]
    public enum PerProcessFlags : uint
    {
        /// <summary>
        /// The per process flag is unknown.
        /// </summary>
        Unknown = 0x00000000,

        /// <summary>
        /// I'm a virtual CPU.
        /// </summary>
        VCPU = 0x00000001,

        /// <summary>
        /// I am an IDLE thread.
        /// </summary>
        IDLE = 0x00000002,

        /// <summary>
        /// Getting shut down.
        /// </summary>
        EXITING = 0x00000004,

        /// <summary>
        /// Coredumps should ignore this task.
        /// </summary>
        POSTCOREDUMP = 0x00000008,

        /// <summary>
        /// Task is an IO worker.
        /// </summary>
        IO_WORKER = 0x00000010,

        /// <summary>
        /// I'm a workqueue worker.
        /// </summary>
        WQ_WORKER = 0x00000020,

        /// <summary>
        /// Forked but didn't exec.
        /// </summary>
        FORKNOEXEC = 0x00000040,

        /// <summary>
        /// Process policy on mce errors.
        /// </summary>
        MCE_PROCESS = 0x00000080,

        /// <summary>
        /// Used super-user privileges.
        /// </summary>
        SUPERPRIV = 0x00000100,

        /// <summary>
        /// Dumped core.
        /// </summary>
        DUMPCORE = 0x00000200,

        /// <summary>
        /// Killed by a signal.
        /// </summary>
        SIGNALED = 0x00000400,

        /// <summary>
        /// Allocating memory.
        /// </summary>
        MEMALLOC = 0x00000800,

        /// <summary>
        /// set_user() noticed that RLIMIT_NPROC was exceeded
        /// </summary>
        NPROC_EXCEEDED = 0x00001000,

        /// <summary>
        /// If unset the fpu must be initialized before use
        /// </summary>
        USED_MATH = 0x00002000,

        /// <summary>
        /// Hole (0x00004000)
        /// </summary>
        HOLE_00004000 = 0x00004000,

        /// <summary>
        /// This thread should not be frozen
        /// </summary>
        NOFREEZE = 0x00008000,

        /// <summary>
        /// Hole (0x00010000)
        /// </summary>
        HOLE_00010000 = 0x00010000,

        /// <summary>
        /// I am kswapd
        /// </summary>
        KSWAPD = 0x00020000,

        /// <summary>
        /// All allocation requests will inherit GFP_NOFS.
        /// </summary>
        MEMALLOC_NOFS = 0x00040000,

        /// <summary>
        /// All allocation requests will inherit GFP_NOIO.
        /// </summary>
        MEMALLOC_NOIO = 0x00080000,

        /// <summary>
        /// Throttle writes only against the bdi I write to, I am cleaning dirty pages from some other bdi.
        /// </summary>
        LOCAL_THROTTLE = 0x00100000,

        /// <summary>
        /// I am a kernel thread.
        /// </summary>
        KTHREAD = 0x00200000,

        /// <summary>
        /// Randomize virtual address space.
        /// </summary>
        RANDOMIZE = 0x00400000,

        /// <summary>
        /// Hole (0x00800000)
        /// </summary>
        HOLE_00800000 = 0x00800000,

        /// <summary>
        /// Hole (0x01000000)
        /// </summary>
        HOLE_01000000 = 0x01000000,

        /// <summary>
        /// Hole (0x02000000)
        /// </summary>
        HOLE_02000000 = 0x02000000,

        /// <summary>
        /// Userland is not allowed to meddle with cpus_mask.
        /// </summary>
        NO_SETAFFINITY = 0x04000000,

        /// <summary>
        /// Early kill for mce process policy.
        /// </summary>
        MCE_EARLY = 0x08000000,

        /// <summary>
        /// Allocation context constrained to zones which allow long term pinning.
        /// </summary>
        MEMALLOC_PIN = 0x10000000,

        /// <summary>
        /// Hole (0x20000000)
        /// </summary>
        HOLE_20000000 = 0x20000000,

        /// <summary>
        /// Hole (0x40000000)
        /// </summary>
        HOLE_40000000 = 0x40000000,

        /// <summary>
        /// This thread called freeze_processes() and should not be frozen.
        /// </summary>
        SUSPEND_TASK = 0x80000000
    }
}
