/*
Copyright(c) 2009, Stefan Simek
Copyright(c) 2016, Vladyslav Taranov

MIT License

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

// SF.net Bug ID: 1864084
// Submitted by ninj on CodeProject and SF.net
// fixed in 0.1.2-pre-alpha, crashes in older versions

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TriAxis.RunSharp;

namespace TriAxis.RunSharp.Tests.Bugs
{
    [TestFixture]
    public class X1864084_ValueTypeVirtual : TestBase
    {
        public static void GenOriginalTest(AssemblyGen ag)
        {
            ITypeMapper m = ag.TypeMapper;
            TypeGen Test = ag.Class("Test");
            {
                CodeGen g = Test.Public.Static.Method(typeof(void), "Main");
                {
                    var value = g.Local(typeof(int), 3);
                    g.WriteLine("Hash code of {0} is {1}", value, value.Invoke("GetHashCode"));
                }
            }
        }

        public static void StructImpl(AssemblyGen ag, bool impl)
        {
            ITypeMapper m = ag.TypeMapper;
            TypeGen Test = ag.Struct("Test");
            {
                CodeGen g = Test.Public.Static.Method(typeof(void), "Main");
                {
                    // test calling virtual member directly on a literal
                    // g.Local(Operand.FromObject(3).Invoke("GetHashCode"));

                    // test special case where the value type target doesn't implement the virtual function
                    var value = g.Local(Test);
                    g.InitObj(value);
                    g.WriteLine("Hash code of {0} is {1}", value, value.Invoke("GetHashCode"));
                }

                if (impl)
                {
                    g = Test.Public.Override.Method(typeof(int), "GetHashCode");
                    {
                        g.Return(-1);
                    }
                }
            }
        }

        public static void ClassImpl(AssemblyGen ag, bool impl)
        {
            ITypeMapper m = ag.TypeMapper;
            TypeGen Test = ag.Class("Test");
            {
                CodeGen g = Test.Public.Static.Method(typeof(void), "Main");
                {
                    // test calling virtual member directly on a literal
                    // g.Local(Operand.FromObject(3).Invoke("GetHashCode"));

                    // test special case where the value type target doesn't implement the virtual function
                    var value = g.Local(Test);
                    g.Assign(value, ag.ExpressionFactory.New(Test));
                    g.WriteLine("Hash code of {0} is {1}", value, value.Invoke("GetHashCode"));
                }

                if (impl)
                {
                    g = Test.Public.Override.Method(typeof(int), "GetHashCode");
                    {
                        g.Return(-1);
                    }
                }
            }
        }

        [Test]
        public void TestGenOriginalTest()
        {
            TestingFacade.GetTestsForGenerator(
                GenOriginalTest,
                @">>> GEN TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenOriginalTest
=== RUN TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenOriginalTest
Hash code of 3 is 3
<<< END TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenOriginalTest

").RunAll();
        }

        [Test]
        public void TestGenClassImpl()
        {
            TestingFacade.GetTestsForGenerator(
                GenClassImpl,
                @">>> GEN TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenClassImpl
=== RUN TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenClassImpl
Hash code of Test is -1
<<< END TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenClassImpl

").RunAll();
        }

        void GenClassImpl(AssemblyGen ag)
        {
            ClassImpl(ag, true);
        }

        [Test]
        public void TestGenStructImpl()
        {
            TestingFacade.GetTestsForGenerator(GenStructImpl,
                @">>> GEN TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenStructImpl
=== RUN TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenStructImpl
Hash code of Test is -1
<<< END TriAxis.RunSharp.Tests.Bugs.X1864084_ValueTypeVirtual.GenStructImpl

").RunAll();
        }

        void GenStructImpl(AssemblyGen ag)
        {
            StructImpl(ag, true);
        }

        [Test]
        public void TestGenClassNotImpl()
        {
            TestingFacade.GetTestsForGenerator(GenClassNotImpl, null).RunAll();
        }

        void GenClassNotImpl(AssemblyGen ag)
        {
            ClassImpl(ag, false);
        }

        [Test]
        public void TestGenStructNotImpl()
        {
            TestingFacade.GetTestsForGenerator(GetStructNotImpl, null).RunAll();
        }

        void GetStructNotImpl(AssemblyGen ag)
        {
            StructImpl(ag, false);
        }
    }
}