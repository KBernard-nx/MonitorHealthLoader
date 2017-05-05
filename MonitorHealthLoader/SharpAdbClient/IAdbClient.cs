﻿// <copyright file="IAdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using Logs;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A common interface for any class that allows you to interact with the
    /// adb server and devices that are connected to that adb server.
    /// </summary>
    public interface IAdbClient
    {
        /// <summary>
        /// Gets the <see cref="EndPoint"/> at which the Android Debug Bridge server is listening.
        /// </summary>
        EndPoint EndPoint { get; }

        // The individual services are listed in the same order as
        // https://android.googlesource.com/platform/system/core/+/master/adb/SERVICES.TXT

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetAdbVersion/*'/>
        int GetAdbVersion();

        /// <include file='IAdbClient.xml' path='/IAdbClient/KillAdb/*'/>
        void KillAdb();

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetDevices/*'/>
        List<DeviceData> GetDevices();

        // host:track-devices is implemented by the DeviceMonitor.
        // host:emulator is not implemented

        // host:transport-usb is not implemented
        // host:transport-local is not implemented
        // host:transport-any is not implemented

        // <host-prefix>:get-product is not implemented
        // <host-prefix>:get-serialno is not implemented
        // <host-prefix>:get-devpath is not implemented
        // <host-prefix>:get-state is not implemented

        /// <include file='IAdbClient.xml' path='/IAdbClient/CreateForward/*'/>
        void CreateForward(DeviceData device, string local, string remote, bool allowRebind);

        /// <include file='IAdbClient.xml' path='/IAdbClient/CreateForward/*'/>
        void CreateForward(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind);

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveForward/*'/>
        void RemoveForward(DeviceData device, int localPort);

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveAllForwards/*'/>
        void RemoveAllForwards(DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/ListForward/*'/>
        IEnumerable<ForwardData> ListForward(DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/ExecuteRemoteCommand/*'/>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken, int maxTimeToOutputResponse);

        // shell: not implemented
        // remount: not implemented
        // dev:<path> not implemented
        // tcp:<port> not implemented
        // tcp:<port>:<server-name> not implemented
        // local:<path> not implemented
        // localreserved:<path> not implemented
        // localabstract:<path> not implemented

        /// <summary>
        /// Gets a <see cref="Framebuffer"/> which contains the framebuffer data for this device. The framebuffer data can be refreshed,
        /// giving you high performance access to the device's framebuffer.
        /// </summary>
        /// <param name="device">
        /// The device for which to get the framebuffer.
        /// </param>
        /// <returns>
        /// A <see cref="Framebuffer"/> object which can be used to get the framebuffer of the device.
        /// </returns>
        Framebuffer CreateRefreshableFramebuffer(DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetFrameBuffer/*'/>
        Task<Image> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken);

        // jdwp:<pid>: not implemented
        // track-jdwp: not implemented
        // sync: not implemented
        // reverse:<forward-command>: not implemented

        /// <include file='IAdbClient.xml' path='/IAdbClient/RunLogService/*'/>
        Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames);

        /// <include file='IAdbClient.xml' path='/IAdbClient/Reboot/*'/>
        void Reboot(string into, DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/Connect/*'/>
        void Connect(DnsEndPoint endpoint);

        /// <include file='IAdbClient.xml' path='/IAdbClient/SetDevice/*'/>
        void SetDevice(IAdbSocket socket, DeviceData device);
        IEnumerable<LogEntry> RunLogService(DeviceData deviceData, LogId[] logNames);
    }
}
