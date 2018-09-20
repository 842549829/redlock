﻿#region LICENSE
/*
 *   Copyright 2014 Angelo Simone Scotto <scotto.a@gmail.com>
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 * 
 * */
#endregion

using StackExchange.Redis;
using System;

namespace Redlock.CSharp
{

    // github https://github.com/kidfashion/redlock-cs
    public class Lock
    {

        public Lock()
        {
        }

        public Lock(RedisKey resource, RedisValue val, TimeSpan validity)
        {
            this.Resource = resource;
            this.Value = val ;
            this.Validity = validity;
        }

        public RedisKey Resource { get; }

        public RedisValue Value { get; }

        public TimeSpan Validity { get; }

        public bool IsLock { get; set; }
    }
}