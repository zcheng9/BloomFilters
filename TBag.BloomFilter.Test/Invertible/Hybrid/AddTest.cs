﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBag.BloomFilters.Invertible;
using TBag.BloomFilter.Test.Infrastructure;
using System.Linq;

namespace TBag.BloomFilter.Test.Invertible.Hybrid
{
    [TestClass]
    public class AddTest
    {
        [TestMethod]
        public void HybridAddTest()
        {
            var addSize = 10000;
            var testData = DataGenerator.Generate().Take(addSize).ToArray();
            var testData2 = DataGenerator.Generate().Skip(addSize).Take(addSize).ToArray();
            var errorRate = 0.001F;
            var size = testData.Length;
            var configuration = new HybridDefaultBloomFilterConfiguration();
            var bloomFilter = new InvertibleHybridBloomFilter<TestEntity, long, sbyte>(configuration);
            bloomFilter.Initialize(2*size, errorRate);
            foreach (var itm in testData)
            {
                bloomFilter.Add(itm);
            }
            var bloomFilter2 = new InvertibleHybridBloomFilter<TestEntity, long, sbyte>(configuration);
            bloomFilter2.Initialize(2 * size, errorRate);
            foreach (var itm in testData2)
            {
                bloomFilter2.Add(itm);
            }
            bloomFilter.Add(bloomFilter2);
            var contained = testData.Union(testData2).Count(item => bloomFilter.Contains(item));
            Assert.AreEqual(contained, 2 * addSize, "Not all items found in added Bloom filters");
        }

        [TestMethod]
        public void HybridAddDifferentSizesTest()
        {
            var addSize = 10000;
            var testData = DataGenerator.Generate().Take(addSize).ToArray();
            var testData2 = DataGenerator.Generate().Skip(addSize).Take(addSize).ToArray();
            var errorRate = 0.001F;
            var size = testData.Length;
            var configuration = new HybridDefaultBloomFilterConfiguration();
            var bloomFilter = new InvertibleHybridBloomFilter<TestEntity, long, sbyte>(configuration);
            bloomFilter.Initialize(4 * size, errorRate);
            foreach (var itm in testData)
            {
                bloomFilter.Add(itm);
            }
            var bloomFilter2 = new InvertibleHybridBloomFilter<TestEntity, long, sbyte>(configuration);
            //We should create a foldable counter part.
            var data = bloomFilter.Extract();
            var foldFactor = configuration.FoldingStrategy.GetAllFoldFactors(data.BlockSize).Where(f=>f>1).OrderBy(f=> f).First();
            bloomFilter2.Initialize(addSize, data.BlockSize / foldFactor,  data.HashFunctionCount);
            foreach (var itm in testData2)
            {
                bloomFilter2.Add(itm);
            }
            bloomFilter.Add(bloomFilter2);
            var contained = testData.Union(testData2).Count(item => bloomFilter.Contains(item));
            Assert.AreEqual(contained, 2 * addSize, "Not all items found in added Bloom filters");
        }
    }
}
