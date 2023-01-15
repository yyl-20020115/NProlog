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
namespace Org.NProlog.Core.Kb;

[TestClass]
public class CoreUtilsTest
{
    [TestMethod]
    public void TestInstantiateUsingNoArgConstructor()
    {
        var s = KnowledgeBaseUtils.Instantiate<string>("System.String");
        Assert.AreEqual("", s);
    }

    [TestMethod]
    public void TestInstantiateUsingStaticMethod()
    {
        var c = KnowledgeBaseUtils.Instantiate<DateTime>("Calendar/GetInstance");
        Assert.IsNotNull(c);
    }

    [TestMethod]
    public void TestInstantiateClassNotFound()
    {
        KnowledgeBaseUtils.Instantiate<object>("Org.NProlog.DoesntExist");
    }

    [TestMethod]
    public void TestInstantiateNoSuchMethod()
    {
        KnowledgeBaseUtils.Instantiate<string>("System.String/GetInstance");
    }

    [TestMethod]
    public void TestInstantiateIllegalAccess()
    {
        KnowledgeBaseUtils.Instantiate<object>(null, "Calendar");
    }

    [TestMethod]
    public void TestKnowledgeBaseConsumerNoArgConstructor()
    {
        var knowledgeBase = KnowledgeBaseUtils.CreateKnowledgeBase();
        var o = KnowledgeBaseUtils.Instantiate<KnowledgeBaseConsumerNoArgConstructorExample>(
            knowledgeBase,
                    "Org.NProlog.Core.Kb.KnowledgeBaseConsumerNoArgConstructorExample");
        Assert.IsNotNull(o);
        Assert.AreSame(knowledgeBase, o.kb);
        Assert.AreEqual(1, KnowledgeBaseConsumerNoArgConstructorExample.INSTANCE_CTR);
    }

    [TestMethod]
    public void TestKnowledgeBaseConsumerStaticMethod()
    {
        KnowledgeBase knowledgeBase = KnowledgeBaseUtils.CreateKnowledgeBase();
        var o = KnowledgeBaseUtils.Instantiate<KnowledgeBaseConsumerStaticMethodExample>(knowledgeBase,
                    "Org.NProlog.Core.Kb.KnowledgeBaseConsumerStaticMethodExample/Create");
        Assert.IsNotNull(o);
        Assert.AreSame(knowledgeBase, o.kb);
        Assert.AreEqual(1, KnowledgeBaseConsumerStaticMethodExample.INSTANCE_CTR);
    }

}
public class KnowledgeBaseConsumerNoArgConstructorExample : KnowledgeBaseConsumer
{
    public static int INSTANCE_CTR;

    public KnowledgeBase kb;

    public KnowledgeBaseConsumerNoArgConstructorExample()
    {
        INSTANCE_CTR++;
    }

    public KnowledgeBase KnowledgeBase { get => kb; set => kb = value; }

    public void SetKnowledgeBase(KnowledgeBase knowledgeBase)
    {
        if (this.kb != null || knowledgeBase == null)
        {
            throw new InvalidOperationException();
        }
        this.kb = knowledgeBase;
    }
}

public class KnowledgeBaseConsumerStaticMethodExample : KnowledgeBaseConsumer
{
    public static int INSTANCE_CTR;

    public KnowledgeBase kb;

    public KnowledgeBase KnowledgeBase { get => kb; set => kb = value; }

    public static KnowledgeBaseConsumerStaticMethodExample Create()
    {
        INSTANCE_CTR++;
        return new KnowledgeBaseConsumerStaticMethodExample();
    }

    private KnowledgeBaseConsumerStaticMethodExample()
    {
    }


    public void SetKnowledgeBase(KnowledgeBase knowledgeBase)
    {
        if (this.kb != null || knowledgeBase == null)
        {
            throw new InvalidOperationException();
        }
        this.kb = knowledgeBase;
    }
}
