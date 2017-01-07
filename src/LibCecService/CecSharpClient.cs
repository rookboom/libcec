/*
 * This file is part of the libCEC(R) library.
 *
 * libCEC(R) is Copyright (C) 2011-2013 Pulse-Eight Limited.  All rights reserved.
 * libCEC(R) is an original work, containing original code.
 *
 * libCEC(R) is a trademark of Pulse-Eight Limited.
 *
 * This program is dual-licensed; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 *
 * Alternatively, you can license this library under a commercial license,
 * please contact Pulse-Eight Licensing for more information.
 *
 * For more information contact:
 * Pulse-Eight Licensing       <license@pulse-eight.com>
 *     http://www.pulse-eight.com/
 *     http://www.pulse-eight.net/
 */

using System;
using CecSharp;

namespace CecSharpClient
{
    class CecSharpClient : IDisposable
    {
        public CecSharpClient()
        {
            var config = new LibCECConfiguration();
            config.DeviceTypes.Types[0] = CecDeviceType.RecordingDevice;
            config.DeviceName = "CEC Service";
            config.ClientVersion = LibCECConfiguration.CurrentVersion;
            var ignoreCallback = new CecCallbackMethods();
            config.SetCallbacks(ignoreCallback);
            Lib = new LibCecSharp(config);
            Lib.InitVideoStandalone();
        }

        public void Dispose()
        {
            Lib?.Dispose();
            Lib = null;
        }

        public bool Connect(int timeout)
        {
            CecAdapter[] adapters = Lib.FindAdapters(string.Empty);
            if (adapters.Length > 0)
                return Connect(adapters[0].ComPort, timeout);
            else
            {
                Console.WriteLine("Did not find any CEC adapters");
                return false;
            }
        }

        public bool Connect(string port, int timeout)
        {
            return Lib.Open(port, timeout);
        }

        public void Close()
        {
            Lib.Close();
        }

        public bool PowerOn()
        {
            return Lib.PowerOnDevices(CecLogicalAddress.Tv);
        }

        public bool Standby()
        {
            return Lib.StandbyDevices(CecLogicalAddress.Tv);
        }

        private LibCecSharp Lib;
    }
}