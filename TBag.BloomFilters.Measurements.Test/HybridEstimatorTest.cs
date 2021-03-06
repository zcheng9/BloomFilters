﻿namespace TBag.BloomFilters.Measurements.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;
    using System.IO;
    using BloomFilters.Estimators;
    using Invertible.Estimators;
    /// <summary>
    /// Summary description for hybridEstimatorTest
    /// </summary>
    [TestClass]
    public class HybridEstimatorTest
    {
        /// <summary>
        /// Generate performance data for the hybrid estimator.
        /// </summary>
        [TestMethod]
        public void HybridEstimatorPerformanceMeasurement()
        {
            var configuration = new LargeBloomFilterConfiguration();
            var testSizes = new [] { 5000, 10000, 100000, 500000};
            var errorSizes = new [] { 1, 5, 10, 20, 50, 75, 100};
            var capacities = new long[] { 80, 250, 1000};
            var stratas = new byte[] { 3, 7, 9, 13, 19, 25, 32};
            foreach (var dataSize in testSizes)
            {

                foreach (var errorSize in errorSizes)
                {
                    using (
                        var writer =
                            new StreamWriter(File.Open($"hybridestimator-{dataSize}-{errorSize}.csv",
                                FileMode.Create)))
                    {
                        writer.WriteLine(
                            "duration,dataSize,strata,capacity,modCount,estimatedModCount,countDiff,countDiffSd,decodeSuccessRate");
                        foreach (var capacity in capacities)
                        {
                            foreach (var strata in stratas)
                            {
                                var timeSpanAggregate = new TimeSpan[50];
                                var countAggregate = new int[50];
                                var modCountResultAggregate = new int[50];
                                var decodeResult = new int[50];

                                for (var run = 0; run < 50; run++)
                                {
                                    var testData = DataGenerator.Generate().Take(dataSize).ToList();
                                    var modCount = (int) (dataSize/100.0D*errorSize);
                                    var startTime = DateTime.UtcNow;
                                    var estimator1 = new HybridEstimator<TestEntity, long, int>(capacity, strata, configuration);
                                    estimator1.Initialize((uint)testData.Count, 2, 10);
                                    foreach (var item in testData)
                                    {
                                        estimator1.Add(item);
                                    }
                                    testData.Modify(modCount);
                                    var estimator2 = new HybridEstimator<TestEntity, long, int>(capacity, strata, configuration);
                                    estimator2.Initialize((uint)testData.Count, 2, 10);
                                    foreach (var item in testData)
                                    {
                                        estimator2.Add(item);
                                    }
                                    var measuredModCount = estimator1.Decode(estimator2);
                                    timeSpanAggregate[run] = DateTime.UtcNow.Subtract(startTime);
                                    countAggregate[run] = modCount;
                                    modCountResultAggregate[run] = (int)(measuredModCount??0L);
                                    decodeResult[run] = measuredModCount.HasValue ? 1 : 0;

                                }
                                var timeAvg = new TimeSpan((long) timeSpanAggregate.Select(t => t.Ticks).Average());
                                var countAvg = (long) countAggregate.Average();
                                var modCountResult = (long) modCountResultAggregate.Average();
                                var differenceResult =
                                    modCountResultAggregate.Select((r, i) => r - countAggregate[i]).ToArray();
                                var differenceSd = Math.Sqrt(differenceResult.Variance());
                                writer.WriteLine(
                                    $"{timeAvg.TotalMilliseconds},{dataSize},{strata},{capacity},{countAvg},{modCountResult},{(long) differenceResult.Average()},{differenceSd},{1.0D * decodeResult.Sum() / 50}");
                            }
                        }

                    }
                }
            }
        }
    }
}
