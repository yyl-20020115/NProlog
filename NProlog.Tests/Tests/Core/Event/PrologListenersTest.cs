/*
 * Copyright 2013-2014 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Text;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Event;

[TestClass]
public class PrologListenersTest
{
    [TestMethod]
    public void TestPrologEventsObservable()
    {
        var testObject = new PrologListeners();

        DummyListener o1 = new DummyListener();
        DummyListener o2 = new DummyListener();
        DummyListener o3 = new DummyListener();

        testObject.NotifyInfo("info1");

        testObject.AddListener(o1);
        testObject.AddListener(o1);
        testObject.AddListener(o2);
        testObject.AddListener(o3);

        testObject.NotifyWarn("warn");

        testObject.DeleteListener(o2);

        testObject.NotifyInfo("info2");

        Assert.AreEqual(2, o1.
        Count);
        Assert.AreEqual(1, o2.Count);
        Assert.AreEqual(2, o3.Count);
        Assert.AreEqual("warninfo2", o1.result());
        Assert.AreEqual("warn", o2.result());
        Assert.AreEqual("warninfo2", o3.result());
    }

    public class DummyListener : PrologListener
    {
        private readonly List<string> events = new();


        public void OnInfo(string message)
        {
            Add(message);
        }


        public void OnWarn(string message)
        {
            Add(message);
        }


        public void OnRedo(SpyPointEvent @event)
        {
            throw new InvalidOperationException();
        }


        public void OnFail(SpyPointEvent @event)
        {
            throw new InvalidOperationException();
        }


        public void OnExit(SpyPointExitEvent @event)
        {
            throw new InvalidOperationException();
        }


        public void OnCall(SpyPointEvent @event)
        {
            throw new InvalidOperationException();
        }

        private void Add(string message)
        {
            events.Add(message);
        }

        public int Count => events.Count;

        public string result()
        {
            var result = new StringBuilder();
            foreach (string @event in events)
            {
                result.Append(@event);
            }
            return result.ToString();
        }
    }
}
