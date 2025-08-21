using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Family
    {
        public string name = "";
        public int number = 0;
        public bool oneEach = true;
        public TrialType type = TrialType.NoResult;
        public VariableOrder order = VariableOrder.FullRandom;
        public List<Variable> variables = new List<Variable>();
        public string resultExpression = "";
        public string storeResultAs = "";

        [JsonIgnore]
        int _nx = -1;
        [JsonIgnore]
        int _ny = -1;
        [JsonIgnore]
        int _ntotal = -1;

        public Family() { }

        public Family(string name)
        {
            this.name = name;
        }

        public Family Clone()
        {
            Family f = new Family();

            f.name = this.name;
            f.number = this.number;
            f.oneEach = this.oneEach;
            f.type = this.type;
            f.order = this.order;

            f.variables = new List<Variable>();
            foreach (Variable v in this.variables) f.variables.Add(v.Clone());
            return f;
        }

        [JsonIgnore]
        public int NumTotal
        {
            get { return _ntotal; }
        }

        public StimConList CreateStimConList(Mode scheduleMode, int numBlocks, bool hasDecisionState)
        {
            Apply();

            StimConList scl = new StimConList();

            int nPerBlock = oneEach ? _ntotal : number;
            int[] iorder = SetOrder(order, _nx, _ny, nPerBlock, numBlocks);

            for (int k=0; k<iorder.Length; k++)
            {
                int i = iorder[k];

                SCLElement sc = new SCLElement();
                sc.group = name;

                int ix = Mathf.FloorToInt((float) i / _ny);
                int iy = i % _ny;

                sc.ix = ix;
                sc.iy = iy;
                sc.block = 1 + Mathf.FloorToInt((float)k / nPerBlock);

                foreach (Variable v in variables)
                {
                    float value = v.GetValue(ix, iy, k);
                    sc.propValPairs.Add(new PropValPair(v.PropertyName, value));
                }

                if (hasDecisionState)
                {
                    if (scheduleMode == Mode.Sequence)
                        sc.trialType = TrialType.GoNoGo;
                    else if (scheduleMode == Mode.CS)
                        sc.trialType = type;
                }

                scl.Add(sc);
            }

            return scl;
        }

        private int[] SetOrder(VariableOrder varOrder, int nx, int ny, int nperblock, int nblocks)
        {
            int idx = 0;
            int numPerX = 0;
            int[] iorder = new int[nx * ny];
            int ntotal = nperblock * nblocks;
            int[] finalOrder = new int[ntotal];
            int blkIndex = 0;

            switch (varOrder)
            {
                case VariableOrder.FullRandom:
                    if (nperblock >= nx * ny)
                    {
                        // more trials in each block than sequence elements (X, Y):
                        // randomize the whole mess, no balance applied
                        finalOrder = KLib.KMath.Permute(nx * ny, ntotal);
                    }
                    else
                    {
                        // fewer trials per block than sequence elements (X, Y):
                        // do equal number of X's per block
                        int numOfEachXPerBlock = Mathf.CeilToInt((float)nperblock / nx);
                        List<List<int>> ilinear = new List<List<int>>();

                        for (int kx = 0; kx < nx; kx++)
                        {
                            List<int> li = new List<int>(KLib.KMath.Permute(ny, nblocks * numOfEachXPerBlock));
                            for (int ki = 0; ki < li.Count; ki++) li[ki] += kx * ny;
                            ilinear.Add(li);
                        }

                        List<int> interleaved = new List<int>(nblocks * numOfEachXPerBlock * nx);

                        idx = 0;
                        int offset = 0;
                        while (idx < ntotal)
                        {
                            for (int kx = 0; kx < nx; kx++)
                            {
                                interleaved.InsertRange(idx, ilinear[kx].GetRange(offset, numOfEachXPerBlock));
                                idx += numOfEachXPerBlock;
                            }
                            offset += numOfEachXPerBlock;
                        }

                        List<int> final = new List<int>(ntotal);

                        for (int kb=0; kb < nblocks; kb++)
                        {
                            final.InsertRange(kb * nperblock, KLib.KMath.Permute(interleaved.GetRange(kb * nperblock, nperblock).ToArray()));
                        }

                        finalOrder = final.ToArray();
                    }
                    break;

                case VariableOrder.XSeqYSeq:
                    blkIndex = 0;
                    for (int kb = 0; kb < nblocks; kb++)
                    {
                        idx = 0;
                        numPerX = Mathf.CeilToInt((float)nperblock / nx);
                        iorder = new int[nx * numPerX];
                        for (int k = 0; k < nx; k++)
                        {
                            for (int j = 0; j < numPerX; j++) iorder[idx++] = k * ny + j % _ny;
                        }
                        for (int k = 0; k < nperblock; k++) finalOrder[blkIndex++] = iorder[k];
                    }

                    break;

                case VariableOrder.XSeqYRand:
                    blkIndex = 0;
                    for (int kb = 0; kb < nblocks; kb++)
                    {
                        idx = 0;
                        numPerX = Mathf.CeilToInt((float)nperblock / nx);
                        iorder = new int[nx * numPerX];
                        for (int k = 0; k < nx; k++)
                        {
                            foreach (int iy in KLib.KMath.Permute(ny, numPerX)) iorder[idx++] = k * ny + iy;
                        }
                        for (int k = 0; k < nperblock; k++) finalOrder[blkIndex++] = iorder[k];
                    }
                    break;

                case VariableOrder.XRandYSeq:
                    blkIndex = 0;
                    for (int kb = 0; kb < nblocks; kb++)
                    {
                        idx = 0;
                        numPerX = Mathf.CeilToInt((float)nperblock / nx);
                        iorder = new int[nx * numPerX];
                        foreach (int ix in KLib.KMath.Permute(nx))
                        {
                            for (int k = 0; k < numPerX; k++) iorder[idx++] = ix * ny + k % ny;
                        }
                        for (int k = 0; k < nperblock; k++) finalOrder[blkIndex++] = iorder[k];
                    }
                    break;

                case VariableOrder.XRandYRand:
                    blkIndex = 0;
                    for (int kb = 0; kb < nblocks; kb++)
                    {
                        idx = 0;
                        numPerX = Mathf.CeilToInt((float)nperblock / nx);
                        iorder = new int[nx * numPerX];
                        foreach (int ix in KLib.KMath.Permute(nx))
                        {
                            foreach (int iy in KLib.KMath.Permute(ny, numPerX)) iorder[idx++] = ix * ny + iy;
                        }
                        for (int k = 0; k < nperblock; k++) finalOrder[blkIndex++] = iorder[k];
                    }
                    break;
            }

            return finalOrder;
        }

        public void Apply()
        {
            float[] xvector = null;
            float[] yvector = null;

            _nx = _ny = 1;

            foreach (var v in variables.FindAll(x => x.dim != VarDimension.Ind))
            {
                v.EvaluateExpression(xvector, yvector);

                if (v.dim == VarDimension.X && xvector == null) xvector = v.Values;
                if (v.dim == VarDimension.Y && yvector == null) yvector = v.Values;

                if (v.dim == VarDimension.X && _nx == 1) _nx = v.Length;
                if (v.dim == VarDimension.Y && _ny == 1) _ny = v.Length;
            }

            _ntotal = _nx * _ny;

            if (oneEach)
            {
                number = _ntotal;
            }

            foreach (var v in variables.FindAll(x => x.dim == VarDimension.Ind))
            {
                v.EvaluateExpression(number);
            }

        }

    }
}