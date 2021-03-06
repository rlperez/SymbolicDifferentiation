﻿#region License

/* ****************************************************************************
 * Copyright (c) Edmondo Pentangelo. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. 
 * A copy of the license can be found in the License.html file at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 * ***************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SymbolicDifferentiation.Core.Computation;

namespace SymbolicDifferentiation.Tests.Computation
{
    public class ParallelComputationTests : ComputationTests
    {
        protected override IDictionary<string, Func<IEnumerable<IEnumerable<KeyValuePair<string, Atom>>>, IEnumerable<KeyValuePair<string, Atom>>>> Compute(string input)
        {
            return ComputeParallel(input, 3);
        }

        [Test]
        [Ignore("long running performance test")]
        public void AddLots()
        {
            var expression = "X = (((A + B) * (A + B)) * ((A + B) * (A + B))) + (((A + B) * (A + B)) * ((A + B) * (A + B)))";

            const int _size = 1000000;

            _data = new Dictionary<string, IEnumerable<KeyValuePair<string, Atom>>>
                        {
                            {"A", Enumerable.Range(0, _size).Select(i => new KeyValuePair<string,Atom>("", i + .0))},
                            {"B", Enumerable.Range(_size, _size).Select(i => new KeyValuePair<string,Atom>("", i + .0))},
                        };


            var watch = new Stopwatch();
            watch.Reset();
            Console.WriteLine("Start sequential...");
            watch.Start();
            var result2 = ComputeSequential(expression, _size)["X"](new[]{new KeyValuePair<string, Atom>[0]}).Select(item => item.Value).ToArray();
            watch.Stop();
            Console.WriteLine("Sequential elapsed:{0}", watch.Elapsed);
            watch.Reset();
            Console.WriteLine("Start parallel...");
            watch.Start();
            var result = ComputeParallel(expression, _size)["X"](new[]{new KeyValuePair<string, Atom>[0]}).Select(item => item.Value).ToArray();
            watch.Stop();
            Console.WriteLine("Parallel elapsed:{0}", watch.Elapsed);

            CollectionAssert.AreEqual(result, result2);
        }
    }
}