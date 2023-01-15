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
namespace Org.NProlog.Core.Terms;

[TestClass]
public class TermFormatterTest : TestUtils
{
    [TestMethod]
    public void TestTermToString()
    {
        string inputSyntax = "?- X = -1 + 1.684 , p(1, 7.3, [_,[]|c])";
        Term inputTerm = ParseSentence(inputSyntax + ".");

        TermFormatter tf = CreateFormatter();
        Assert.AreEqual(inputSyntax, tf.FormatTerm(inputTerm));
    }

    private static TermFormatter CreateFormatter()
    {
        return CreateKnowledgeBase().TermFormatter;
    }
}
