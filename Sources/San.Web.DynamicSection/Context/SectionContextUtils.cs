using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using San.Web.DynamicSection.Controls;

namespace San.Web.DynamicSection.Context
{
    public static class SectionContextUtils
    {
        private static readonly int FirstIdByte;
        private static readonly int IdBytesCount;
        
        private static readonly byte?[] Bytes;
        
        public static int BytesCount
        {
            get { return Bytes.Length; }
        }

        static SectionContextUtils()
        {
            Guid randomId = Guid.NewGuid();
            string idString = randomId.ToString();
            MvcSectionContext context = new MvcSectionContext(idString);
            string element = context.ToHtmlString().ToString();

            int startId = element.IndexOf(idString, StringComparison.InvariantCultureIgnoreCase);
            IdBytesCount = Encoding.UTF8.GetBytes(idString).Length;
            byte?[] idBytes = new byte?[IdBytesCount];

            List<byte?> prefix = Encoding.UTF8.GetBytes(element.Substring(0, startId)).Select(value => (byte?) value).ToList();
            FirstIdByte = prefix.Count;
            IEnumerable<byte?> postfix = Encoding.UTF8.GetBytes(element.Substring(startId + IdBytesCount)).Select(value => (byte?) value); 
            Bytes = prefix.Concat(idBytes).Concat(postfix).ToArray();
        }

        public static Dictionary<Guid, int> FindUsages(byte[] bytes, int offset, int count)
        {
            Dictionary<Guid, int> sectionIndexes = new Dictionary<Guid, int>();
            for (int i = offset; i < offset + count - BytesCount; i++)
            {
                for (int j = 0; j < BytesCount; j++)
                {
                    if (Bytes[j].HasValue && bytes[i + j] != Bytes[j])
                        break;
                    if (j == BytesCount - 1)
                    {
                        byte[] idBytes = new byte[IdBytesCount];
                        Array.Copy(bytes, i + FirstIdByte, idBytes, 0, idBytes.Length);
                        Guid sectionId;
                        if (Guid.TryParse(Encoding.UTF8.GetString(idBytes), out sectionId))
                        {
                            if (sectionIndexes.ContainsKey(sectionId))
                            {
                                sectionIndexes[sectionId] = i;
                            }
                            else
                            {
                                sectionIndexes.Add(sectionId, i);
                            }
                        }
                    }
                }
            }
            return sectionIndexes;
        }
    }
}