using System;
using System.Collections.Generic;
using System.Text;

namespace LogEntryClustering
{
    public class Tests
    {
        //[TestMethod]
        /*
        public void MinHashFunc1()
        {
            var inums1 = new List<int>();
            inums1.Add(10);
            inums1.Add(8);
            inums1.Add(11);
            inums1.Add(13);
            inums1.Add(2);
            inums1.Add(17);
            inums1.Add(3);
            inums1.Add(1);
            inums1.Add(19);
            inums1.Add(11);
            MinHash mh = new MinHash(20, 100);
            var hvs1 = mh.GetMinHash(inums1).ToList();

            List inums2 = new List();
            inums2.Add(1);
            inums2.Add(2);
            inums2.Add(5);
            inums2.Add(9);
            inums2.Add(12);
            inums2.Add(17);
            inums2.Add(13);
            inums2.Add(11);
            inums2.Add(9);
            inums2.Add(10);

            List hvs2 = mh.GetMinHash(inums2).ToList();
            Console.WriteLine();
            Console.WriteLine("Estimated similarity: " + mh.Similarity(hvs1, hvs2));
            Console.WriteLine("Jaccard similarity: " + Jaccard.Calc(inums1, inums2));
        }

        [TestMethod]
        public void CheckLSHAll()
        {
            string lang = "en";
            SectionBO.MinHashForSections mhfs = SectionBO.GetMinHash(4, 100, lang, 1);
            LSH lsh = new LSH(mhfs.hashes, 20);
            lsh.Calc();
            for (int n = 0; n < mhfs.hashes.GetUpperBound(0); n++)
            {
                List<int> nearest = lsh.GetNearest(n);
            }
        }
    */
    }
}
