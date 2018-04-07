/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
namespace FilterIcap.Net.Exceptions
{
    public class IcapException : Exception
    {
        public IcapException(string message) : base(message)
        {
        }

        public IcapException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
