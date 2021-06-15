using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Object.UDP
{
    public static class GCByteWriter
    {
        public async static Task<byte[]> Write(GCCollection gCElements)
        {
            List<Byte> theBytes = new List<Byte>();

            theBytes.Add((byte)'G');
            theBytes.Add((byte)'e');
            theBytes.Add((byte)'o');
            theBytes.Add((byte)'m');
            theBytes.Add((byte)'e');
            theBytes.Add((byte)'t');
            theBytes.Add((byte)'r');
            theBytes.Add((byte)'y');

            char[] name = new char[16];
            int max = gCElements.Name.Length < 16 ? gCElements.Name.Length : 16;
            for (int s = 0; s < max; s += 1) {
                name[s] = gCElements.Name[s];
            }

            foreach (char ch in name) theBytes.Add((byte)ch);


            theBytes.Add((byte)gCElements.Count);

            for (int p = 0; p < gCElements.Count; p++)
            {
                theBytes.Add(gCElements[p].IsClosed == true ? (byte)1 : (byte)0);

                if (gCElements[p] is PointsElement pointCollection)
                {
                    theBytes.Add((byte)pointCollection.Count);
                    for (int i = 0; i < pointCollection.Count; i++)
                    {
                        theBytes.Add((byte)pointCollection[i].X);
                        theBytes.Add((byte)-pointCollection[i].Y);
                    }
                }
                else
                {
                    theBytes.Add((byte)0);
                }
            }

            return theBytes.ToArray();
        }
    
    }
}
