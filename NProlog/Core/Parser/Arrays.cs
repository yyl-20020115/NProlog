/*
 * Copyright 2013-2014 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Org.NProlog.Core.Parser;

public static class Arrays
{
    public static string ToString(params object[] values) 
        => string.Join(", ", values);

    public static int GetHashCode(params object[] values)
    {
        int hash = 0;
        foreach(var o in values) 
            hash = 31 * hash + (o!=null? o.GetHashCode():0);
        return hash;
    }
    public static T[] Fill<T>(T[] args, T v)
    {
        for (int i = 0; i < args.Length; i++) args[i] = v;
        return args;
    }

}
