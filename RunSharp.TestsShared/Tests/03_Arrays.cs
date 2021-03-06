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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace TriAxis.RunSharp.Tests
{
	[TestFixture]
    public class _03_Arrays : TestBase
    {
        [Test]
        public void TestGenArrays()
        {
            TestingFacade.GetTestsForGenerator(GenArrays, @">>> GEN TriAxis.RunSharp.Tests.03_Arrays.GenArrays
=== RUN TriAxis.RunSharp.Tests.03_Arrays.GenArrays
Length of row 2 is 5
Length of row 3 is 6
Length of row 4 is 7
<<< END TriAxis.RunSharp.Tests.03_Arrays.GenArrays

").RunAll();
        }

        // example based on the MSDN Arrays Sample (arrays.cs)
        public static void GenArrays(AssemblyGen ag)
		{
            var st = ag.StaticFactory;
            var exp = ag.ExpressionFactory;

            TypeGen DeclareArraysSample = ag.Class("DecalreArraysSample");
		    {
				CodeGen g = DeclareArraysSample.Public.Static.Method(typeof(void), "Main");
				{
                    // Single-dimensional array
                    var numbers = g.Local(exp.NewArray(typeof(int), 5));

                    // Multidimensional array
                    var names = g.Local(exp.NewArray(typeof(string), 5, 4));

                    // Array-of-arrays (jagged array)
                    var scores = g.Local(exp.NewArray(typeof(byte[]), 5));

                    // Create the jagged array
                    var i = g.Local();
					g.For(i.Assign(0), i < scores.ArrayLength(), i.Increment());
					{
						g.Assign(scores[i], exp.NewArray(typeof(byte), i + 3));
					}
					g.End();

					// Print length of each row from 3
					g.For(i.Assign(2), i < scores.ArrayLength(), i.Increment());
					{
						g.WriteLine("Length of row {0} is {1}", i, scores[i].ArrayLength());
					}
					g.End();
				}
			}
		}
	}
}
