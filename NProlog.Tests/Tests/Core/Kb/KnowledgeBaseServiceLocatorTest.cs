/*
 * Copyright 2013 S. Webber
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
using System.Runtime.Serialization;
using System.Text;

namespace Org.NProlog.Core.Kb;

[TestClass]
public class KnowledgeBaseServiceLocatorTest : TestUtils
{
    /** Tests one-to-one relationship between KnowledgeBase and KnowledgeBaseServiceLocator instances */
    [TestMethod]
    public void TestCreation()
    {
        var kb1 = CreateKnowledgeBase();
        var sl1 = KnowledgeBaseServiceLocator.GetServiceLocator(kb1);
        Assert.IsNotNull(sl1);
        Assert.AreSame(sl1, KnowledgeBaseServiceLocator.GetServiceLocator(kb1));

        var kb2 = CreateKnowledgeBase();
        var sl2 = KnowledgeBaseServiceLocator.GetServiceLocator(kb2);
        Assert.IsNotNull(sl2);
        Assert.AreNotSame(sl1, sl2);
    }

    [TestMethod]
    public void TestGetInstanceOneArgument()
    {
        var l = CreateKnowledgeBaseServiceLocator();
        var o = l.GetInstanceForClass<object>(typeof(Object));
        Assert.AreSame(o, l.GetInstanceForClass<object>(typeof(Object)));

        var sb = l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder));
        Assert.AreSame(sb, l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder)));
        Assert.AreNotSame(sb, o);
        //Assert.AreNotSame(sb, l.GetInstance<StringBuilder>(typeof(StringBuilder)));
    }

    [TestMethod]
    public void TestGetInstanceTwoArguments()
    {
        var l = CreateKnowledgeBaseServiceLocator();

        var o = l.GetInstanceForClass<StringBuilder>(typeof(Object), typeof(StringBuilder));
        Assert.AreSame(o, l.GetInstanceForClass<StringBuilder>(typeof(Object), typeof(StringBuilder)));
        Assert.AreSame(o, l.GetInstanceForClass<StringBuilder>(typeof(Object), typeof(StringBuilder)));
        Assert.AreSame(o, l.GetInstanceForClass<StringBuilder>(typeof(Object)));

        var c = l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder), typeof(StringBuilder));
        Assert.AreSame(c, l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder), typeof(StringBuilder)));
        Assert.AreSame(c, l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder), typeof(StringBuilder)));
        Assert.AreNotSame(c, l.GetInstanceForClass<string>(typeof(string)));

        Assert.AreNotSame(o, c);
        Assert.AreNotSame(o, l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder)));
        Assert.AreSame(c, l.GetInstanceForClass<StringBuilder>(typeof(StringBuilder)));
    }

    [TestMethod]
    public void TestGetInstanceInterface()
    {
        try
        {
            CreateKnowledgeBaseServiceLocator().GetInstanceForClass<ISerializable>(typeof(ISerializable));
            Assert.Fail();
        }
        catch (SystemException e)
        {
            Assert.AreEqual("Could not create new instance of service: System.Runtime.Serialization.ISerializable", e.Message);
        }
    }

    [TestMethod]
    public void TestGetInstanceNoValidConstructor()
    {
        try
        {
            CreateKnowledgeBaseServiceLocator().GetInstanceForStruct<int>(typeof(int));
            //Assert.Fail();
        }
        catch (SystemException e)
        {
            Assert.AreEqual("Could not create new instance of service: class java.lang.int", e.Message);
        }
    }

    [TestMethod]
    public void TestGetInstanceInstanceDoesNotExtendReference()
    {
        try
        {
            CreateKnowledgeBaseServiceLocator().GetInstanceForClass<StringBuilder>(typeof(StringBuilder), typeof(StringBuilder));
            //Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("class java.lang.StringBuilder is not of type: System.Text.StringBuilder", e.Message);
        }
    }

    public static void TestGetInstanceInstanceDoesNotImplementReference()
    {
        try
        {
            CreateKnowledgeBaseServiceLocator().GetInstanceForClass<StringBuilder>(typeof(IDictionary<string, string>), typeof(StringBuilder));
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("class java.lang.StringBuilder is not of type: interface Dictionary", e.Message);
        }
    }

    [TestMethod]
    public void TestAddInstance()
    {
        var l = CreateKnowledgeBaseServiceLocator();
        var s = "hello";
        l.AddInstance(typeof(string), s);
        Assert.AreSame(s, l.GetInstanceForClass<string>(typeof(string)));
    }

    [TestMethod]
    public void TestAddInstanceIllegalStateException()
    {
        var l = CreateKnowledgeBaseServiceLocator();
        l.AddInstance(typeof(string), "hello");
        try
        {
            l.AddInstance(typeof(string), "hello");
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual("Already have a service with key: System.String", e.Message);
        }
    }

    [TestMethod]
    public void TestAddInstanceIllegalArgumentException()
    {
        try
        {
            CreateKnowledgeBaseServiceLocator().AddInstance(typeof(StringBuilder), "hello");
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual("hello is not of type: System.Text.StringBuilder", e.Message);
        }
    }

    /** Test that the KnowledgeBase gets passed as an argument to the constructor of new services */
    [TestMethod]
    public void TestClassWithSingleKnowledgeBaseArgumentConstrutor()
    {
        var kb = CreateKnowledgeBase();
        var l = KnowledgeBaseServiceLocator.GetServiceLocator(kb);
        var s = l.GetInstanceForClass<DummyService>(typeof(DummyService));
        Assert.AreSame(s, l.GetInstanceForClass<DummyService>(typeof(DummyService)));
        Assert.AreSame(kb, s?.kb);
    }

    private static KnowledgeBaseServiceLocator CreateKnowledgeBaseServiceLocator()
        => KnowledgeBaseServiceLocator.GetServiceLocator(CreateKnowledgeBase());

    public class DummyService
    {
        public readonly KnowledgeBase kb;

        public DummyService(KnowledgeBase kb) => this.kb = kb;
    }
}
