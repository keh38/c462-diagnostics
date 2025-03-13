using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Schedule
    {
        public Mode mode = Mode.Sequence;
        public int numBlocks = 1;
        public string decisionState = "";
        public Order order = Order.Interleave;
        public List<Family> families = new List<Family>();
        public bool training = false;
        public float targetPc = 0;
        public string performancePrompt = "";
        public float targetCV = 0.2f;
        public int maxExtraBlocks = 0;
        public int offerBreakAfter = 0;
        public string breakInstructions = "";
        public int maxPracticeBlocks = -1;

        public Schedule()
        {
        }

        public void AppendNewStimConList(StimConList sclIn, int numNewBlocks)
        {
            int firstBlockNum = sclIn[sclIn.Count - 1].block ;

            int tmp_nblocks = numBlocks;
            numBlocks = numNewBlocks;

            StimConList scl = CreateStimConList();
            numBlocks = tmp_nblocks;

            foreach (SCLElement sc in scl) sc.block += firstBlockNum;

            sclIn.AddRange(scl);
        }


        public StimConList CreateStimConList()
        {
            List<StimConList> sclByFamily = new List<StimConList>();
            StimConList scl = null;

            if (families.Count == 0)
            {
                scl = new StimConList();
                for (int k=0; k<numBlocks; k++)
                    scl.Add(new SCLElement(k+1, 1));
                return scl;
            }

            foreach (Family f in families)
            {
                sclByFamily.Add(f.CreateStimConList(mode, numBlocks, !string.IsNullOrEmpty(decisionState)));
            }

            switch (order)
            {
                case Order.Interleave:
                    scl = Interleave(sclByFamily, numBlocks);
                    break;

                case Order.Sequential:
                    scl = Sequential(sclByFamily, numBlocks);
                    break;

                case Order.Alternate:
                    int nmax = families.Select(o => o.number).Max();
                    scl = Alternate(sclByFamily, nmax, numBlocks);
                    break;

                case Order.Random:
                    scl = Random(sclByFamily, numBlocks);
                    break;
            }

            return scl;
        }

        private StimConList Interleave(List<StimConList> byFamily, int nblocks)
        {
            StimConList scl = new StimConList();
            StimConList byBlock = new StimConList();

            for (int kb = 0; kb < nblocks; kb++)
            {
                byBlock.Clear();

                foreach (StimConList s in byFamily)
                {
                    byBlock.AddRange(s.FindAll(o => o.block == kb + 1));
                }

                int[] iorder = KLib.KMath.Permute(byBlock.Count);
                for (int k = 0; k < iorder.Length; k++)
                {
                    SCLElement sc = byBlock[iorder[k]];
                    sc.trial = k + 1;
                    scl.Add(sc);
                }
            }

            return scl;
        }

        private StimConList Sequential(List<StimConList> byFamily, int nblocks)
        {
            StimConList scl = new StimConList();
            StimConList byBlock = new StimConList();

            for (int kb = 0; kb < nblocks; kb++)
            {
                byBlock.Clear();

                foreach (StimConList s in byFamily)
                {
                    byBlock.AddRange(s.FindAll(o => o.block == kb + 1));
                }
                for (int k = 0; k < byBlock.Count; k++) byBlock[k].trial = k + 1;
                scl.AddRange(byBlock);
            }

            return scl;
        }

        private StimConList Alternate(List<StimConList> byFamily, int nmax, int nblocks)
        {
            StimConList scl = new StimConList();
            StimConList byBlock = new StimConList();

            for (int kb = 0; kb < nblocks; kb++)
            {
                byBlock.Clear();
                for (int k = 0; k < nmax; k++)
                {
                    foreach (StimConList s in byFamily)
                    {
                        var b = s.FindAll(o => o.block == kb + 1);
                        int idx = k % b.Count;
                        byBlock.Add(b[idx]);
                    }
                }
                for (int k = 0; k < byBlock.Count; k++) byBlock[k].trial = k + 1;
                scl.AddRange(byBlock);
            }

            return scl;
        }

        private StimConList Random(List<StimConList> byFamily, int nblocks)
        {
            StimConList scl = new StimConList();
            StimConList byBlock = new StimConList();

            for (int kb = 0; kb < nblocks; kb++)
            {
                byBlock.Clear();

                foreach (int i in KLib.KMath.Permute(byFamily.Count))
                {
                    byBlock.AddRange(byFamily[i].FindAll(o => o.block == kb + 1));
                }
                for (int k = 0; k < byBlock.Count; k++) byBlock[k].trial = k + 1;
                scl.AddRange(byBlock);
            }

            return scl;
        }
    }
}